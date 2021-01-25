using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using N = Newtonsoft.Json.NullValueHandling;

namespace AngryMonkey
{
    public partial class Changelog
    {
        [J("name")]
        public string Name { get; set; }

        [J("notes")]
        public string[] Notes { get; set; }

        [J("pub_date")]
        public string PubDate { get; set; }

        [J("version")]
        public string Version { get; set; }

        [J("url", NullValueHandling = N.Ignore)]
        public Uri Url { get; set; }
    }

    public partial class Changelog
    {
        public static Changelog[] FromJson(string json) =>
            JsonConvert.DeserializeObject<Changelog[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Changelog[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new()
                                                                 {
                                                                           MetadataPropertyHandling =
                                                                               MetadataPropertyHandling.Ignore,
                                                                           DateParseHandling = DateParseHandling.None,
                                                                           Converters =
                                                                           {
                                                                               new IsoDateTimeConverter
                                                                               {
                                                                                   DateTimeStyles =
                                                                                       DateTimeStyles.AssumeUniversal
                                                                               }
                                                                           }
                                                                       };
    }
}