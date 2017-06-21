using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {

        // GET api/values
        public IEnumerable<string> Get()
        {

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=jcaula04puc;AccountKey=MQAYfJvM4bn/ZjuBZibvQu3pjDRpZUAN4Z7yv0GW92XLDQ1u/KBIv/AtuniFjStr9lqaDuTlu9ElpX9cAQT46w==;EndpointSuffix=core.windows.net";

            CloudStorageAccount cloudStorageAccount;

            if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                Trace.TraceInformation("Expected connection string 'DefaultEndpointsProtocol=https;AccountName=jcaula04puc;AccountKey=MQAYfJvM4bn/ZjuBZibvQu3pjDRpZUAN4Z7yv0GW92XLDQ1u/KBIv/AtuniFjStr9lqaDuTlu9ElpX9cAQT46w==;EndpointSuffix=core.windows.net' to be a valid Azure Storage Connection String.");
            }

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            cloudQueue = cloudQueueClient.GetQueueReference("apiqueue");

            cloudQueue.CreateIfNotExists();

            return;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
