using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StronglyTypedDictionaryGenerator
{
    [Generator]
    public sealed class StronglyTypedDictionaryGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new StronglyTypedDictionarySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            var stronglyTypedDictionary = new StronglyTypedDictionary(context);
            stronglyTypedDictionary.GenerateAttributes();
            stronglyTypedDictionary.GenerateSupportingTypes();

            if (context.SyntaxReceiver is StronglyTypedDictionarySyntaxReceiver receiver)
            {
                var compilation = stronglyTypedDictionary.GetCompilation();
                var attributeSymbol = stronglyTypedDictionary.GetAttributeSymbol(compilation);
                if (attributeSymbol is null)
                {
                    return;
                }

                var namedTypeSymbols = receiver.CandidateClasses
                    .Select(x => compilation
                        .GetSemanticModel(x.SyntaxTree)
                        .GetDeclaredSymbol(x))
                    .Distinct(SymbolEqualityComparer.Default)
                    .OfType<INamedTypeSymbol>();

                foreach (var namedTypeSymbol in namedTypeSymbols)
                {
                    try
                    {
                        if (HasAttribute(namedTypeSymbol, attributeSymbol) && namedTypeSymbol.IsInContainingNamespace())
                        {
                            stronglyTypedDictionary.Generate(namedTypeSymbol, attributeSymbol);
                        }

                    }
                    catch (Exception e)
                    {
                        _ = e;
#if DEBUG
                        System.Diagnostics.Debugger.Launch();
#endif
                        throw;
                    }


                }
            }
        }

        public static bool HasAttribute(INamedTypeSymbol symbol, INamedTypeSymbol? attributeSymbol) => symbol
            .GetAttributes()
            .Any(ad => ad?.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) ?? false);

        class StronglyTypedDictionarySyntaxReceiver : ISyntaxReceiver
        {
            public HashSet<ClassDeclarationSyntax> CandidateClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}
