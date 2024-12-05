using Confluent.Kafka;

namespace UserApi.Services
{
    // Kafka producer
    public class ProducerService
    {
        private readonly IProducer<Null, string> _producer;

        public ProducerService()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void Produce(string value)
        {
            _producer.Produce("coin-topic", new Message<Null, string> { 
                Value = value
            });
        }
    }
}
