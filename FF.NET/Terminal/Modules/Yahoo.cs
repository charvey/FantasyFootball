using DotNetOpenAuth.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using yahoo.fantastysports;

namespace Terminal.Modules
{
    public class Yahoo : Module
    {
        private const string CONSUMER_KEY = "dj0yJmk9WW1oeklyOGR6NGZJJmQ9WVdrOWMxVm5RelZVTldjbWNHbzlNelkwTURNeU56WXkmcz1jb25zdW1lcnNlY3JldCZ4PTA0";
        private const string CONSUMER_SECRET = "8a9c49b8d5fdd8e8b5b2527217cc72312ae027ac";
        private string token;
        private OAuthWrapper wrapper;

        protected override List<string> Dependencies
        {
            get { return new List<string>(); }
        }

        protected override void Initialize()
        {
            wrapper = new OAuthWrapper(CONSUMER_KEY, CONSUMER_SECRET);

            if (File.Exists("TOKEN"))
            {
            }
            else
            {
                Process.Start(wrapper.BeginAuth());
                Console.WriteLine("Enter pin:");
                token = wrapper.CompleteAuth(Console.ReadLine());
                //File.WriteAllText("TOKEN", token);
            }
        }

        public string GetCall(string url)
        {
            MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint(url, HttpDeliveryMethods.GetRequest);
            var incomingResponse = wrapper.consumer.PrepareAuthorizedRequestAndSend(endpoint, token);
            string responseString = new StreamReader(incomingResponse.ResponseStream).ReadToEnd();
            return responseString;
        }
    }
}
