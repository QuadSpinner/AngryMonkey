#region Header

// 2020
// 12
// 01
// 10:30 PM
// 

#endregion

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AngryMonkey
{
    public partial class Processor
    {
        private void AppendXML(string @params, string title)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();


            string[] lines = File.ReadAllLines(@params);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("^"))
                    continue;
            }

            using FileStream fs = new FileStream(Nav.RootPath + "\\XML\\" + title + ".xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            new XmlSerializer(fields.GetType()).Serialize(fs, fields);
        }
    }
}