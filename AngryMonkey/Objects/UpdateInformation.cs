using System;
using System.Xml.Serialization;

namespace Gaea.Internals.Online
{
    [Serializable]
    public sealed class UpdateManifest
    {
        [XmlAttribute]
        public string Version { get; set; }

        [XmlAttribute]
        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        [XmlAttribute]
        public long Size { get; set; }

        [XmlAttribute]
        public long PatchSize { get; set; }

        [XmlAttribute]
        public string ReleaseType { get; set; }

        [XmlAttribute]
        public DateTime ReleaseDate { get; set; }

        [XmlAttribute]
        public string Filename { get; set; }

        [XmlAttribute]
        public string URL { get; set; }

        [XmlAttribute]
        public string PatchURL { get; set; }
    }
}