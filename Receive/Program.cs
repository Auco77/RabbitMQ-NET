using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost", UserName = "user01", Password = "auco^(x@user01" };
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
