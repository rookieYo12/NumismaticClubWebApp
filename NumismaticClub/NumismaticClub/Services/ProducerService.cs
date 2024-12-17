using Confluent.Kafka;

namespace NumismaticClub.Services
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

        public void Produce(string topicName, string value)
        {
            _producer.Produce(topicName, new Message<Null, string> { 
                Value = value
            });
        }
    }
}
