namespace sjam.RabbitMQ.Models.RabbitMQModel
{
    public class RabbitMQConfigurationModel
    {
        public string Host { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5672;
        public bool Enabled { get; set; } = false;
    }
}
