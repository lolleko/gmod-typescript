using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonType
{
    public static class Extension
    {
        public static T Clone<T>(T obj) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, JsonType.Converter.Settings), JsonType.Converter.Settings);
        public static string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, JsonType.Converter.Settings);
        public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, JsonType.Converter.Settings);
        public static string SerializeEnum<T>(T obj) => JsonConvert.SerializeObject(new T[]{obj}, JsonType.Converter.Settings).Replace("[\"", "").Replace("\"]", "");
        public static T DeserializeEnum<T>(string json) => JsonConvert.DeserializeObject<T[]>($"[\"{json.ToLower()}\"]", JsonType.Converter.Settings)[0];
    }
}

