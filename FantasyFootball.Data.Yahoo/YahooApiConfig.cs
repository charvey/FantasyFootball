using Newtonsoft.Json;
using System;
using System.IO;

namespace FantasyFootball.Data.Yahoo
{
    public class YahooApiConfig
    {
        private string filepath;

        public static YahooApiConfig FromFile(string filepath)
        {
            var json = File.ReadAllText(filepath);
            var instance = JsonConvert.DeserializeObject<YahooApiConfig>(json);
            instance.filepath = filepath;
            return instance;
        }

        private YahooApiConfig() { }

        ~YahooApiConfig()
        {
            File.WriteAllText(filepath, JsonConvert.SerializeObject(this));
        }

        public string AccessToken;
        public int AccessTokenCalls;
        public DateTime AccessTokenExpires;
        public string ClientId;
        public string ClientSecret;
        public string RefreshToken;
    }
}
