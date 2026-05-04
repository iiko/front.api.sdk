using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Resto.Front.Api.SampleRazorRunner;
//If you are going to use RazorRunner from the development environment, please comment block about System.Runtime.CompilerServices.Unsafe in the app.config
public static class RazorRunner
{
    /// <summary>
    /// Updates filter of assemblies references for Rasor engine that can be used in <see cref="RunCompile"/>.<br/>
    /// It is the caller's responsibility to load the necessary assemblies.
    /// </summary>
    /// <param name="restoApiAssemblyToUse">The dll of the currently used version of Resto.Front.Api.<br/>
    /// For example: "Resto.Front.Api.V8.dll"</param>
    /// <param name="allowedAssembliesReferences">Filter that will be used to filter all found assemblies for <see cref="RunCompile"/> for template.</param>
    /// <remarks>Next assemblies can be loaded by default:<br/>
    /// mscorlib.dll<br/>
    /// netstandard.dll<br/>
    /// System.dll<br/>
    /// System.Core.dll<br/>
    /// RazorEngine.NetStandard.dll<br/>
    /// Microsoft.CSharp.dll </remarks>
    public static void UpdateRazorAssemblyReference(string restoApiAssemblyToUse, IEnumerable<string> allowedAssembliesReferences)
    {
        Engine.Razor = RazorEngineService.Create(new TemplateServiceConfiguration { ReferenceResolver = new ExternalAssemblyReferenceResolver(restoApiAssemblyToUse, allowedAssembliesReferences) });
    }

    /// <summary>
    /// To familiarize yourself with the markup description language
    /// <see href="https://ru.iiko.help/articles/#!iikooffice-8-7/topic-5">click here</see>
    /// </summary>
    /// <param name="template">Razor template that needs to RunCompile.</param>
    /// <param name="model">The object of model itself.</param>
    public static string RunCompile(string template, object model)
    {
        var modelType = model?.GetType();
        return Engine.Razor.RunCompile(template, DateTime.Now.ToString(CultureInfo.CurrentCulture), modelType?.Namespace != null ? modelType : null, model);
    }
}

internal class ExternalAssemblyReferenceResolver : IReferenceResolver
{
    private static readonly string[] BaseRazorAssembliesFilter =
    {
        "mscorlib.dll",
        "netstandard.dll",
        "System.dll",
        "System.Core.dll",
        "RazorEngine.NetStandard.dll",
        "Microsoft.CSharp.dll",
        typeof(ExternalAssemblyReferenceResolver).Assembly.ManifestModule.Name
    };

    private readonly string restoApiAssemblyToUse;
    private readonly HashSet<string> allowedAssembliesReferences;
    public ExternalAssemblyReferenceResolver(string restoApiAssemblyToUse, IEnumerable<string> allowedAssembliesReferences)
    {
        this.restoApiAssemblyToUse = restoApiAssemblyToUse;
        this.allowedAssembliesReferences = allowedAssembliesReferences.ToHashSet();
    }

    public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies = null)
    {
        var loadedCompilerReference = CompilerServicesUtility
            .GetLoadedAssemblies()
            .Where(a => !a.IsDynamic)
            .GroupBy(a => a.GetName().Name).Select(grp =>
                grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version)))
            .Select(CompilerReference.From)
            .Concat(includeAssemblies ?? Enumerable.Empty<CompilerReference>())
            .OfType<CompilerReference.DirectAssemblyReference>()
            .ToArray();

        var currentRazorAssembliesFilter = new HashSet<string>(new List<string> { restoApiAssemblyToUse }, StringComparer.CurrentCultureIgnoreCase);
        currentRazorAssembliesFilter.UnionWith(BaseRazorAssembliesFilter);
        currentRazorAssembliesFilter.UnionWith(allowedAssembliesReferences);

        return loadedCompilerReference.Where(reference => currentRazorAssembliesFilter.Contains(reference.Assembly.ManifestModule.Name)).ToArray();
    }
}