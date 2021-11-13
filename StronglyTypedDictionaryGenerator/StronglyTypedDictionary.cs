﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StronglyTypedDictionaryGenerator
{

    internal class StronglyTypedDictionary
    {
        private readonly GeneratorExecutionContext context;

        public StronglyTypedDictionary(GeneratorExecutionContext context)
        {
            this.context = context;
        }

        public void GenerateSupportingTypes()
        {
            GeneratedTypes.GeneratedBase.AddToContext(context);
            GeneratedTypes.IStronglyTypedKeyValuePairAccessor.AddToContext(context);

        }

        public void GenerateAttributes()
        {
            GeneratedTypes.StronglyTypedDictionaryAttribute.AddToContext(context);
        }

        public void Generate(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol attributeSymbol)
        {
            var sourceText = GetGeneratedSource(namedTypeSymbol, attributeSymbol);
            if (sourceText is not null)
            {
                context.AddSource($"{namedTypeSymbol.Name}_StronglyTypedDictionary.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        }

        public Compilation GetCompilation()
        {
            var options = (context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions;
            return context.Compilation.AddSyntaxTrees(
                GeneratedTypes.GeneratedBase.GetSyntaxTree(options),
                GeneratedTypes.IStronglyTypedKeyValuePairAccessor.GetSyntaxTree(options),
                GeneratedTypes.StronglyTypedDictionaryAttribute.GetSyntaxTree(options)
            );
        }

        public INamedTypeSymbol? GetAttributeSymbol(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(GeneratedTypes.StronglyTypedDictionaryAttribute.TypeName);
        }

        private string? GetGeneratedSource(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol attributeSymbol)
        {
            var attributeArgs = GetAttributeArgs(namedTypeSymbol, attributeSymbol);
            if (attributeArgs is null)
            {
                return null;
            }

            var namespaceName = namedTypeSymbol.ContainingNamespace.ToDisplayString();
            var accessModifier = namedTypeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
            SymbolDisplayFormat classNameFormat = GetFullyQualifiedTypeNameFormat();
            var className = namedTypeSymbol.ToDisplayString(classNameFormat);
            return GeneratedTypes.Generated
                .GetTemplate()
                .Render(new
                {
                    AccessModifier = accessModifier,
                    Namespace = namespaceName,
                    ClassName = className,
                    FullyQualifiedTargetType = attributeArgs.TargetSymbol
                        .ToDisplayString(new SymbolDisplayFormat(
                            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included)
                        ),
                    GeneratedProperties = EnumerateAndGenerateProperties(attributeArgs)
                }
            );
        }

        private static SymbolDisplayFormat GetFullyQualifiedTypeNameFormat()
        {
            return new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance);
        }

        private string EnumerateAndGenerateProperties(AttributeArgs attributeArgs)
        {
            var sb = new StringBuilder();
            var members = attributeArgs.TargetSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => !x.IsIndexer);
            foreach (var member in members)
            {
                sb.AppendLine(attributeArgs.ImplementationPublic
                    ? $"{GetIndent(2)}public {member.Type.ToDisplayString()} {member.Name}"
                    : $"{GetIndent(2)}{member.Type.ToDisplayString()} {attributeArgs.TargetSymbol.Name}.{member.Name}");
                sb.AppendLine($"{GetIndent(2)}{{");
                sb.Append($"{GetIndent(3)}get => Get");
                var defaultValue = attributeArgs.SupportsDefaultValues ? GetDefaultValue(member) : null;
                var hasDefaultValue = defaultValue is not null;
                if (hasDefaultValue)
                {
                    sb.Append($"OrDefault");
                }
                sb.Append($"<{member.Type.ToDisplayString()}>(");
                if (hasDefaultValue)
                {
                    sb.Append(defaultValue!.Value.ToCSharpString());
                }
                sb.AppendLine(");");
                sb.AppendLine($"{GetIndent(3)}set => Set(value);");
                sb.AppendLine($"{GetIndent(2)}}}");
            }

            return sb.ToString();
        }

        private TypedConstant? GetDefaultValue(IPropertySymbol member)
        {
            if (member.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "DefaultValueAttribute") is AttributeData attributeData)
            {
                return attributeData.ConstructorArguments.FirstOrDefault();
            }

            return null;
        }

        private static AttributeArgs? GetAttributeArgs(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol attributeSymbol)
        {
            var attribute = namedTypeSymbol.GetAttributes()
                .First(x => x.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) ?? false);

            if (!attribute.ConstructorArguments.Any())
            {
                return null;
            }

            var targetTypeArg = (INamedTypeSymbol?)attribute.ConstructorArguments[1].Value;
            return new AttributeArgs
            {
                ImplementationPublic = ((int)attribute.ConstructorArguments[0].Value!) == 0,
                TargetSymbol = targetTypeArg ?? namedTypeSymbol,
                SupportsDefaultValues = (bool)attribute.ConstructorArguments[2].Value!
            };
        }

        private static string GetIndent(int level)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < level; i++)
            {
                sb.Append(' ');
                sb.Append(' ');
                sb.Append(' ');
                sb.Append(' ');
            }

            return sb.ToString();
        }

        private class AttributeArgs
        {
            public bool ImplementationPublic { get; set; }

            public INamedTypeSymbol TargetSymbol { get; set; }

            public bool SupportsDefaultValues { get; set; }
        }
    }
}
