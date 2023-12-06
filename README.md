# Kysect.DotnetSlnParser

DotnetSlnParser is a nuget package for working with .sln, .csproj and .props files.

## Parsing

Nuget provide API for parsing .sln and .csproj files:

```csharp
var csprojContent = """
                    <Project Sdk="Microsoft.NET.Sdk">
                        <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <Nullable>enable</Nullable>
                        </PropertyGroup>
                    
                        <ItemGroup>
                        <PackageReference Include="FluentAssertions" />
                        <PackageReference Include="Microsoft.NET.Test.Sdk" />
                        </ItemGroup>
                    
                        <ItemGroup>
                        <ProjectReference Include="..\Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj" />
                        </ItemGroup>
                    </Project>
                    """;

var expected = new DotnetProjectFileContent(
    TargetFramework: "net8.0",
    EnableDefaultItems: true,
    IncludedFiles: Array.Empty<string>(),
    References: new[] { "..\\Kysect.DotnetSlnParser\\Kysect.DotnetSlnParser.csproj" });

DotnetProjectFileContent? result = _parser.ParseContent(csprojContent);

result.Should().BeEquivalentTo(expected);
```

## Modification

Nuget provide API for modification of parsed solution. Sample of modification;

```csharp
var projectContent = """
                             <Project Sdk="Microsoft.NET.Sdk">
                               <PropertyGroup>
                                 <TargetFramework>net8.0</TargetFramework>
                               </PropertyGroup>
                             </Project>
                             """;

var expectedProjectContent = """
                        <Project Sdk="Microsoft.NET.Sdk">
                        <PropertyGroup>
                            <TargetFramework>net9.0</TargetFramework>
                        </PropertyGroup>
                        </Project>
                        """;
```

For this modification was introduces strategy that describe modification:

```csharp
public class SetTargetFrameworkModifyStrategy(string value) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        document.ThrowIfNull();

        return document
            .GetNodesByName("TargetFramework")
            .OfType<XmlElementSyntax>()
            .ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        syntax.ThrowIfNull();

        XmlTextSyntax content = SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteralToken(value, null, null));
        return syntax.ReplaceNode(syntax.Content.Single(), content);
    }
}
```

And this strategy applied to solutions:

```csharp
var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileParser(_logger));

foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
    solutionModifierProject.Accessor.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

solutionModifier.Save();
```
