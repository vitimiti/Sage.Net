// -----------------------------------------------------------------------
// <copyright file="GlobalData.cs" company="Sage.Net">
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
using System.Text;
using Microsoft.Extensions.Logging;
using Sage.Net.Core.GameEngine.Common;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Provides functionality for managing global data.</summary>
public partial class GlobalData : SubsystemBase
{
    private readonly ILogger _logger;

    private readonly GlobalData? _next = null;

    /// <summary>Initializes a new instance of the <see cref="GlobalData"/> class.</summary>
    /// <param name="logger">The logger to use for logging.</param>
    public GlobalData(ILogger logger)
    {
        _logger = logger;
        TheOriginal ??= this;
    }

    /// <summary>Gets or sets the writable global data.</summary>
    public static GlobalData? TheWritableGlobalData { get; set; }

    /// <summary>Gets the global data.</summary>
    public static GlobalData? TheGlobalData => TheWritableGlobalData;

    /// <summary>Gets or sets the command line data.</summary>
    public CommandLineData CommandLineData { get; set; } = new();

    /// <summary>Gets or sets the executable CRC.</summary>
    public uint ExeCrc { get; set; }

    /// <summary>Gets or sets a value indicating whether the game is running in windowed mode.</summary>
    public bool Windowed { get; set; }

    private static GlobalData? TheOriginal { get; set; }

    /// <inheritdoc/>
    /// <remarks>Generates the executable CRC.</remarks>
    public override void Initialize() => ExeCrc = GenerateCrc();

    /// <inheritdoc/>
    /// <remarks>Resets the global data.</remarks>
    public override void Reset()
    {
        Debug.Assert(ReferenceEquals(this, TheWritableGlobalData), $"Calling reset on wrong {nameof(GlobalData)}");

        while (!ReferenceEquals(TheWritableGlobalData, TheOriginal))
        {
            GlobalData? next = TheWritableGlobalData?._next;
            TheWritableGlobalData?.Dispose();
            TheWritableGlobalData = next;
        }

        Debug.Assert(TheWritableGlobalData?._next is null, $"{nameof(TheOriginal)} is not original.");
        Debug.Assert(
            ReferenceEquals(TheWritableGlobalData, TheOriginal),
            $"{nameof(TheWritableGlobalData)} is not original."
        );
    }

    /// <inheritdoc/>
    public override void UpdateCore() { }

    /// <inheritdoc/>
    /// <remarks>Disposes the global data.</remarks>
    protected override void Dispose(bool disposing)
    {
        Debug.Assert(TheWritableGlobalData?._next is null, $"{nameof(TheOriginal)} is not original.");

        if (ReferenceEquals(this, TheOriginal))
        {
            TheOriginal = null;
            TheWritableGlobalData = null;
        }

        base.Dispose(disposing);
    }

    private unsafe uint GenerateCrc()
    {
        const int blockSize = 65_536;
        Crc exeCrc = new();
        var assemblyFile = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (assemblyFile is not null)
        {
            try
            {
                using FileStream fp = File.OpenRead(assemblyFile);
                using BinaryReader br = new(fp, Encoding.Default, leaveOpen: true);
                Span<byte> crcBlock = stackalloc byte[blockSize];
                int amountRead;
                while ((amountRead = br.Read(crcBlock)) > 0)
                {
                    fixed (byte* pCrcBlock = crcBlock)
                    {
                        exeCrc.Compute(pCrcBlock, amountRead);
                    }
                }

                Log.CrcValue(_logger, exeCrc.Value);
            }
            catch (Exception ex)
            {
                Debug.Fail("Executable file has failed to open.", ex.ToString());
            }
        }

        var version = 0U;
        if (VersionHelper.TheVersion is not null)
        {
            version = VersionHelper.TheVersion.VersionNumber;
            exeCrc.Compute(&version, sizeof(uint));
        }

        try
        {
            // TODO: Add the working directory path to allow running the game from outside the original stuff.
            using FileStream fp = File.OpenRead(Path.Combine("Data", "Scripts", "SkirmishScripts.scb"));
            using BinaryReader br = new(fp, Encoding.Default, leaveOpen: true);
            Span<byte> crcBlock = stackalloc byte[blockSize];
            int amountRead;
            while ((amountRead = br.Read(crcBlock)) > 0)
            {
                fixed (byte* pCrcBlock = crcBlock)
                {
                    exeCrc.Compute(pCrcBlock, amountRead);
                }
            }
        }
        catch
        {
            // Swallow all errors and keep going.
        }

        try
        {
            // TODO: Add the working directory path to allow running the game from outside the original stuff.
            using FileStream fp = File.OpenRead(Path.Combine("Data", "Scripts", "MultiplayerScripts.scb"));
            using BinaryReader br = new(fp, Encoding.Default, leaveOpen: true);
            Span<byte> crcBlock = stackalloc byte[blockSize];
            int amountRead;
            while ((amountRead = br.Read(crcBlock)) > 0)
            {
                fixed (byte* pCrcBlock = crcBlock)
                {
                    exeCrc.Compute(pCrcBlock, amountRead);
                }
            }
        }
        catch
        {
            // Swallow all errors and keep going.
        }

        Log.FinalCrcValue(_logger, version >> 16, version & 0xFFFF, exeCrc.Value);
        return exeCrc.Value;
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, Message = "EXE CRC is 0x{ExeCrc:X8}")]
        public static partial void CrcValue(ILogger logger, uint exeCrc);

        [LoggerMessage(LogLevel.Debug, Message = "EXE+Version({Major}.{Minor})+SCB CRC is 0x{ExeCrc:X8}")]
        public static partial void FinalCrcValue(ILogger logger, uint major, uint minor, uint exeCrc);
    }
}
