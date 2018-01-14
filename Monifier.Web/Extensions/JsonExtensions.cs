using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Monifier.Web.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings);
        }

        public static T FromJson<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}