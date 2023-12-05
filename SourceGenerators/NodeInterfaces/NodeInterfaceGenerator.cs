using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LandlessSkies.Generators;

[Generator]
public class NodeInterfaceGenerator : ISourceGenerator {


    public void Initialize(GeneratorInitializationContext context) {
    }

    public void Execute(GeneratorExecutionContext context) {

        // Find all interface declarations with the "NodeInterface" attribute
        List<InterfaceDeclarationSyntax>? interfaces = context.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodesAndSelf().OfType<InterfaceDeclarationSyntax>())
            .Where(
                interfaceSyntax => interfaceSyntax.AttributeLists.Any(attrList => 
                    attrList.Attributes.Any(attr => attr.Name.ToString().Contains("NodeInterface"))
                )
            )
            .ToList();

        // Generate code for each interface
        foreach (InterfaceDeclarationSyntax interfaceSyntax in interfaces) {
            INamedTypeSymbol? interfaceSymbol = context.Compilation.GetSemanticModel(interfaceSyntax.SyntaxTree).GetDeclaredSymbol(interfaceSyntax);
            if (interfaceSymbol is null) {
                continue;
            }

            IEnumerable<INamedTypeSymbol> implementingClasses = context.Compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
                .Select(classSyntax => context.Compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax))
                .Where(type => type is not null && type.BaseType is not null && type.AllInterfaces.Contains(interfaceSymbol)/*  && type.BaseType.Name.Contains("Node") */)
                .ToList()!;
            
            if (implementingClasses != null && implementingClasses.Any()) {
                string infoSource = GenerateInfoCode(interfaceSymbol, implementingClasses);
                
                string infoFileName = $"{interfaceSymbol.Name}.info.cs";
                SyntaxTree? infoSyntaxTree = SyntaxFactory.ParseSyntaxTree(infoSource, encoding: Encoding.UTF8);

                context.AddSource(infoFileName, infoSyntaxTree.GetText());

                
                string wrapperSource = GenerateWrapperCode(interfaceSymbol, implementingClasses);
                
                string wrapperFileName = $"{interfaceSymbol.Name}.wrapper.cs";
                SyntaxTree? wrappperSyntaxTree = SyntaxFactory.ParseSyntaxTree(wrapperSource, encoding: Encoding.UTF8);

                context.AddSource(wrapperFileName, wrappperSyntaxTree.GetText());
            }
        }
    }

    private string GenerateInfoCode(INamedTypeSymbol interfaceSymbol, IEnumerable<INamedTypeSymbol> implementingClasses) {
        string className = $"{interfaceSymbol.Name}Info";
        StringBuilder codeBuilder = new();
        
        List<string> namespaces = implementingClasses
            .Select(symbol => symbol.ContainingNamespace.Name)
            .Distinct()
            .Where(ns => ns != interfaceSymbol.ContainingNamespace.Name)
            .ToList();
        
        string hintString = string.Join(",", implementingClasses.Select(symbol => symbol.Name));

        

        codeBuilder.AppendLine("using System;");
        foreach (string @namespace in namespaces) {
            codeBuilder.AppendLine($"using {@namespace};");
        }
        codeBuilder.AppendLine();
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"namespace {interfaceSymbol.ContainingNamespace};");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"public static class {className} {{");
        codeBuilder.AppendLine($"    public const string HintString = \"{hintString}\";");
        codeBuilder.AppendLine($"    public static readonly Type[] Implementations = {{");

        foreach (INamedTypeSymbol implementingClass in implementingClasses) {
            codeBuilder.AppendLine($"        typeof({implementingClass.Name}),");
        }

        codeBuilder.AppendLine("    };");
        codeBuilder.AppendLine("}");

        return codeBuilder.ToString();
    }
    private string GenerateWrapperCode(INamedTypeSymbol interfaceSymbol, IEnumerable<INamedTypeSymbol> implementingClasses) {
        string className = $"{interfaceSymbol.Name}WrapperTest";
        StringBuilder codeBuilder = new();
        
        // List<string> namespaces = implementingClasses
        //     .Select(symbol => symbol.ContainingNamespace.Name)
        //     .Distinct()
        //     .Where(ns => ns != interfaceSymbol.ContainingNamespace.Name)
        //     .ToList();
        
        // string hintString = string.Join(",", implementingClasses.Select(symbol => symbol.Name));

        

        codeBuilder.AppendLine("using Godot;");
        codeBuilder.AppendLine("using System;");
        // foreach (string @namespace in namespaces) {
        //     codeBuilder.AppendLine($"using {@namespace};");
        // }
        codeBuilder.AppendLine();
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"namespace {interfaceSymbol.ContainingNamespace};");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine("[Tool]");
        codeBuilder.AppendLine("[GlobalClass]");
        codeBuilder.AppendLine($"public partial class {className} : Resource {{");
        codeBuilder.AppendLine($"   static {className}() {{");
        codeBuilder.AppendLine("        GD.Print(\"TestFromClass\");");
        codeBuilder.AppendLine("    }");
        // codeBuilder.AppendLine($"    public const string HintString = \"{hintString}\";");
        // codeBuilder.AppendLine($"    public static readonly Type[] Implementations = {{");

        // foreach (INamedTypeSymbol implementingClass in implementingClasses) {
        //     codeBuilder.AppendLine($"        typeof({implementingClass.Name}),");
        // }

        // codeBuilder.AppendLine("    };");
        codeBuilder.AppendLine("}");

        return codeBuilder.ToString();
    }
}