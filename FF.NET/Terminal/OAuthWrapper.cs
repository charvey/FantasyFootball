using DotNetOpenAuth.OAuth.ChannelElements;
using System.Collections.Generic;

namespace yahoo.fantastysports
{
    public class OAuthWrapper
    {
        public DotNetOpenAuth.OAuth.DesktopConsumer consumer { get; set; }

        private string RequestToken = "";
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public OAuthWrapper(string consumer_key, string consumer_secret)
        {
            this.ConsumerKey = consumer_key;
            this.ConsumerSecret = consumer_secret;

            consumer = new DotNetOpenAuth.OAuth.DesktopConsumer(
                YahooFantasySportsService.Description,
                new InMemoryTokenManager(ConsumerKey, ConsumerSecret));
            return;
        }

        public string BeginAuth()
        {
            var requestArgs = new Dictionary<string, string>();
            return this.consumer.RequestUserAuthorization(requestArgs, null, out this.RequestToken).AbsoluteUri;
        }

        public string CompleteAuth(string verifier)
        {
            var response = this.consumer.ProcessUserAuthorization(this.RequestToken, verifier);
            return response.AccessToken;
        }

        public IConsumerTokenManager TokenManager
        {
            get
            {
                return (IConsumerTokenManager)consumer.TokenManager;
            }
        }
    }
}