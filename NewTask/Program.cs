using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

var configBuilder = new ConfigurationBuilder();
configBuilder.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfigurationRoot config = configBuilder.Build();
var rabbitmqSettings = new RabbitmqSettings();
config.GetSection("RabbitMQ").Bind(rabbitmqSettings);

var factory = new ConnectionFactory
{
	HostName = rabbitmqSettings.HostName ?? throw new InvalidOperationException("RabbitMQ HostName is not configured."),
	UserName = rabbitmqSettings.UserName ?? throw new InvalidOperationException("RabbitMQ UserName is not configured."),
	Password = rabbitmqSettings.Password ?? throw new InvalidOperationException("RabbitMQ Password is not configured.")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "task_durable", durable: true, exclusive: false, autoDelete: false, arguments: null);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

var properties = new BasicProperties { Persistent = true };

await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "task_durable", mandatory: true, basicProperties: properties, body: body);

Console.WriteLine($" [x] Sent {message}");

static string GetMessage(string[] args)
{
	return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}