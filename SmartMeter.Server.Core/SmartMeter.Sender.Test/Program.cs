using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var publisher = new RabbitMQPublisher();

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        Console.WriteLine("Press [Enter] to stop sending messages.");

        // Send messages every 15 seconds
        Task sendingTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var reading = RandomDataGenerator.GenerateRandomMeterReading();
                publisher.SendMessage(reading);
                await Task.Delay(15000);  // Adjust frequency as needed
            }
        }, cancellationToken);

        Console.ReadLine();
        cancellationTokenSource.Cancel();

        await sendingTask; // Wait for sending to complete
        publisher.Close();
        Console.WriteLine("Stopped sending messages.");
    }
}
