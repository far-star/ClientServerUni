using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Press [Enter] to stop sending messages.");

        int numClients = 10; // Number of simulated clients
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var tasks = new Task[numClients];
        for (int i = 0; i < numClients; i++)
        {
            int clientId = i; // Capture the client ID for each task
            tasks[i] = Task.Run(async () =>
            {
                var publisher = new RabbitMQPublisher();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var reading = RandomDataGenerator.GenerateRandomMeterReading(); // Use client ID to generate client-specific data
                        publisher.SendMessage(reading);
                        Console.WriteLine($"Client {clientId} sent a message.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in client {clientId}: {ex.Message}");
                    }

                    await Task.Delay(15000); // Interval between messages
                }

                publisher.Close();
            }, cancellationToken);
        }

        // Wait for the user to stop the simulation
        Console.ReadLine();
        cancellationTokenSource.Cancel();

        await Task.WhenAll(tasks); // Wait for all clients to complete
        Console.WriteLine("Stopped sending messages.");
    }
}
