using System;
using System.Threading.Tasks;
using System.Text.Json;
using Azure.Storage.Queues;

namespace StorageQueueApp
{


    class Program
    {
        static async Task Main(string[] args)
        {
            // Add code to create QueueClient and Storage Queue Here
            var connectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
            QueueClient queueClient = new QueueClient(connectionString, "newsqueue");

            await queueClient.CreateIfNotExistsAsync();

            bool exitProgram = false;
            while (exitProgram == false)
            {
                Console.WriteLine("What operation would you like to perform?");
                Console.WriteLine("  1 - Send message");
                Console.WriteLine("  2 - Peek at the next message");
                Console.WriteLine("  3 - Receive message");
                Console.WriteLine("  X - Exit program");

                ConsoleKeyInfo option = Console.ReadKey();
                Console.WriteLine();  // ReadKey does not got the the next line, so this does
                Console.WriteLine();  // Provide some whitespace between the menu and the action

                if (option.KeyChar == '1')
                    await SendMessageAsync(queueClient);
                else if (option.KeyChar == '2')
                    await PeekMessageAsync(queueClient);
                else if (option.KeyChar == '3')
                    await ReceiveMessageAsync(queueClient);
                else if (option.KeyChar == 'X')
                    exitProgram = true;
                else
                    Console.WriteLine("invalid choice");
            }
        }
     

        static async Task SendMessageAsync(QueueClient queueClient)
        {
            Console.WriteLine("Enter a headline: ");
            var headline = Console.ReadLine();

            Console.WriteLine("Enter a location: ");
            var location = Console.ReadLine();

            var article = new NewsArticle() { Headline = headline, Location = location };

            var message = JsonSerializer.Serialize(article);
            var  response = await queueClient.SendMessageAsync(message);
            var sendReceipt = response.Value;

            Console.WriteLine($"Message sent. Message Id={sendReceipt.MessageId} Expiration time={sendReceipt.ExpirationTime}");
            Console.WriteLine();
        }


        static async Task PeekMessageAsync(QueueClient queueClient)
        {
            var response = await queueClient.PeekMessageAsync();
            var message = response.Value;

            Console.WriteLine($"Message id: {message.MessageId}");
            Console.WriteLine($"Inserted on: {message.InsertedOn}");
            Console.WriteLine("We are only peeking at the message, so another consumer could dequeue this message");
        }


        static async Task ReceiveMessageAsync(QueueClient queueClient)
        {
            var response = await queueClient.ReceiveMessageAsync();
            var message = response.Value;

            Console.WriteLine($"Message id: {message.MessageId}");
            Console.WriteLine($"Inserted on: {message.InsertedOn}");
            Console.WriteLine($"Message (raw) : {message.Body}");

            var article = message.Body.ToObjectFromJson<NewsArticle>();
            Console.WriteLine("News Article");
            Console.WriteLine($"- Headline: {article.Headline}");
            Console.WriteLine($"- Location: {article.Location}");

            Console.WriteLine("The processing for this message is just printing it out, so now it will be deleted");
            await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            Console.WriteLine("Message deleted");
        }
    }


    class NewsArticle
    {
        public string Headline { get; set; }
        public string Location { get; set; }
    }


    enum QueueOperation
    {
        SendMessage,
        PeekMessage,
        ReceiveMessage
    }

}
