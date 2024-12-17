using Confluent.Kafka;
using UserApi.Services;

namespace UserApi.Services
{
    // Consumer realized like background service
    public class ConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly RequestProcessingService _requestProcessingService;
        private readonly UsersService _usersService;

        public ConsumerService(RequestProcessingService requestProcessingService, UsersService usersService)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "user-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _requestProcessingService = requestProcessingService;
            _usersService = usersService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(["create-user-topic", "update-user-topic"]);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    var messageValue = consumeResult.Message.Value;

                    if (!string.IsNullOrWhiteSpace(messageValue))
                    {
                        if (consumeResult.Topic == "create-user-topic")
                        {
                            await _requestProcessingService.CreateUser(messageValue);
                        }
                        else if (consumeResult.Topic == "update-user-topic")
                        {
                            await _requestProcessingService.UpdateRegisteredObjects(messageValue);
                        }
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
