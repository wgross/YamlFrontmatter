using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using YamlDotNet.Serialization;

namespace frontmatter;

[Cmdlet(VerbsCommon.Get, "YamlFrontmatter", DefaultParameterSetName = nameof(Path))]
public sealed class SelectFrontMatterYamlCommand : PSCmdlet
{
    private static readonly MarkdownPipeline frontMatterPipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .Build();
    
    private static readonly IDeserializer yamlFrontmatterDeserializer = new DeserializerBuilder()
        .Build();

    [Parameter(
        ParameterSetName = nameof(LiteralPath),
        Mandatory = true,
        Position = 0,
        ValueFromPipelineByPropertyName = true)]
    [Alias("PSPath")]
    [ValidateNotNullOrEmpty]
    public string LiteralPath { get; set; }

    [Parameter(
        ParameterSetName = nameof(Path),
        Mandatory = true,
        Position = 0,
        ValueFromPipelineByPropertyName = true)]
    public string Path { get; set; }

    //[Parameter(
    //    Mandatory = true,
    //    ValueFromPipeline = true,
    //    ParameterSetName = nameof(Content))]
    //[ValidateNotNullOrEmpty]
    //public object[] Content { get; set; }

    protected override void ProcessRecord()
    {
        //if (this.ParameterSetName == nameof(Content))
        //    this.SelectYamlFontMatterFromContent(string.Join(Environment.NewLine, this.Content));
        //else
        this.SelectYamlFrontmatterFromFile();
    }

    private void SelectYamlFontMatterFromContent(string content)
    {
        var yamlFrontmatter = Markdown
            .Parse(content, frontMatterPipeline)
            .Descendants<YamlFrontMatterBlock>()
            .FirstOrDefault();

        if (yamlFrontmatter is { Span: { IsEmpty: false } })
            if (yamlFrontmatterDeserializer.Deserialize(yamlFrontmatter.Lines.ToString()) is Dictionary<object, object> properties)
                this.WriteObject(this.MapToPsObject(properties));
    }

    private void SelectYamlFrontmatterFromFile()
    {
        string selectPath() => nameof(Path).Equals(this.ParameterSetName)
            ? this.Path
            : this.LiteralPath;

        // always expand without wildcards. Wildcards belong into the filter
        // https://stackoverflow.com/questions/8505294/how-do-i-deal-with-paths-when-writing-a-powershell-cmdlet
        var resolvedPath = this.SessionState
            .Path
            .GetUnresolvedProviderPathFromPSPath(
                path: selectPath(),
                out var provider,
                out var drive);

        // break hard if the path isn't pointing to a win32 file system.
        if (provider.ImplementingType != typeof(FileSystemProvider))
            throw new PSNotSupportedException("provider not supported");

        this.SelectYamlFontMatterFromContent(File.ReadAllText(resolvedPath));
    }

    private PSObject MapToPsObject(Dictionary<object, object> propertiesDictionary)
    {
        var psObject = new PSObject();
        foreach (var kv in propertiesDictionary)
        {
            psObject.Properties.Add(new PSNoteProperty(kv.Key.ToString(), kv.Value));

            if (kv.Value is Dictionary<object, object> nestedDictionary)
                psObject.Properties.Add(new PSNoteProperty(kv.Key.ToString(), this.MapToPsObject(nestedDictionary)));
        }
        return psObject;
    }
}