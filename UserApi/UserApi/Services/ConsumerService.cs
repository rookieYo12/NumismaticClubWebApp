using Confluent.Kafka;

namespace UserApi.Services
{
    // Consumer realized like background service
    public class ConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly RequestProcessingService _requestProcessingService;

        public ConsumerService(RequestProcessingService requestProcessingService)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "user-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _requestProcessingService = requestProcessingService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe("user-topic");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken).Message.Value;

                    if (!string.IsNullOrWhiteSpace(consumeResult))
                    {
                        await _requestProcessingService.Process(consumeResult);
                    }
                }
                catch (OperationCanceledException) // When a cancel signal is received 
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Consume error: {ex.Error.Reason}");

                    if (ex.Error.IsFatal)
                    {
                        break;
                    }
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close(); // Commit offsets and leave the group cleanly
            _consumer.Dispose();

            base.Dispose();
        }
    }
}
