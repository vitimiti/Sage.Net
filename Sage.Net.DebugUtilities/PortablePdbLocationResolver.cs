// -----------------------------------------------------------------------
// <copyright file="PortablePdbLocationResolver.cs" company="Sage.Net">
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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Sage.Net.DebugUtilities;

/// <summary>Provides source location resolution services for Portable PDBs.</summary>
public sealed class PortablePdbLocationResolver
{
    private readonly ConcurrentDictionary<Guid, ModulePdb> _byMvId = new();

    /// <summary>Scans all currently loaded assemblies and registers their modules so that their Portable PDBs (if available) can be used to resolve source information.</summary>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid exceptions, simply keep looping."
    )]
    public void RegisterLoadedModules()
    {
        foreach (Assembly? asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Module[] modules;

            try
            {
                modules = asm.GetModules();
            }
            catch
            {
                continue;
            }

            foreach (Module m in modules)
            {
                TryRegisterModule(m);
            }
        }
    }

    /// <summary>Attempts to resolve the file name and line number for a captured callsite using Portable PDB data.</summary>
    /// <param name="site">The captured callsite to resolve.</param>
    /// <returns>The file name (without path) and line number if resolvable; otherwise <see langword="null"/>.</returns>
    public (string File, int Line)? TryResolveFileLine(CapturedCallsite site)
    {
        ArgumentNullException.ThrowIfNull(site);

        if (site.IlOffset < 0)
        {
            return null;
        }

        if (!_byMvId.TryGetValue(site.ModuleVersionId, out ModulePdb? mod))
        {
            return null;
        }

        ModulePdb.SourceLoc? loc = mod.TryResolve(site.MethodMetadataToken, site.IlOffset);
        return loc is null ? null : (Path.GetFileName(loc.Value.FilePath), loc.Value.Line);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Avoid exceptions.")]
    private void TryRegisterModule(Module m)
    {
        Guid mvId;
        string assemblyPath;

        try
        {
            mvId = m.ModuleVersionId;
            assemblyPath = m.FullyQualifiedName;
        }
        catch
        {
            return;
        }

        _ = _byMvId.TryAdd(mvId, ModulePdb.TryCreate(assemblyPath) ?? ModulePdb.Missing());
    }

    private sealed class ModulePdb
    {
        private readonly MetadataReader? _reader;

        private ModulePdb(MetadataReader? reader) => _reader = reader;

        public static ModulePdb Missing() => new(null);

        public static ModulePdb? TryCreate(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                return null;
            }

            var pdbPath = Path.ChangeExtension(assemblyPath, "pdb");
            if (!File.Exists(pdbPath))
            {
                return null;
            }

            using FileStream stream = File.OpenRead(pdbPath);
            var provider = MetadataReaderProvider.FromPortablePdbStream(stream);
            MetadataReader reader = provider.GetMetadataReader();

            return new ModulePdb(reader);
        }

        public SourceLoc? TryResolve(int methodToken, int ilOffset)
        {
            if (_reader is null)
            {
                return null;
            }

            var methodHandle = (MethodDefinitionHandle)MetadataTokens.Handle(methodToken);
            MethodDebugInformationHandle debugHandle = methodHandle.ToDebugInformationHandle();
            MethodDebugInformation mdi = _reader.GetMethodDebugInformation(debugHandle);

            SequencePoint? best = null;
            foreach (
                SequencePoint sp in mdi.GetSequencePoints()
                    .Where(sp => !sp.IsHidden)
                    .TakeWhile(sp => sp.Offset <= ilOffset)
            )
            {
                best = sp;
            }

            if (best is null)
            {
                return null;
            }

            Document doc = _reader.GetDocument(best.Value.Document);
            var path = _reader.GetString(doc.Name);
            return new SourceLoc(path, best.Value.StartLine);
        }

        public readonly record struct SourceLoc(string FilePath, int Line);
    }
}
