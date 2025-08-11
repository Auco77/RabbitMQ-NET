using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost", UserName = "user01", Password = "auco^(x@user01" };
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

