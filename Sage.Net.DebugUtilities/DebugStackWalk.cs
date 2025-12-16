// -----------------------------------------------------------------------
// <copyright file="DebugStackWalk.cs" company="Sage.Net">
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

using System.Diagnostics;
using System.Reflection;

namespace Sage.Net.DebugUtilities;

/// <summary>A collection of utilities for capturing information about the immediate callsite on the current thread.</summary>
public static class DebugStackWalk
{
    /// <summary>Captures information about the immediate callsite on the current thread.</summary>
    /// <param name="skipFrames">The number of stack frames to skip before capturing. Does not include the frame of this method itself.</param>
    /// <returns>A <see cref="CapturedCallsite"/> containing display name, module version ID, metadata token and IL offset.</returns>
    public static CapturedCallsite Capture(int skipFrames = 0)
    {
        var st = new StackTrace(skipFrames + 1, fNeedFileInfo: false);
        StackFrame frame = st.GetFrame(0) ?? throw new InvalidOperationException("No stack frame available.");
        MethodBase method = frame.GetMethod() ?? throw new InvalidOperationException("No method available.");
        var methodName = FormatMethod(method);

        return new CapturedCallsite(
            MethodDisplayName: methodName,
            ModuleVersionId: method.Module.ModuleVersionId,
            MethodMetadataToken: method.MetadataToken,
            IlOffset: frame.GetILOffset()
        );
    }

    /// <summary>Formats a method symbol along with its IL offset in hexadecimal when non-zero.</summary>
    /// <param name="symbol">The human-readable method symbol.</param>
    /// <param name="ilOffset">The IL offset for the site within the method.</param>
    /// <returns>The formatted symbol, optionally suffixed with <c>+0x&lt;offset&gt;</c>.</returns>
    public static string FormatSymbolWithOffset(string symbol, int ilOffset) =>
        ilOffset != 0 ? $"{symbol}+0x{ilOffset:x}" : symbol;

    /// <summary>Builds a simple, fully qualified method name for display.</summary>
    /// <param name="method">The method to format.</param>
    /// <returns>The formatted method name.</returns>
    private static string FormatMethod(MethodBase method)
    {
        Type? dt = method.DeclaringType;
        var typeName = dt?.FullName ?? dt?.Name ?? string.Empty;
        return string.IsNullOrEmpty(typeName) ? method.Name : $"{typeName}.{method.Name}";
    }
}
