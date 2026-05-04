using Resto.Front.Api.Data.Cheques;
using System;
using System.Xml.Linq;
using Resto.Front.Api.Data.Print;

namespace Resto.Front.Api.SamplePlugin;

public sealed class PrintTester : IDisposable
{
    private readonly IDisposable beforeFormatHandlerSubscription;
    private readonly IDisposable afterFormatHandlerSubscription;

    public void Dispose()
    {
        beforeFormatHandlerSubscription.Dispose();
        afterFormatHandlerSubscription.Dispose();
    }

    public PrintTester()
    {
        beforeFormatHandlerSubscription =
            PluginContext.Operations.RegisterBeforeFormatDocumentHandler(BeforeFormatDocumentHandler);
        afterFormatHandlerSubscription =
            PluginContext.Operations.RegisterAfterFormatDocumentHandler(AfterFormatDocumentHandler);
    }

    private Document BeforeFormatDocumentHandler((Guid printingDeviceId, Document documentMarkup) arg)
    {
        var changed = false;
        foreach (var element in arg.documentMarkup.Markup.Elements())
        {
            if (element.Name.LocalName == "section" && element.Attribute("name")?.Value == "SamplePlugin")
            {
                element.AddAfterSelf(new XElement(Tags.Pair,
                    new XAttribute("left", "SamplePlugin before data:"),
                    new XAttribute("right", element.Attribute("data")?.Value ?? "None")));
                changed = true;
            }
        }

        return changed ? arg.documentMarkup : null;
    }

    private Document AfterFormatDocumentHandler((Guid printingDeviceId, Document documentMarkup) arg)
    {
        arg.documentMarkup.Markup.AddFirst(new XElement(Tags.LargeFont, "Sample plugin header line 3"));
        arg.documentMarkup.Markup.AddFirst(new XElement(Tags.MediumFont, "Sample plugin header line 2"));
        arg.documentMarkup.Markup.AddFirst(new XElement(Tags.SmallFont, "Sample plugin header line 1"));
        arg.documentMarkup.Markup.Add(new XElement(Tags.SmallFont, "Sample plugin footer line 1"));
        arg.documentMarkup.Markup.Add(new XElement(Tags.MediumFont, "Sample plugin footer line 2"));
        arg.documentMarkup.Markup.Add(new XElement(Tags.LargeFont, "Sample plugin footer line 3"));
        return arg.documentMarkup;
    }

}
