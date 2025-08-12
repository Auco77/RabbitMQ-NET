using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Receive;
using System.Text;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfigurationRoot config = builder.Build();
var rabbitmqSettings = new RabbitmqSettings();
config.GetSection("RabbitMQ").Bind(rabbitmqSettings);

var _hostName = rabbitmqSettings.HostName ?? "<NULL>";
var _userName = rabbitmqSettings.UserName ?? "<NULL>";
var _pwd = rabbitmqSettings.Password ?? "<NULL>";

Console.WriteLine($"Try connect to {_hostName} @ {_userName} :: {_pwd}");

var factory = new ConnectionFactory { HostName = _hostName, UserName = _userName, Password = _pwd };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

//Note that we declare the queue here as well. Because we might start the consumer before the publisher,
//we want to make sure the queue exists before we try to consume messages from it.
await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine(" 🥱 Waiting for Messages");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
	var body = ea.Body.ToArray();
	var message = Encoding.UTF8.GetString(body);
	Console.WriteLine($" [x] Received {message}");
	return Task.CompletedTask;
};

await channel.BasicConsumeAsync("hello", autoAck:true, consumer: consumer);

Console.WriteLine(" Press [Enter] to exit.");
Console.ReadLine();