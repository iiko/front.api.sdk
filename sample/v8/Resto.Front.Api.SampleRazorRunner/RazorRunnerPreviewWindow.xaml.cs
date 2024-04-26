using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using Resto.Front.Api.Data.Print;

namespace Resto.Front.Api.SampleRazorRunner;

public partial class RazorRunnerPreviewWindow
{
    //If you are going to use RazorRunner from the Front`s plugins directory, please uncomment the bindingRedirect about "System.Runtime.CompilerServices.Unsafe" in the project's app.config.
    public RazorRunnerPreviewWindow()
    {
        InitializeComponent();

        // Templates should be saved to the user profile folder, because when the plugin is updated, all the contents of its folder are cleared.
        try
        {
            var templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "RazorTemplateSample.cshtml");
            if (File.Exists(templatePath))
            {
                var text = File.ReadAllLines(templatePath);
                ShowTextOnRichTextBox(text, RazorTemplateRichTextBox);
            }
        }
        catch
        {
            // ignored
        }
    }

    private void btnCompileAndExecuteRazorTemplate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsEnabled = false;

            var templateToCompileRun = new TextRange(RazorTemplateRichTextBox.Document.ContentStart,
                RazorTemplateRichTextBox.Document.ContentEnd).Text;

            //if you want to use a static model for RazorTemplate
            var result = RunCompile(templateToCompileRun, new SampleOrderModel { OrderId = PluginContext.Operations.GetOrders().Last().Id });

            //If you want to use a dynamic model for RazorTemplate
            //But you need to remember that in this case you will need to define the model data types inside the template
            //For example: (Guid)@Model.OrderId
            //var result = RunCompile(templateToCompileRun, new { OrderId = PluginContext.Operations.GetOrders().Last().Id });

            var formattedDocument = PluginContext.Operations.FormatDocumentOnPrintingDevice(null, new Document { Markup = XElement.Parse(result) });

            //Alternatively, you can use "Print" the operation to print the resulting formatted template
            //PluginContext.Operations.Print(PluginContext.Operations.GetPrintingDeviceInfos().Last(), new Document { Markup = XElement.Parse(result) });

            var xml = new XmlDocument();
            xml.LoadXml(formattedDocument.Markup.ToString());
            if (xml.DocumentElement == null)
                return;

            ShowTextOnRichTextBox(
                (from XmlNode child in xml.DocumentElement.ChildNodes select child.InnerText).ToList(),
                CompiledAndExecutedRazorTemplateRichTextBox);
        }
        catch (Exception ex)
        {
            var exceptionMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ShowTextOnRichTextBox(new List<string> { exceptionMessage },
                CompiledAndExecutedRazorTemplateRichTextBox);
        }
        finally
        {
            IsEnabled = true;
        }
    }

    /// <summary>
    /// To familiarize yourself with the markup description language
    /// <see href="https://ru.iiko.help/articles/#!iikooffice-8-7/topic-5">click here</see>
    /// </summary>
    /// <param name="template">Razor template that needs to RunCompile.</param>
    /// <param name="model">The object of model itself.</param>
    private string RunCompile(string template, object model = null)
    {
        RazorRunner.UpdateRazorAssemblyReference(typeof(PluginContext).Assembly.ManifestModule.Name,
            new List<string> { model?.GetType().Assembly.ManifestModule.Name /*, add others assemblies that can be used in compilation here*/ });
        return RazorRunner.RunCompile(template, model);
    }

    private void ShowTextOnRichTextBox(IEnumerable<string> text, RichTextBox rtb)
    {
        var document = new FlowDocument();
        foreach (var str in text)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(str));
            document.Blocks.Add(paragraph);
        }

        rtb.Document = document;
    }

    public sealed class SampleOrderModel
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Guid OrderId { set; get; }
    }
}