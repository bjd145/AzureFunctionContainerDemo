using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace EventGridFunctionsContainerDemo
{
    public class EventTracker : TableEntity
    {
        public string id { get; set; }
        public string BlobUrl { get; set; }
        public string EventType { get; set; }

        public EventTracker(string RequestId)
        {
            PartitionKey = "Events";
            RowKey = RequestId;
        }

    }

    public static class EventGridDemo
    {
        [FunctionName("eventgriddemo")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            string response = "";
            string requestContent = await req.Content.ReadAsStringAsync();

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=bjddemosa002;AccountKey=||==;EndpointSuffix=core.windows.net";
            var tableName = "EventsTracker";

            try {
                EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();
                EventGridEvent[] eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestContent);

                foreach (EventGridEvent eventGridEvent in eventGridEvents)
                {
                    if (eventGridEvent.Data is SubscriptionValidationEventData)
                    {
                        var eventData = (SubscriptionValidationEventData)eventGridEvent.Data;
                        var responseData = new SubscriptionValidationResponse()
                        {
                            ValidationResponse = eventData.ValidationCode
                        };

                        return req.CreateResponse(HttpStatusCode.OK, responseData);
                    }
                    else if (eventGridEvent.Data is StorageBlobCreatedEventData)
                    {
                        var eventData = (StorageBlobCreatedEventData)eventGridEvent.Data;
                        var tracker = new EventTracker(eventData.RequestId) {
                            id = eventData.ClientRequestId,
                            BlobUrl = eventData.Url,
                            EventType = eventGridEvent.EventType
                        };

                        var _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                        var _client = _cloudStorageAccount.CreateCloudTableClient();
                        var _table = _client.GetTableReference(tableName);
                        await _table.CreateIfNotExistsAsync();
                        
                        TableOperation insertOperation = TableOperation.Insert(tracker);
                        await _table.ExecuteAsync(insertOperation);

                        response = $"Written event to Table Storage";
                    }
                    else {
                        return req.CreateResponse(HttpStatusCode.BadRequest, $"Unknown EventGrid event type - {requestContent}");
                    }
                }
                return req.CreateResponse(HttpStatusCode.OK, response);
            }
            catch( Exception e ) {
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Failed to parse -  {e.Message}. {e.InnerException}");
            }
        }
    }
}
