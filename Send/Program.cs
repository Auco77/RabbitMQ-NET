using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Send;
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

await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

var hasMoreMessage = true;

while (hasMoreMessage)
{
	Console.WriteLine("Write a Message to Send");
	var newMsg = Console.ReadLine();

	if (newMsg != null)
	{
		var msgBody = Encoding.UTF8.GetBytes($"{DateTime.Now:HH:mm:ss} => {newMsg}");
		await channel.BasicPublishAsync(exchange: String.Empty, routingKey: "hello", body: msgBody);
		Console.WriteLine("Message Sent");
	}

	Console.WriteLine("Do you want send more message (Y/n)");
	hasMoreMessage = Console.ReadLine()?.ToUpper() != "N";
}

Console.WriteLine(" Press [Enter] to exit");
Console.ReadLine();

