using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Resto.Front.Api.CustomerScreen.Settings
{
    [XmlRoot("Config")]
    public sealed class CustomerScreenConfig
    {
        private const string ConfigFileName = @"CustomerScreen.front.config.xml";

        public CustomerScreenConfig()
        {
            SupportedExtensions = new List<string>();
        }
        private static string PathToConfigDir;
        private static string FilePath
        {
            get { return Path.GetFullPath(Path.Combine(PathToConfigDir, ConfigFileName)); }
        }

        [XmlElement]
        public string PathToPlaylistFolder { get; set; }

        [XmlElement]
        public List<string> SupportedExtensions { get; set; }

        [XmlElement]
        public string WelcomeText { get; set; }

        [XmlElement]
        public string LogoImagePath { get; set; }

        private static CustomerScreenConfig instance;
        public static CustomerScreenConfig Instance
        {
            get
            {
                return instance ?? (instance = Load());
            }
        }

        private static CustomerScreenConfig Load()
        {
            try
            {
                PluginContext.Log.InfoFormat("Loading Customer Screen config from {0}", FilePath);
                using (var stream = new FileStream(FilePath, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    return (CustomerScreenConfig)new XmlSerializer(typeof(CustomerScreenConfig)).Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                PluginContext.Log.Error("Failed to load Customer Screen config; using default settings.", e);
            }
            var config = new CustomerScreenConfig();
            config.SupportedExtensions.AddRange(new[] { "*.avi", "*.wmv" });
            config.PathToPlaylistFolder = string.Empty;
            config.LogoImagePath = string.Empty;
            config.Save();
            return config;
        }

        public void Save()
        {
            try
            {
                PluginContext.Log.InfoFormat("Saving Customer Screen config to {0}", FilePath);
                using (Stream stream = new FileStream(FilePath, FileMode.Create))
                {
                    new XmlSerializer(typeof(CustomerScreenConfig)).Serialize(stream, this);
                }
            }
            catch (Exception e)
            {
                PluginContext.Log.Error("Failed to save Customer Screen config.", e);
            }
        }

        public static void Init(string pathToConfigDir)
        {
            PathToConfigDir = pathToConfigDir;
        }
    }
}
