// -----------------------------------------------------------------------
// <copyright file="RefPackOptions.cs" company="Sage.Net">
// Copyright (c) Sage.Net. All rights reserved.
// Licensed under the MIT license.
// See LICENSE.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Sage.Net.Compression.Eac.RefPack.Options;

/// <summary>
/// Options for the RefPack compression.
/// </summary>
public class RefPackOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use the quick compression mode.
    /// </summary>
    /// <remarks>
    /// When enabled, the quick mode will reduce the overhead of the hash calculation.
    /// </remarks>
    public bool QuickCompression { get; set; }
}
