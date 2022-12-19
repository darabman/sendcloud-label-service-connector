using SendCloudApi.Net.Models;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendCloudApi.Net.Helpers
{
    /// <summary>
    /// Summary description for JsonHelper
    /// </summary>
    public static class JsonHelper
    {
        public static string Serialize<T>(T obj, string dateTimeFormat)
        {
            string retVal;
            var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { DateTimeFormat = new DateTimeFormat(dateTimeFormat), UseSimpleDictionaryFormat = true });
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                retVal = Encoding.UTF8.GetString(ms.ToArray());
            }
            return retVal;
        }

        public static T Deserialize<T>(string json, string dateTimeFormat)
        {
            T obj;
            var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { DateTimeFormat = new DateTimeFormat(dateTimeFormat), UseSimpleDictionaryFormat = true });
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                obj = (T)serializer.ReadObject(ms);
                ms.Close();
            }
            return obj;
        }

        public static Dictionary<string, T> DeserializeAsDictionary<T>(string json, string dateTimeFormat)
        {
            Dictionary<string, T> obj;
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, T>), new DataContractJsonSerializerSettings { DateTimeFormat = new DateTimeFormat(dateTimeFormat), UseSimpleDictionaryFormat = true });
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                obj = (Dictionary<string, T>)serializer.ReadObject(ms);
                ms.Close();
            }
            return obj;
        }

        public static Page<T> DeserializeAsPage<T>(string json, string dateTimeFormat)
        {
            Page<T> obj;

            var serializer = new DataContractJsonSerializer(typeof(Page<T>), new DataContractJsonSerializerSettings { DateTimeFormat = new DateTimeFormat(dateTimeFormat) });
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                obj = (Page<T>)serializer.ReadObject(ms);
                ms.Close();
            }
            return obj;
        }

        [DataContract]
        public class Page<T>
        {
            [DataMember(Name = "next")]
            public string? NextPage { get; set; } = null;

            [DataMember(Name = "previous")]
            public string PreviousPage { get; set; }

            [DataMember(Name = "data")]
            public T Data { get; set; }
        }
    }
}