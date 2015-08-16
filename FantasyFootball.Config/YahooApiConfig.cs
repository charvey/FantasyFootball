using System;
using System.IO;
using Newtonsoft.Json;

namespace FantasyFootball.Config
{
    public class YahooApiConfig
    {
        private const string filename = "Yahoo.json";

        private static YahooApiConfig instance;
        public static YahooApiConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    var filepath = DataDirectory.FilePath(filename);
                    var json = File.ReadAllText(filepath);
                    instance = JsonConvert.DeserializeObject<YahooApiConfig>(json);
                }
                return instance;
            }
        }

        private YahooApiConfig() { }

        ~YahooApiConfig()
        {
            File.WriteAllText(DataDirectory.FilePath(filename), JsonConvert.SerializeObject(this));
        }
        
        public string AccessToken;
        public int AccessTokenCalls;
        public DateTime AccessTokenExpires;
        public string ClientId;
        public string ClientSecret;
        public string RefreshToken;
    }
}
