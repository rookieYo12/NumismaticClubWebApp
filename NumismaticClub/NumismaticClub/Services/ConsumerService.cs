using Confluent.Kafka;
using NumismaticClub.Services;

namespace NumismaticClub.Services
{
    // Consumer realized like background service
    public class ConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly MessageProcessingService _messageProcessingService;

        public ConsumerService(MessageProcessingService messageProcessingService)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "coin-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _messageProcessingService = messageProcessingService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe("coin-topic");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken).Message.Value;

                    if (!string.IsNullOrWhiteSpace(consumeResult))
                    {
                        await _messageProcessingService.Process(consumeResult);
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
