using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.SamplePlugin.Config;

internal static class ConfigService
{
    private const string ConfigName = "config.xml";
    private static readonly string ConfigDir = PluginContext.Integration.GetConfigsDirectoryPath();
    private static readonly string ConfigPath = Path.Combine(ConfigDir, ConfigName);
    private static readonly XmlSerializer Serializer = new(typeof(Config));

    [NotNull]
    public static Config Config { get; } = LoadOrCreate();

    #region
    [NotNull]
    private static Config LoadOrCreate()
    {
        if (!File.Exists(ConfigPath))
        {
            PluginContext.Log.Info($"File “{ConfigPath}” doesn't exist. Creating default...");
            return Save(Config.CreateDefault());
        }

        string xml;
        try
        {
            xml = File.ReadAllText(ConfigPath);
        }
        catch (Exception e)
        {
            PluginContext.Log.Error($"Couldn't read file “{ConfigPath}”. Creating default...", e);
            return Save(Config.CreateDefault());
        }

        PluginContext.Log.Info($"File “{ConfigPath}” to load:{Environment.NewLine}{xml}");

        try
        {
            return Deserialize(xml);
        }
        catch (Exception e)
        {
            var configBakName = $"{ConfigName}.{DateTime.Now:yyyy-MM-ddTHH-mm-ss.fffffff}.bak";
            var configBakPath = Path.Combine(ConfigDir, configBakName);
            PluginContext.Log.Error($"Couldn't to deserialize config. Renaming broken file to “{configBakName}” and creating default...", e);
            File.Move(ConfigPath, configBakPath);
            return Save(Config.CreateDefault());
        }
    }

    [NotNull]
    private static Config Save([NotNull] Config config)
    {
        string xml;
        try
        {
            xml = Serialize(config);
        }
        catch (Exception e)
        {
            PluginContext.Log.Error("Couldn't to serialize config. Creating default...", e);
            return Config.CreateDefault();
        }

        PluginContext.Log.Info($"File “{ConfigPath}” to save:{Environment.NewLine}{xml}");

        try
        {
            File.WriteAllText(ConfigPath, xml);
        }
        catch (Exception e)
        {
            PluginContext.Log.Error($"Couldn't write file “{ConfigPath}”.", e);
        }

        return config;
    }

    [NotNull, Pure]
    private static Config Deserialize([NotNull] string xml)
    {
        using (var reader = new StringReader(xml))
        {
            return (Config)Serializer.Deserialize(reader);
        }
    }

    [NotNull, Pure]
    private static string Serialize([NotNull] Config config)
    {
        var buffer = new StringBuilder();
        var xmlSettings = new XmlWriterSettings
        {
            Indent = true,
            CheckCharacters = false,
            OmitXmlDeclaration = true
        };
        using (var writer = XmlWriter.Create(buffer, xmlSettings))
        {
            Serializer.Serialize(writer, config);
        }
        return buffer.ToString();
    }
    #endregion
}