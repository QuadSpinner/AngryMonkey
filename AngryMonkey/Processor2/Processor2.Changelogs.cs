#region Header

// 2020
// 12
// 03
// 3:27 AM
// 

#endregion

using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Gaea.Internals.Online;

namespace AngryMonkey
{
    public partial class Processor2
    {
        private void PreprocessChangelogs(string dir)
        {
            string[] xmlFiles = Directory.GetFiles(dir, "*.update", SearchOption.AllDirectories);

            XmlSerializer xs = new XmlSerializer(typeof(UpdateManifest));

            foreach (string x in xmlFiles)
            {
                UpdateManifest manifest;
                using (var fs = new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    manifest = xs.Deserialize(fs) as UpdateManifest;
                }

                string version = Version.Parse(manifest.Version).ToString(4);
                string versionSafe = version.Replace(".", "_");


                string changelog = $"{Path.GetDirectoryName(x)}\\{version}.md";

                if (!Force && File.Exists(changelog))
                    continue;

                StringBuilder md = new StringBuilder();

                md.AppendLine("---");
                md.AppendLine("uid: gaea_" + versionSafe);
                md.AppendLine("title: Gaea " + version);
                md.AppendLine("---\n\n");
                md.AppendLine($"**Released on {manifest.ReleaseDate:dd MMMM yyyy}**\n");
                md.AppendLine($"<a href=\"{manifest.URL}\">Download {manifest.Size / 1024.0 / 1024.0:F}MB</a> <br>");
                md.AppendLine("\n");
                md.AppendLine("<div class=\"release-note\">\n");
                md.AppendLine(manifest.FullDescription);
                md.AppendLine("</div>");

                File.WriteAllText(changelog, md.ToString());
            }
        }
    }
}