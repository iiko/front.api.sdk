using System;
using System.Xml.Serialization;

namespace Resto.Front.Api.SamplePlugin.Config;

[XmlRoot("config")]
public sealed class Config
{
    [XmlElement]
    public Guid ClientId { get; set; } = Guid.NewGuid();

    [XmlElement]
    public Guid HostDatabaseId { get; set; } = Guid.Empty;

    [XmlElement]
    public int MaxKnownVersionedEntityRevision { get; set; }

    public static Config CreateDefault()
    {
        return new Config();
    }
}