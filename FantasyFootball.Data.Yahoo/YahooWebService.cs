using System;
using System.Collections.Generic;

using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using FantasyFootball.Config;

namespace FantasyFootball.Data.Yahoo
{
    public abstract class YahooWebService
    {
        protected HttpClient client = new HttpClient();
        private const string GetTokenUrl = "https://api.login.yahoo.com/oauth2/get_token";
        private readonly YahooApiConfig apiConfig = YahooApiConfig.Instance;
               
        private string BasicAuthorizationHeader
        {
            get
            {
                var auth = Encoding.ASCII.GetBytes(apiConfig.ClientId + ":" + apiConfig.ClientSecret);
                return "Basic " + Convert.ToBase64String(auth);
            }
        }

        protected string BearerAuthorizationHeader
        {
            get
            {
                return "Bearer " + AccessToken;
            }
        }

        private string RefreshToken
        {
            get
            {
                if (apiConfig.RefreshToken == null)
                {
                    //var command = string.Format("https://api.login.yahoo.com/oauth2/request_auth?client_id={0}&redirect_uri=oob&response_type=code", ClientId);
                    //Process.Start("explorer.exe", command);
                    var code = "bbtwn43";// Console.ReadLine();
                    var request = new HttpRequestMessage(HttpMethod.Post, GetTokenUrl);
                    request.Headers.Add("Authorization", BasicAuthorizationHeader);
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                        {"grant_type","authorization_code" },
                        {"redirect_uri","oob" },
                        {"code",code }
                    });
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<GetTokenResponse>(responseString);
                    apiConfig.AccessToken = responseObject.access_token;
                    apiConfig.AccessTokenExpires = DateTime.Now.AddSeconds(responseObject.expires_in);
                    apiConfig.AccessTokenCalls = 0;
                    apiConfig.RefreshToken = responseObject.refresh_token;
                }
                return apiConfig.RefreshToken;
            }
        }
                
        private string AccessToken
        {
            get
            {
                if (apiConfig.AccessToken == null || apiConfig.AccessTokenExpires < DateTime.Now)
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, GetTokenUrl);
                    request.Headers.Add("Authorization", BasicAuthorizationHeader);
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                        {"grant_type","refresh_token" },
                        {"redirect_uri","oob" },
                        {"refresh_token",RefreshToken }
                    });
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<GetTokenResponse>(responseString);
                    apiConfig.AccessToken = responseObject.access_token;
                    apiConfig.AccessTokenExpires = DateTime.Now.AddSeconds(responseObject.expires_in);
                    apiConfig.AccessTokenCalls = 0;
                    apiConfig.RefreshToken = responseObject.refresh_token;
                }
                return apiConfig.AccessToken;
            }
        }
    }
}
