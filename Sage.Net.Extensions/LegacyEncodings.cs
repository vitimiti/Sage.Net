// -----------------------------------------------------------------------
// <copyright file="LegacyEncodings.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

namespace Sage.Net.Extensions;

/// <summary>
/// Provides legacy encodings used by older systems.
/// </summary>
public static class LegacyEncodings
{
    static LegacyEncodings() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <summary>
    /// Gets the encoding for ANSI code page 1252 (Western European).
    /// </summary>
    public static Encoding Ansi1252 => Encoding.GetEncoding(1252);
}
