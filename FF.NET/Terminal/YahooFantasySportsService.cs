﻿
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace yahoo.fantastysports
{
    public static class YahooFantasySportsService
    {
        public static readonly ServiceProviderDescription Description = new ServiceProviderDescription
        {
            RequestTokenEndpoint = new MessageReceivingEndpoint("https://api.login.yahoo.com/oauth/v2/get_request_token", HttpDeliveryMethods.PostRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://api.login.yahoo.com/oauth/v2/request_auth", HttpDeliveryMethods.GetRequest),
            AccessTokenEndpoint = new MessageReceivingEndpoint("https://api.login.yahoo.com/oauth/v2/get_token", HttpDeliveryMethods.GetRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] {
                new HmacSha1SigningBindingElement()
            }
        };
    }
}