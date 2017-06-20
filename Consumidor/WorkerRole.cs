using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Consumidor
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        static CloudQueue cloudQueue;

        public override void Run()
        {
            Trace.TraceInformation("Consumidor is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("Consumidor has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Consumidor is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Consumidor has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=jcaula04puc;AccountKey=MQAYfJvM4bn/ZjuBZibvQu3pjDRpZUAN4Z7yv0GW92XLDQ1u/KBIv/AtuniFjStr9lqaDuTlu9ElpX9cAQT46w==;EndpointSuffix=core.windows.net";

            CloudStorageAccount cloudStorageAccount;

            if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                Trace.TraceInformation("Expected connection string 'DefaultEndpointsProtocol=https;AccountName=jcaula04puc;AccountKey=MQAYfJvM4bn/ZjuBZibvQu3pjDRpZUAN4Z7yv0GW92XLDQ1u/KBIv/AtuniFjStr9lqaDuTlu9ElpX9cAQT46w==;EndpointSuffix=core.windows.net' to be a valid Azure Storage Connection String.");
            }

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            cloudQueue = cloudQueueClient.GetQueueReference("testqueue");

            cloudQueue.CreateIfNotExists();

            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                var cloudQueueMessage = cloudQueue.GetMessage();

                if (cloudQueueMessage == null)
                {
                    return;
                }

                cloudQueue.DeleteMessage(cloudQueueMessage);

                Trace.TraceInformation("Message removed from queue.");
                await Task.Delay(5000);
            }
        }
    }
}
