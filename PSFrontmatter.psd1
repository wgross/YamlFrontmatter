@{
    RootModule = 'PSFrontmatter.dll'
    ModuleVersion = '0.1.0'
    GUID = '0821167d-d0b8-4e33-8be2-bc8f3acc110e'
    Author = 'github.com/wgross'
    Copyright = '(c) github.com/wgross. All rights reserved.'
    Description = 'Provides a parser to extract a YAML frontmatter from a markdown file'
    PowershellHostVersion="7.0"
    CmdletsToExport = @(
        'Get-YamlFrontmatter'
    )
    PrivateData = @{
        PSData = @{
            Tags= @("Markdown","Yaml","Parser")
        }
    }
}