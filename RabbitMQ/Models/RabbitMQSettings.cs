namespace EasyRabbitMQ.Net.RabbitMQ.Models;

public class RabbitMQSettings
{
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? ExchangeName { get; set; }
    public string? QueueName { get; set; }
    public string? VirtualHost { get; set; }
    public int Port { get; set; }
    public SslSettings Ssl { get; set; } = new SslSettings();
}

public class SslSettings
{
    public bool Enabled { get; set; }
    public string? ServerName { get; set; }
    public string? AcceptablePolicyErrors { get; set; }
}