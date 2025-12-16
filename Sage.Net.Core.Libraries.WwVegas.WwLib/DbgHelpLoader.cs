// -----------------------------------------------------------------------
// <copyright file="DbgHelpLoader.cs" company="Sage.Net">
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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Diagnostics.NETCore.Client;

namespace Sage.Net.Core.Libraries.WwVegas.WwLib;

public static class DbgHelpLoader
{
    private static readonly Lock Sync = new();

    private static int _refCount;
    private static bool _failed;
    private static bool _loaded;

    public static bool IsLoaded
    {
        get
        {
            lock (Sync)
            {
                return _loaded;
            }
        }
    }

    public static bool IsFailed
    {
        get
        {
            lock (Sync)
            {
                return _failed;
            }
        }
    }

    public static bool IsLoadedFromSystem => false;

    public static bool Load()
    {
        lock (Sync)
        {
            _refCount++;

            if (_failed)
            {
                return false;
            }

            if (_loaded)
            {
                return true;
            }

            // "Load" here means "we believe diagnostics dump writing is usable".
            // In practice, DiagnosticsClient is available on all .NET 10 platforms,
            // but dump generation can still fail due to permissions/IO/etc.
            _loaded = true;
            return true;
        }
    }

    public static void Unload()
    {
        lock (Sync)
        {
            if (_refCount <= 0)
            {
                return;
            }

            _refCount--;
            if (_refCount != 0)
            {
                return;
            }

            // Reset to initial state.
            _loaded = false;
            _failed = false;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Avoid crash paths here."
    )]
    public static bool WriteDump(string dumpPath, DumpType dumpType)
    {
        lock (Sync)
        {
            if (!_loaded || _failed)
            {
                return false;
            }

            try
            {
                var client = new DiagnosticsClient(Environment.ProcessId);
                client.WriteDump(dumpType, dumpPath, logDumpGeneration: false);
                return true;
            }
            catch
            {
                _failed = true;
                return false;
            }
        }
    }
}
