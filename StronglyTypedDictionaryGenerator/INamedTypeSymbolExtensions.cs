using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StronglyTypedDictionaryGenerator
{
    internal static class INamedTypeSymbolExtensions
    {
        public static bool IsInContainingNamespace(this INamedTypeSymbol namedTypeSymbol)
            => namedTypeSymbol.ContainingSymbol.Equals(namedTypeSymbol.ContainingNamespace,
                SymbolEqualityComparer.Default);

        public static bool IsString(this ITypeSymbol? typeSymbol)
        {
            return typeSymbol?.ContainingNamespace?.Name == "System" && typeSymbol?.MetadataName == "String";
        }
    }
}
