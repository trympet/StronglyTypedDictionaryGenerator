using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StronglyTypedDictionaryGenerator
{

    internal class StronglyTypedDictionary
    {
        private static readonly DiagnosticDescriptor UnsupportedDefaultValue = new DiagnosticDescriptor(
            "STDG001",
            "Unsupported default value",
            "Default value for property '{0}' is not supported: {1}",
            "StronglyTypedDictionaryGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: [WellKnownDiagnosticTags.NotConfigurable]);

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
                context.AddSource($"{namedTypeSymbol.Name}_StronglyTypedDictionary.g", SourceText.From(sourceText, Encoding.UTF8));
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
            var targetSymbol = attributeArgs.TargetSymbol;
            var targetSymbols = new List<INamedTypeSymbol> { targetSymbol };
            targetSymbols.AddRange(targetSymbol.AllInterfaces);
            var members = targetSymbols.SelectMany(x => x
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(x => !x.IsIndexer)
                    .Where(x => x.Type.IsUnmanagedType || x.Type.IsString()))
                .Distinct<IPropertySymbol>(SymbolEqualityComparer.Default);

            EnumerateAndGenerate(attributeArgs, sb, members);

            return sb.ToString();
        }

        private void EnumerateAndGenerate(AttributeArgs attributeArgs, StringBuilder sb, IEnumerable<IPropertySymbol> members)
        {
            foreach (var member in members)
            {
                sb.AppendLine(attributeArgs.ImplementationPublic
                    ? $"{GetIndent(2)}public {member.Type.ToDisplayString()} {member.Name}"
                    : $"{GetIndent(2)}{member.Type.ToDisplayString()} {attributeArgs.TargetSymbol.Name}.{member.Name}");
                sb.AppendLine($"{GetIndent(2)}{{");
                sb.Append($"{GetIndent(3)}get => Get");
                var defaultValue = attributeArgs.SupportsDefaultValues ? GetDefaultValueString(member) : null;
                var hasDefaultValue = defaultValue is not null;
                if (hasDefaultValue)
                {
                    sb.Append($"OrDefault");
                }
                if (member.Type.IsUnmanagedType)
                {
                    sb.Append($"<{member.Type.ToDisplayString()}>");
                }
                sb.Append('(');
                if (hasDefaultValue)
                {
                    sb.Append(defaultValue);
                }
                sb.AppendLine(");");
                sb.AppendLine($"{GetIndent(3)}set => Set(value);");
                sb.AppendLine($"{GetIndent(2)}}}");
            }
        }

        private string? GetDefaultValueString(IPropertySymbol member)
        {
            var attributeData = member.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "DefaultValueAttribute");
            if (attributeData is null)
            {
                return null;
            }

            if (attributeData.ConstructorArguments.Length != 1)
            {
                return ReportUnsupportedDefaultValue(member, attributeData, "only the single-value constructor is supported");
            }

            var defaultConstant = attributeData.ConstructorArguments[0];
            if (defaultConstant.Kind == TypedConstantKind.Error)
            {
                return ReportUnsupportedDefaultValue(member, attributeData, "the argument is not a valid constant");
            }

            if (defaultConstant.Kind == TypedConstantKind.Primitive && defaultConstant.IsNull)
            {
                var attributeSyntax = attributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) as AttributeSyntax;
                var argumentExpression = attributeSyntax?.ArgumentList?.Arguments[0].Expression;
                var argumentType = argumentExpression is null
                    ? null
                    : context.Compilation.GetSemanticModel(argumentExpression.SyntaxTree).GetTypeInfo(argumentExpression, context.CancellationToken).Type;
                if (argumentType is not null
                    && argumentType.SpecialType != SpecialType.System_Object
                    && !SymbolEqualityComparer.Default.Equals(argumentType, member.Type))
                {
                    return ReportUnsupportedDefaultValue(member, attributeData, $"null argument of type '{argumentType.ToDisplayString()}' is unsupported for this property");
                }

                return member.Type.IsUnmanagedType ? $"default({member.Type.ToDisplayString()})" : "null";
            }

            if (defaultConstant.Kind == TypedConstantKind.Enum
                && SymbolEqualityComparer.Default.Equals(defaultConstant.Type, member.Type))
            {
                return $"({member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})({defaultConstant.ToCSharpString()})";
            }

            if (defaultConstant.Kind != TypedConstantKind.Primitive)
            {
                return ReportUnsupportedDefaultValue(member, attributeData, $"constant kind '{defaultConstant.Kind}' is unsupported for this property");
            }

            return FormatPrimitiveConstant(defaultConstant)
                ?? ReportUnsupportedDefaultValue(member, attributeData, "the constant's value type is unsupported");
        }

        private static string? FormatPrimitiveConstant(TypedConstant defaultConstant)
        {
            return defaultConstant.Value switch
            {
                float value when float.IsNaN(value) => "float.NaN",
                float value when float.IsPositiveInfinity(value) => "float.PositiveInfinity",
                float value when float.IsNegativeInfinity(value) => "float.NegativeInfinity",
                float value => SyntaxFactory.Literal(value).Text,
                double value when double.IsNaN(value) => "double.NaN",
                double value when double.IsPositiveInfinity(value) => "double.PositiveInfinity",
                double value when double.IsNegativeInfinity(value) => "double.NegativeInfinity",
                double value => $"{SyntaxFactory.Literal(value).Text}D",
                decimal value => SyntaxFactory.Literal(value).Text,
                bool or char or string or sbyte or byte or short or ushort or int or uint or long or ulong => defaultConstant.ToCSharpString(),
                _ => null
            };
        }

        private static AttributeArgs? GetAttributeArgs(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol attributeSymbol)
        {
            var attribute = namedTypeSymbol.GetAttributes()
                .First(x => x.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) ?? false);

            if (!attribute.ConstructorArguments.Any())
            {
                return null;
            }

            try
            {
                var targetTypeArg = (INamedTypeSymbol?)attribute.ConstructorArguments[1].Value;
                return new AttributeArgs
                {
                    ImplementationPublic = ((int)attribute.ConstructorArguments[0].Value!) == 0,
                    TargetSymbol = targetTypeArg ?? namedTypeSymbol,
                    SupportsDefaultValues = (bool)attribute.ConstructorArguments[2].Value!
                };
            }
            catch (InvalidCastException)
            {
                return null;
            }
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

        private string? ReportUnsupportedDefaultValue(IPropertySymbol member, AttributeData attributeData, string reason)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                UnsupportedDefaultValue,
                attributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                    ?? member.Locations.FirstOrDefault()
                    ?? Location.None,
                member.ToDisplayString(),
                reason));

            return null;
        }

        private class AttributeArgs
        {
            public bool ImplementationPublic { get; set; }

            public INamedTypeSymbol TargetSymbol { get; set; } = null!;

            public bool SupportsDefaultValues { get; set; }
        }
    }
}
