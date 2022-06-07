using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;

namespace RefactoringAssessment
{
    public class UserRequestHandler
    {
        private UserRequestDbContext _context;
        private EventHubProducerClient _producerClient;

        public UserRequestHandler(string dbConnectString, string eventHubConnectionString, string eventHubName)
        {
            var options = new DbContextOptionsBuilder<UserRequestDbContext>()
                .UseSqlServer(dbConnectString)
                .Options;

            _context = new UserRequestDbContext(options);

            _producerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName);

        }

        public async void Handle(UserRequest userRequest)
        {
            // 1- validate user input
            if (userRequest == null)
            {
                throw new ArgumentNullException(nameof(userRequest));
            }

            if (string.IsNullOrWhiteSpace(userRequest.UserName))
            {
                throw new ArgumentNullException(nameof(userRequest.UserName));
            }

            if (userRequest.Data is null)
            {
                throw new ArgumentNullException(nameof(userRequest.Data));
            }

            // 2- store to database
            _context.Add(userRequest);
            _context.SaveChanges();


            // 3- produce outbound event
            var stringMessage = JsonConvert.SerializeObject(userRequest);

            using (EventDataBatch eventBatch = await _producerClient.CreateBatchAsync())
            {
                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(stringMessage))))
                {
                    throw new Exception($"Something went wrong");
                }

                try
                {
                    // Use the producer client to send the batch of events to the event hub
                    await _producerClient.SendAsync(eventBatch);
                    Console.WriteLine($"User request sent: {stringMessage}.");
                }
                finally
                {
                    await _producerClient.DisposeAsync();
                }
            }
        }
    }
}
