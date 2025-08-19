using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var configBuilder = new ConfigurationBuilder();
configBuilder.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = configBuilder.Build();
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

await channel.ExchangeDeclareAsync(exchange: "logs", ExchangeType.Fanout);

QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
string queueName = queueDeclareResult.QueueName;

await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: "");

Console.WriteLine(" 🥱 Waiting for Logs...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) => {
	byte[] body = ea.Body.ToArray();
	var message = Encoding.UTF8.GetString(body);
	Console.WriteLine($"\t📩 {message}");
	return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queueName, autoAck:true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();