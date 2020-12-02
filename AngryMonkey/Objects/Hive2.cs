﻿namespace AngryMonkey.Objects
{
    public class Hive2
    {
        public string Path { get; set; }

        public string Title { get; set; }

        //! THE FOLLOWING ARE CUSTOM PROPERTIES.
        //! You can remove them, along with their usages, or set them to false in your hives.
        public bool ProcessProceduralFiles { get; set; }

        public bool ProcessXmlChangelogs { get; set; }
    }
}