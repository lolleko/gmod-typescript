// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var wikiData = WikiData.FromJson(jsonString);

namespace JsonType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class WikiData
    {
        [JsonProperty("enums", Required = Required.Always)]
        public List<Enum> Enums { get; set; }

        /// <summary>
        /// A dictionary of Contacts, indexed by unique ID
        /// </summary>
        [JsonProperty("functionCollections", Required = Required.Always)]
        public List<FunctionCollection> FunctionCollections { get; set; }

        [JsonProperty("structures", Required = Required.Always)]
        public List<Structure> Structures { get; set; }
    }

    public partial class Enum
    {
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("enumFields", Required = Required.Always)]
        public List<EnumField> EnumFields { get; set; }

        [JsonProperty("isMembersOnly", Required = Required.Always)]
        public bool IsMembersOnly { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }

    public partial class EnumField
    {
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public long Value { get; set; }
    }

    public partial class FunctionCollection
    {
        [JsonProperty("classFields", Required = Required.Always)]
        public List<Field> ClassFields { get; set; }

        [JsonProperty("collectionType", Required = Required.Always)]
        public CollectionType CollectionType { get; set; }

        [JsonProperty("customConstructor", Required = Required.Always)]
        public string CustomConstructor { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("examples", Required = Required.Always)]
        public List<Example> Examples { get; set; }

        [JsonProperty("extends", Required = Required.Always)]
        public string Extends { get; set; }

        [JsonProperty("functions", Required = Required.Always)]
        public List<Function> Functions { get; set; }

        [JsonProperty("isHook", Required = Required.Always)]
        public bool IsHook { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }

    public partial class Field
    {
        [JsonProperty("default", Required = Required.Always)]
        public string Default { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("isOptional", Required = Required.Always)]
        public bool IsOptional { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }
    }

    public partial class Example
    {
        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }
    }

    public partial class Function
    {
        [JsonProperty("accessModifier", Required = Required.Always)]
        public AccessModifier AccessModifier { get; set; }

        [JsonProperty("arguments", Required = Required.Always)]
        public List<Argument> Arguments { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("examples", Required = Required.Always)]
        public List<Example> Examples { get; set; }

        [JsonProperty("isConstructor", Required = Required.Always)]
        public bool IsConstructor { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("realm", Required = Required.Always)]
        public Realm Realm { get; set; }

        [JsonProperty("returns", Required = Required.Always)]
        public List<Return> Returns { get; set; }
    }

    public partial class Argument
    {
        [JsonProperty("default", Required = Required.Always)]
        public string Default { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("isOptional", Required = Required.Always)]
        public bool IsOptional { get; set; }

        [JsonProperty("isVarArg", Required = Required.Always)]
        public bool IsVarArg { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }
    }

    public partial class Return
    {
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }
    }

    public partial class Structure
    {
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("structureFields", Required = Required.Always)]
        public List<Field> StructureFields { get; set; }
    }

    public enum CollectionType { Class, Global, Library };

    public enum AccessModifier { Private, Protected, Public };

    public enum Realm { Client, ClientAndMenu, Menu, Server, Shared, SharedAndMenu };

    public partial class WikiData
    {
        public static WikiData FromJson(string json) => JsonConvert.DeserializeObject<WikiData>(json, JsonType.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WikiData self) => JsonConvert.SerializeObject(self, JsonType.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                CollectionTypeConverter.Singleton,
                AccessModifierConverter.Singleton,
                RealmConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            }
        };
    }

    internal class CollectionTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CollectionType) || t == typeof(CollectionType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "class":
                    return CollectionType.Class;
                case "global":
                    return CollectionType.Global;
                case "library":
                    return CollectionType.Library;
            }
            throw new Exception("Cannot unmarshal type CollectionType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (CollectionType)untypedValue;
            switch (value)
            {
                case CollectionType.Class:
                    serializer.Serialize(writer, "class");
                    return;
                case CollectionType.Global:
                    serializer.Serialize(writer, "global");
                    return;
                case CollectionType.Library:
                    serializer.Serialize(writer, "library");
                    return;
            }
            throw new Exception("Cannot marshal type CollectionType");
        }

        public static readonly CollectionTypeConverter Singleton = new CollectionTypeConverter();
    }

    internal class AccessModifierConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(AccessModifier) || t == typeof(AccessModifier?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "private":
                    return AccessModifier.Private;
                case "protected":
                    return AccessModifier.Protected;
                case "public":
                    return AccessModifier.Public;
            }
            throw new Exception("Cannot unmarshal type AccessModifier");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (AccessModifier)untypedValue;
            switch (value)
            {
                case AccessModifier.Private:
                    serializer.Serialize(writer, "private");
                    return;
                case AccessModifier.Protected:
                    serializer.Serialize(writer, "protected");
                    return;
                case AccessModifier.Public:
                    serializer.Serialize(writer, "public");
                    return;
            }
            throw new Exception("Cannot marshal type AccessModifier");
        }

        public static readonly AccessModifierConverter Singleton = new AccessModifierConverter();
    }

    internal class RealmConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Realm) || t == typeof(Realm?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "client":
                    return Realm.Client;
                case "client and menu":
                    return Realm.ClientAndMenu;
                case "menu":
                    return Realm.Menu;
                case "server":
                    return Realm.Server;
                case "shared":
                    return Realm.Shared;
                case "shared and menu":
                    return Realm.SharedAndMenu;
            }
            throw new Exception("Cannot unmarshal type Realm");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Realm)untypedValue;
            switch (value)
            {
                case Realm.Client:
                    serializer.Serialize(writer, "client");
                    return;
                case Realm.ClientAndMenu:
                    serializer.Serialize(writer, "client and menu");
                    return;
                case Realm.Menu:
                    serializer.Serialize(writer, "menu");
                    return;
                case Realm.Server:
                    serializer.Serialize(writer, "server");
                    return;
                case Realm.Shared:
                    serializer.Serialize(writer, "shared");
                    return;
                case Realm.SharedAndMenu:
                    serializer.Serialize(writer, "shared and menu");
                    return;
            }
            throw new Exception("Cannot marshal type Realm");
        }

        public static readonly RealmConverter Singleton = new RealmConverter();
    }
}
