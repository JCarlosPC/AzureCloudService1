using System;
using System.Configuration;
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

namespace Produtor
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        static CloudQueue cloudQueue1;
        static CloudQueue cloudQueue2;
        int count = 0;

        public override void Run()
        {
            Trace.TraceInformation("Produtor is running");

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

            Trace.TraceInformation("Produtor has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Produtor is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Produtor has stopped");
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
            cloudQueue1 = cloudQueueClient.GetQueueReference("apiqueue");
            cloudQueue2 = cloudQueueClient.GetQueueReference("testqueue");

            cloudQueue1.CreateIfNotExists();
            cloudQueue2.CreateIfNotExists();

            string MessageText = " - Here is a nice message!";

            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                //Adicionando a mensagem na apiqueue
                var message1 = new CloudQueueMessage(count + MessageText); // concatenando contador na mensagem para facilitar visualização
                cloudQueue1.AddMessage(message1);
                count++;

                await Task.Delay(5000);

                //Removendo mensagem da apiqueue
                var cloudQueueMessage = cloudQueue1.GetMessage();

                if (cloudQueueMessage == null)
                {
                    return;
                }

                cloudQueue1.DeleteMessage(cloudQueueMessage);

                //Adicionando a mensagem na testqueue
                var message2 = new CloudQueueMessage(cloudQueueMessage.AsString);
                cloudQueue2.AddMessage(message2);

                Trace.TraceInformation("Message added to queue.");
            }
        }
    }
}
