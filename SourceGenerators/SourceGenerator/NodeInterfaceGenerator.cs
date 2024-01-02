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

        INamedTypeSymbol nodeClass = context.Compilation.GetTypeByMetadataName("Godot.Node") ?? throw new NullReferenceException("Node type cannot be found");
        INamedTypeSymbol resourceClass = context.Compilation.GetTypeByMetadataName("Godot.Resource") ?? throw new NullReferenceException("Resource type cannot be found");
        INamedTypeSymbol attributeClass = context.Compilation.GetTypeByMetadataName("LandlessSkies.Generators.NodeInterfaceAttribute") ?? throw new NullReferenceException("Interface Attribute cannot be found");


		foreach (SyntaxTree syntaxTree in context.Compilation.SyntaxTrees) {

			SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
			IEnumerable<SyntaxNode> syntaxNodes = syntaxTree.GetRoot().DescendantNodesAndSelf();

			IEnumerable<ClassDeclarationSyntax> classes = syntaxNodes
				.OfType<ClassDeclarationSyntax>();

			// Find all interface declarations with the "NodeInterface" attribute
			List<InterfaceDeclarationSyntax>? interfaces = syntaxNodes
				.OfType<InterfaceDeclarationSyntax>()
				.Where(
					interfaceSyntax => interfaceSyntax.AttributeLists
						.SelectMany(attrList => attrList.Attributes)
						.Any(attr => SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(attr).Symbol, attributeClass)
					)
				)
				.ToList();

			// Generate code for each interface
			foreach (InterfaceDeclarationSyntax interfaceSyntax in interfaces) {
				INamedTypeSymbol? interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceSyntax);
				if (interfaceSymbol is null) continue;
				

				IEnumerable<INamedTypeSymbol> implementingClasses = syntaxTree.GetRoot().DescendantNodesAndSelf()
					.OfType<ClassDeclarationSyntax>()
					.Select(classSyntax => semanticModel.GetDeclaredSymbol(classSyntax))
					.Where(type => type is not null && type.BaseType is not null && type.AllInterfaces.Contains(interfaceSymbol))!;

				IEnumerable<INamedTypeSymbol> nodeClasses = implementingClasses.Where(type => InheritsFrom(type, nodeClass));
				IEnumerable<INamedTypeSymbol> resourceClasses = implementingClasses.Where(type => InheritsFrom(type, resourceClass));

				
				string infoSource = GenerateInfoCode(interfaceSymbol, nodeClasses, resourceClasses);
				
				string infoFileName = $"{interfaceSymbol.Name}.info.cs";
				SyntaxTree? infoSyntaxTree = SyntaxFactory.ParseSyntaxTree(infoSource, encoding: Encoding.UTF8);

				context.AddSource(infoFileName, infoSyntaxTree.GetText());
			}
		}
    }

    private bool InheritsFrom(INamedTypeSymbol childType, INamedTypeSymbol baseType) {
        while (childType.BaseType is not null) {
            if (childType.BaseType.ToString() == baseType.ToString()) {
                return true;
            }

            childType = childType.BaseType;
        }

        return false;
    }

    private string GenerateInfoCode(INamedTypeSymbol interfaceSymbol, IEnumerable<INamedTypeSymbol> nodeClasses, IEnumerable<INamedTypeSymbol> resourceClasses) {
        string className = $"{interfaceSymbol.Name}Info";
        StringBuilder codeBuilder = new();
        
        List<string> namespaces = nodeClasses.Concat(resourceClasses)
            .Select(symbol => symbol.ContainingNamespace.Name)
            .Distinct()
            .Where(ns => ns != interfaceSymbol.ContainingNamespace.Name)
            .ToList();
        
        string nodeHintString = string.Join(",", nodeClasses.Select(symbol => symbol.Name).Distinct());
        string resourceHintString = string.Join(",", resourceClasses.Select(symbol => symbol.Name).Distinct());

        

        codeBuilder.AppendLine("using System;");
        foreach (string @namespace in namespaces) {
            codeBuilder.AppendLine($"using {@namespace};");
        }
        codeBuilder.AppendLine();
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"namespace {interfaceSymbol.ContainingNamespace};");
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"public static class {className} {{");

        codeBuilder.AppendLine($"    public const string NodeHintString = \"{nodeHintString}\";");

        codeBuilder.AppendLine($"    public const string ResourceHintString = \"{resourceHintString}\";");

        codeBuilder.AppendLine($"    public static readonly Type[] NodeImplementations = {{");
        foreach (INamedTypeSymbol implementingClass in nodeClasses) {
            codeBuilder.AppendLine($"        typeof({implementingClass.Name}),");
        }
        codeBuilder.AppendLine("    };");

        codeBuilder.AppendLine($"    public static readonly Type[] ResourceImplementations = {{");
        foreach (INamedTypeSymbol implementingClass in resourceClasses) {
            codeBuilder.AppendLine($"        typeof({implementingClass.Name}),");
        }
        codeBuilder.AppendLine("    };");

        codeBuilder.AppendLine("}");

        return codeBuilder.ToString();
    }
}