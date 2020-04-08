using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace simple
{
    public static class keda
    {
        [FunctionName("keda")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string timestamp = DateTime.Now.ToString();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var request = new Request(){
                RequestId = Guid.NewGuid().ToString(),
                RequestTimeStamp = timestamp,
                RequesterName = name
            };

            Thread.Sleep(200);

            return name != null
                ? (ActionResult)new OkObjectResult(request)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        public class Request 
        { 
            public string RequestId { get; set; }
            public string RequestTimeStamp { get; set; }
            public string RequesterName { get; set; }

        }
    }
}
