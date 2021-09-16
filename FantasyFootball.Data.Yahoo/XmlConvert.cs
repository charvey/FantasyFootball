using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo
{
    public static class XmlConvert
    {
        public static T Deserialize<T>(string xml)
        {
            var regex = new Regex("xmlns=\".+\"");
            var serializer = new XmlSerializer(typeof(T), "");
            using (var reader = new StringReader(regex.Replace(xml, "")))
                return (T)serializer.Deserialize(reader);
        }
    }
}
