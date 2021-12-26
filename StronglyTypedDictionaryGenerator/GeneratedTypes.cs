using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Text;

namespace StronglyTypedDictionaryGenerator
{
    internal static class GeneratedTypes
    {
        public static GeneratedType Generated = new GeneratedType("Generated", true);

        public static GeneratedType GeneratedBase = new GeneratedType("GeneratedBase", false);

        public static GeneratedType IStronglyTypedKeyValuePairAccessor = new GeneratedType("IStronglyTypedKeyValuePairAccessor", false);

        public static GeneratedType StronglyTypedDictionaryAttribute = new GeneratedType("StronglyTypedDictionaryAttribute", false);

        public class GeneratedType
        {
            private readonly bool isScriban;

            public GeneratedType(string typeName, bool isScriban)
            {
                TypeName = typeName;
                this.isScriban = isScriban;
            }

            public string TypeName { get; }

            public string GeneratedFileName => $"{TypeName}.g";

            public string SourceTextFileName => $"{TypeName}.{Extension}";

            public SourceText SourceText => SourceText.From(EmbeddedResource.GetContent(SourceTextFileName), Encoding.UTF8);

            public SyntaxTree GetSyntaxTree(CSharpParseOptions? parseOptions) => CSharpSyntaxTree.ParseText(SourceText, parseOptions);

            private string Extension => isScriban ? "sbntxt" : "txt";

            public void AddToContext(GeneratorExecutionContext context)
            {
                context.AddSource(GeneratedFileName, SourceText);

            }

            public Template GetTemplate()
            {
                if (isScriban)
                {
                    return Template.Parse(EmbeddedResource.GetContent(SourceTextFileName));
                }

                throw new InvalidOperationException();
            }
        }
    }
}
