using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

var configBuider = new ConfigurationBuilder();
configBuider.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = configBuider.Build();
var rabbitmqSettings = new RabbitmqSettings();
config.GetSection("RabbitMQ").Bind(rabbitmqSettings);

var factory = new ConnectionFactory
{
	HostName = rabbitmqSettings.HostName ?? throw new InvalidOperationException("RabbitMQ Hostname is not configured"),
	UserName = rabbitmqSettings.UserName ?? throw new InvalidOperationException("RabbitMQ Username is not configured"),
	Password = rabbitmqSettings.Password ?? throw new InvalidOperationException("RabbitMQ Password is not configured")
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);
await channel.BasicPublishAsync(exchange: "logs", routingKey: "", body: body);

Console.WriteLine($" 📨 Sent {message}");

Console.WriteLine(" Press [Enter] to exit");
Console.ReadLine();

static string GetMessage(string[] args) => ((args.Length > 0) ? string.Join(" ", args) : "info: Hello World!");