// -----------------------------------------------------------------------
// <copyright file="MemoryPoolGenerator.cs" company="Sage.Net">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 Sage.Net Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Sage.Net.Generators.GameMemoryGenerator;

/// <summary>Generates code for a game memory pool.</summary>
[Generator]
public sealed class MemoryPoolGenerator : IIncrementalGenerator
{
    /// <summary>Initializes the pool generator and calls the system to generate the glue code.</summary>
    /// <param name="context">The <see cref="IncrementalGeneratorInitializationContext"/> to use.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol?> classDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidate(s),
                transform: static (ctx, _) => GetSemanticTarget(ctx)
            )
            .Where(static m => m is not null);

        // Combine the per-symbol stream with the compilation (single value).
        IncrementalValuesProvider<(INamedTypeSymbol? Left, Compilation Right)> symbolAndCompilation =
            classDeclarations.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(
            symbolAndCompilation,
            static (spc, pair) => GenerateGlueCode(spc, pair.Right, pair.Left!)
        );
    }

    private static bool IsCandidate(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static INamedTypeSymbol? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        INamedTypeSymbol? memoryPooledAttribute = context.SemanticModel.Compilation.GetTypeByMetadataName(
            "Sage.Net.Core.GameEngine.Common.MemoryPooledAttribute"
        );

#pragma warning disable IDE0046 // Convert to conditional expression
        if (memoryPooledAttribute is null)
#pragma warning restore IDE0046 //  Convert to conditional expression
        {
            return null;
        }

        return symbol
            .GetAttributes()
            .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, memoryPooledAttribute))
            ? symbol
            : null;
    }

    private static bool DetectPhysicalMember(
        INamedTypeSymbol baseType,
        string memberName,
        bool requireStatic,
        int requiredParameterCount
    )
    {
        foreach (
            ISymbol? member in baseType
                .GetMembers()
                .Where(member => string.Equals(member.Name, memberName, StringComparison.Ordinal))
        )
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic != requireStatic)
            {
                continue;
            }

            if (method.Parameters.Length != requiredParameterCount)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool HasBaseMemberOrGeneratedEquivalent(
        Compilation compilation,
        INamedTypeSymbol type,
        string memberName,
        bool requireStatic,
        int requiredParameterCount
    )
    {
        INamedTypeSymbol? memoryPooledAttribute = compilation.GetTypeByMetadataName(
            "Sage.Net.Core.GameEngine.Common.MemoryPooledAttribute"
        );

        for (INamedTypeSymbol? baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            // 1) If the member is physically present on the base type, great.
            if (DetectPhysicalMember(baseType, memberName, requireStatic, requiredParameterCount))
            {
                return true;
            }

            // 2) If the base type is also [MemoryPooled], its pool API is generated too.
            // Generators can't reliably "see" other generators' emitted members, so infer it from the attribute.
            if (
                memoryPooledAttribute is null
                || !baseType
                    .GetAttributes()
                    .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, memoryPooledAttribute))
            )
            {
                continue;
            }

            if (memberName is "New" or "Delete" or "GetClassMemoryPool")
            {
                return true;
            }
        }

        return false;
    }

    private static void GenerateGlueCode(
        SourceProductionContext context,
        Compilation compilation,
        INamedTypeSymbol classSymbol
    )
    {
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;

        AttributeData? attribute = classSymbol
            .GetAttributes()
            .FirstOrDefault(ad => ad.AttributeClass?.Name is "MemoryPooledAttribute" or "MemoryPooled");

        if (attribute is null)
        {
            return;
        }

        var poolName = (string)attribute.ConstructorArguments[0].Value!;
        var initial = (int)attribute.ConstructorArguments[1].Value!;
        var overflow = (int)attribute.ConstructorArguments[2].Value!;

        var newStaticNew = HasBaseMemberOrGeneratedEquivalent(
            compilation,
            classSymbol,
            "New",
            requireStatic: true,
            requiredParameterCount: 0
        )
            ? "new "
            : string.Empty;

        var newInstanceDelete = HasBaseMemberOrGeneratedEquivalent(
            compilation,
            classSymbol,
            "Delete",
            requireStatic: false,
            requiredParameterCount: 0
        )
            ? "new "
            : string.Empty;

        var newStaticGetPool = HasBaseMemberOrGeneratedEquivalent(
            compilation,
            classSymbol,
            "GetClassMemoryPool",
            requireStatic: true,
            requiredParameterCount: 0
        )
            ? "new "
            : string.Empty;

        var glue = $$"""
            // <auto-generated/>
            #nullable enable

            using Sage.Net.Core.GameEngine.Common;

            namespace {{namespaceName}};

            /// <summary>A memory pool for {{className}}.</summary>
            public partial class {{className}}
            {
                private static readonly MemoryPool<{{className}}> _classMemoryPool
                    = new MemoryPool<{{className}}>("{{poolName}}", {{initial}}, {{overflow}});

                /// <summary>Allocates a new instance from the memory pool.</summary>
                /// <returns>A new instance of <see cref="{{className}}"/>.</returns>
                public {{newStaticNew}}static {{className}} New() => _classMemoryPool.Allocate();

                /// <summary>Returns this instance to the memory pool.</summary>
                public {{newInstanceDelete}}void Delete() => _classMemoryPool.Free(this);

                /// <summary>Returns the static memory pool associated with this class.</summary>
                /// <returns>The existing <see cref="MemoryPool{T}"/> of type <see cref="{{className}}"/>.</returns>
                public {{newStaticGetPool}}static MemoryPool<{{className}}> GetClassMemoryPool() => _classMemoryPool;
            }

            """;

        context.AddSource($"{className}_MemoryPool.g.cs", SourceText.From(glue, Encoding.UTF8));
    }
}
