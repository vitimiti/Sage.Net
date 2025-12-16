// -----------------------------------------------------------------------
// <copyright file="CapturedCallsite.cs" company="Sage.Net">
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

namespace Sage.Net.DebugUtilities;

/// <summary>Represents a captured callsite in the current process, including a user-friendly method name, module identity, method metadata token, and IL offset.</summary>
/// <param name="MethodDisplayName">Human-readable fully qualified method name (e.g., <c>Namespace.Type.Method</c>).</param>
/// <param name="ModuleVersionId">The <see cref="System.Reflection.Module.ModuleVersionId"/> of the declaring module.</param>
/// <param name="MethodMetadataToken">The metadata token (method definition) used to look up debug information.</param>
/// <param name="IlOffset">The IL offset within the method body for the captured frame.</param>
public record CapturedCallsite(string MethodDisplayName, Guid ModuleVersionId, int MethodMetadataToken, int IlOffset);
