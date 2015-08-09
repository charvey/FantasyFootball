using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace FantasyFootball.Data.Yahoo
{
    public abstract class YahooWebService
    {
        protected HttpClient client = new HttpClient();
        private const string GetTokenUrl = "https://api.login.yahoo.com/oauth2/get_token";

        private string clientId;
        protected string ClientId
        {
            get
            {
                if (clientId == null)
                    clientId = File.ReadAllText("ClientId");
                return clientId;
            }
        }

        private string clientSecret;
        protected string ClientSecret
        {
            get
            {
                if (clientSecret == null)
                    clientSecret = File.ReadAllText("ClientSecret");
                return clientSecret;
            }
        }

        private string refreshToken;
        protected string RefreshToken
        {
            get
            {
                if (refreshToken == null && File.Exists("RefreshToken"))
                    refreshToken = File.ReadAllText("RefreshToken");
                if (refreshToken == null)
                {
                    //var command = string.Format("https://api.login.yahoo.com/oauth2/request_auth?client_id={0}&redirect_uri=oob&response_type=code", ClientId);
                    //Process.Start("explorer.exe", command);
                    var code = "bbtwn43";// Console.ReadLine();
                    var request = new HttpRequestMessage(HttpMethod.Post, GetTokenUrl);
                    var auth = Encoding.ASCII.GetBytes(ClientId + ":" + ClientSecret);
                    request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(auth));
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                        {"grant_type","authorization_code" },
                        {"redirect_uri","oob" },
                        {"code",code }
                    });
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<GetTokenResponse>(responseString);
                    AccessToken = responseObject.access_token;
                    AccessTokenExpires = DateTime.Now.AddSeconds(responseObject.expires_in);
                    RefreshToken = responseObject.refresh_token;
                }
                return refreshToken;
            }
            set
            {
                File.WriteAllText("RefreshToken", value.ToString());
                refreshToken = value;
            }
        }

        private DateTime? accessTokenExpires;
        private DateTime AccessTokenExpires
        {
            get
            {
                if (accessTokenExpires == null && File.Exists("AccessTokenExpires"))
                    accessTokenExpires = DateTime.Parse(File.ReadAllText("AccessTokenExpires"));
                if (accessTokenExpires == null)
                    accessTokenExpires = DateTime.MinValue;
                return accessTokenExpires.Value;
            }
            set
            {
                File.WriteAllText("AccessTokenExpires", value.ToString());
                accessTokenExpires = value;
            }
        }

        private string accessToken;
        protected string AccessToken
        {
            get
            {
                if (accessToken == null && File.Exists("AccessToken"))
                    accessToken = File.ReadAllText("AccessToken");
                if (accessToken == null || AccessTokenExpires < DateTime.Now)
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, GetTokenUrl);
                    var auth = Encoding.ASCII.GetBytes(ClientId + ":" + ClientSecret);
                    request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(auth));
                    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                        {"grant_type","refresh_token" },
                        {"redirect_uri","oob" },
                        {"refresh_token",RefreshToken }
                    });
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<GetTokenResponse>(responseString);
                    AccessToken = responseObject.access_token;
                    AccessTokenExpires = DateTime.Now.AddSeconds(responseObject.expires_in);
                    RefreshToken = responseObject.refresh_token;
                }
                return accessToken;
            }
            set
            {
                File.WriteAllText("AccessToken", value.ToString());
                accessToken = value;
            }
        }
    }
}
