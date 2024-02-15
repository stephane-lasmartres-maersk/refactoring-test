using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;

namespace RefactoringAssessment;

public class UserRequestHandler
{
    private string _dbConnectionString;
    private string _eventHubConnectionString;
    private string _eventHubName;

    public UserRequestHandler(string dbConnectString, string eventHubConnectionString, string eventHubName)
    {
        _dbConnectionString = dbConnectString;
        _eventHubConnectionString = eventHubConnectionString;
        _eventHubName = eventHubName;
    }

    public async void Handle(UserRequest userRequest)
    {
        // 1- validate
        if (userRequest == null)
        {
            throw new ArgumentNullException(nameof(userRequest));
        }

        if (string.IsNullOrWhiteSpace(userRequest.UserName))
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userRequest.UserName);
        }

        if (userRequest.Data is null)
        {
            ArgumentNullException.ThrowIfNull(userRequest.Data);
        }

        // 2 - process
        if(userRequest.UserType == UserType.Basic)
        {
            ProcessStandardUser();
        } 
        else if(userRequest.UserType == UserType.Premium)
        {
            ProcessPremiumUser();
        }
        else if (userRequest.UserType == UserType.Admin)
        {
            ProcessAdminUser();
        }
        else
        {
            throw new NotImplementedException();
        }

        // 3 - persist
        var options = new DbContextOptionsBuilder<UserRequestDbContext>()
            .UseSqlServer(_dbConnectionString)
            .Options;
        var context = new UserRequestDbContext(options);
        context.Add(userRequest);
        context.SaveChanges();


        // 4- produce outbound event
        var stringMessage = JsonConvert.SerializeObject(userRequest);

        var producerClient = new EventHubProducerClient(_eventHubConnectionString, _eventHubName);
        using (EventDataBatch eventBatch = await producerClient.CreateBatchAsync())
        {
            if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(stringMessage))))
            {
                throw new Exception($"Something goes wrong");
            }

            try
            {
                // Use the producer client to send the batch of events to the event hub
                await producerClient.SendAsync(eventBatch);
                Console.WriteLine($"User request sent: {stringMessage}.");
            }
            finally
            {
                await producerClient.DisposeAsync();
            }
        }
    }

    private void ProcessAdminUser()
    {
        // process admin user
    }

    private void ProcessPremiumUser()
    {
        // process premium user
    }

    private void ProcessStandardUser()
    {
        // process standard user
    }
}
