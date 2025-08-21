using System.Text;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace CreditSimulationApi.Services
{
    public interface IEventHubJsonWriter
    {
        Task WriteJsonAsync(object data);
    }

    public class EventHubJsonWriter : IEventHubJsonWriter
    {
        private readonly string _connectionString;
        private readonly string _entityPath;

        public EventHubJsonWriter(IConfiguration configuration)
        {
            _connectionString = configuration["EventHub:ConnectionString"];
            _entityPath = configuration["EventHub:EntityPath"];
        }

        public async Task WriteJsonAsync(object data)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(_connectionString)
            {
                EntityPath = _entityPath
            };

            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            try
            {
                var jsonString = JsonConvert.SerializeObject(data);
                var eventData = new EventData(Encoding.UTF8.GetBytes(jsonString));
                await eventHubClient.SendAsync(eventData);
            }
            finally
            {
                await eventHubClient.CloseAsync();
            }
        }
    }
}