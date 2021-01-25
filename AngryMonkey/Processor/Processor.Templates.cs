using System.Collections.Generic;
using System.IO;
using System.Text;
using AngryMonkey.Objects;

namespace AngryMonkey
{
    public partial class Processor
    {
        public void InitializeTemplates()
        {
            Write("   Loading templates...");

            // Load templates into Dictionaries
            string[] files = Directory.GetFiles(Source + "\\_template", "*.html");
            foreach (string file in files)
            {
                templates.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
            }

            files = Directory.GetFiles(Source + "\\_includes", "*.md");
            foreach (string file in files)
            {
                includes.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
            }

            //StringBuilder navHtml = new();
            //foreach (Hive hive in hives)
            //{
            //    navHtml.AppendLine($"<li><a class=\"top-nav\" href=\"/{hive.Path}/\">{hive.Title}</a></li>");
            //}

            CheckTemplateHash();

            //templates[mainTemplate] = templates[mainTemplate].Replace("{{TOPNAV}}", navHtml.ToString());

            foreach ((string varName, string varValue) in variables)
            {
                foreach ((string key, string value) in templates)
                    templates[key] = value.Replace("{{" + varName + "}}", varValue);
                
                foreach ((string key, string value) in includes)
                    templates[key] = value.Replace("{{" + varName + "}}", varValue);
            }

            OK();
        }

        private void CheckTemplateHash()
        {
            // Get hashes for the variables
            string md5variables = GetMD5(File.ReadAllText(Source + "\\variables.json"));
            
            // Get hashes for the templates and fragments
            string md5templates = CreateMd5ForFolder(Source + "\\_template");
            string md5includes = CreateMd5ForFolder(Source + "\\_includes");

            // If the hashes don't match, force rebuild of all pages
            if (hashes.ContainsKey("variables.json"))
            {
                if (md5variables != hashes["variables.json"])
                {
                    Force = true;
                    hashes["variables.json"] = md5variables;
                }
            }
            else
            {
                hashes.Add("variables.json", md5variables);
            }

            if (hashes.ContainsKey("_template"))
            {
                if (md5templates != hashes["_template"])
                {
                    Force = true;
                    hashes["_template"] = md5templates;
                }
            }
            else
            {
                hashes.Add("_template", md5templates);
            }

            if (hashes.ContainsKey("_includes"))
            {
                if (md5includes != hashes["_includes"])
                {
                    Force = true;
                    hashes["_includes"] = md5includes;
                }
            }
            else
            {
                hashes.Add("_includes", md5includes);
            }
        }
    }
}