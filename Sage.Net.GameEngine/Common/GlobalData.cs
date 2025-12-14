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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
#if !RETAIL_COMPTAIBLE_CRC
using System.Diagnostics;
using System.Reflection;
using System.Text;
#endif

namespace Sage.Net.GameEngine.Common;

/// <summary>A class to manage all the global data.</summary>
public class GlobalData : SubsystemBase
{
    private static readonly FieldParse[] GlobalDataFieldParseTable =
    [
        new(nameof(Windowed), Ini.ParseBool, null, (int)Marshal.OffsetOf<GlobalData>(nameof(Windowed))),
        new(nameof(XResolution), Ini.ParseInt32, null, (int)Marshal.OffsetOf<GlobalData>(nameof(XResolution))),
        new(nameof(YResolution), Ini.ParseInt32, null, (int)Marshal.OffsetOf<GlobalData>(nameof(YResolution))),
        new(nameof(MapName), Ini.ParseAsciiString, null, (int)Marshal.OffsetOf<GlobalData>(nameof(MapName))),
        new(nameof(MoveHintName), Ini.ParseAsciiString, null, (int)Marshal.OffsetOf<GlobalData>(nameof(MoveHintName))),
        new(nameof(UseTrees), Ini.ParseBool, null, (int)Marshal.OffsetOf<GlobalData>(nameof(UseTrees))),
        new("UseFPSLimit", Ini.ParseBool, null, (int)Marshal.OffsetOf<GlobalData>(nameof(UseFpsLimit))),
        new(nameof(DumpAssetUsage), Ini.ParseBool, null, (int)Marshal.OffsetOf<GlobalData>(nameof(DumpAssetUsage))),
        new(
            nameof(FramesPerSecondLimit),
            Ini.ParseInt32,
            null,
            (int)Marshal.OffsetOf<GlobalData>(nameof(FramesPerSecondLimit))
        ),
    ];

    private static GlobalData? _theOriginal;

    private string _userDataDir;
    private string _userDataLeafName;
    private GlobalData? _next;

    /// <summary>Gets or sets the writable global data.</summary>
    public static GlobalData? TheWritableGlobalData { get; set; }

    /// <summary>Gets the global data.</summary>
    public static GlobalData? TheGlobalData => TheWritableGlobalData;

    /// <summary>Gets or sets the map name.</summary>
    /// <remarks>This is a hack and will go away, eventually.</remarks>
    public string MapName { get; set; } = string.Empty;

    /// <summary>Gets or sets the move hint name.</summary>
    public string MoveHintName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether to use trees.</summary>
    public bool UseTrees { get; set; }

    /// <summary>Gets or sets a value indicating whether to use the FPS limit.</summary>
    public bool UseFpsLimit { get; set; }

    /// <summary>Gets or sets a value indicating whether to dump asset usage.</summary>
    public bool DumpAssetUsage { get; set; }

    /// <summary>Gets or sets the frame rate limit.</summary>
    public int FramesPerSecondLimit { get; set; }

    /// <summary>Gets or sets a value indicating whether the game is running in windowed mode.</summary>
    public bool Windowed { get; set; }

    /// <summary>Gets or sets the X resolution.</summary>
    public int XResolution { get; set; }

    /// <summary>Gets or sets the Y resolution.</summary>
    public int YResolution { get; set; }

    /// <summary>Gets or sets the CRC of the executable.</summary>
    public uint ExeCrc { get; set; }

    /// <summary>Initializes the <see cref="ExeCrc"/>.</summary>
    public override void Initialize() => ExeCrc = GenerateExeCrc();

    /// <summary>Resets the global data to the original state.</summary>
    public override void Reset()
    {
        Debug.Assert(ReferenceEquals(this, TheWritableGlobalData), $"Calling reset on wrong {nameof(GlobalData)}");

        while (!ReferenceEquals(TheWritableGlobalData, _theOriginal))
        {
            GlobalData? current = TheWritableGlobalData;
            GlobalData? next = current?._next;
            current?.Dispose();
            TheWritableGlobalData = next;
        }

        Debug.Assert(TheWritableGlobalData?._next is null, $"{nameof(_theOriginal)} is not original.");
        Debug.Assert(ReferenceEquals(TheWritableGlobalData, _theOriginal), "Oops.");
    }

    /// <inheritdoc/>
    /// <remarks>This is a no-op.</remarks>
    public override void UpdateCore() { }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "This is intentional to either log or ignore exceptions, making it exception safe."
    )]
    [SuppressMessage(
        "Roslynator",
        "RCS1075:Avoid empty catch clause that catches System.Exception",
        Justification = "This is intentional to either log or ignore exceptions, making it exception safe."
    )]
    private static uint GenerateExeCrc()
    {
        const int blockSize = 65_536;
        Crc exeCrc = new();
#if RETAIL_COMPTAIBLE_CRC
        exeCrc.Set(0x0003_B6FB_2CFU);
        Debug.WriteLine($"Fake EXE CRC is 0x{exeCrc.Value:X8}");
#else
        var assemblyFile = Assembly.GetExecutingAssembly().Location;
        try
        {
            using FileStream assemblyFileStream = File.OpenRead(assemblyFile);
            using BinaryReader reader = new(assemblyFileStream, Encoding.Default, leaveOpen: true);

            var crcBlock = new byte[blockSize];
            var amountRead = 0;
            while ((amountRead = reader.Read(crcBlock)) > 0)
            {
                exeCrc.Compute(crcBlock.AsSpan()[..amountRead]);
            }

            Debug.WriteLine($"EXE CRC is 0x{exeCrc.Value:X8}");
        }
        catch (Exception ex)
        {
            Debug.Fail("Executable file has failed to open", ex.ToString());
        }
#endif

        var version = VersionHelper.GetVersionNumber();
        exeCrc.Compute(BitConverter.GetBytes(version));

        try
        {
            using FileStream skirmishScriptsStream = File.OpenRead(
                Path.Combine("Data", "Scripts", "SkirmishScripts.scb")
            );

            using BinaryReader reader = new(skirmishScriptsStream, Encoding.Default, leaveOpen: true);
            var crcBlock = new byte[blockSize];
            var amountRead = 0;
            while ((amountRead = reader.Read(crcBlock)) > 0)
            {
                exeCrc.Compute(crcBlock.AsSpan()[..amountRead]);
            }
        }
        catch (Exception)
        {
            // Swallow all errors
        }

        try
        {
            using FileStream multiplayerScriptsStream = File.OpenRead(
                Path.Combine("Data", "Scripts", "MultiplayerScripts.scb")
            );

            using BinaryReader reader = new(multiplayerScriptsStream, Encoding.Default, leaveOpen: true);
            var crcBlock = new byte[blockSize];
            var amountRead = 0;
            while ((amountRead = reader.Read(crcBlock)) > 0)
            {
                exeCrc.Compute(crcBlock.AsSpan()[..amountRead]);
            }
        }
        catch (Exception)
        {
            // Swallow all errors
        }

        Debug.WriteLine(
            $"EXE+Version({VersionHelper.GetVersionNumber() >> 16}.{VersionHelper.GetVersionNumber() & 0xFFFF})+SCB CRC is 0x{exeCrc.Value:X8}"
        );

        return exeCrc.Value;
    }

    private GlobalData NewOverride()
    {
        Debug.Assert(TheWritableGlobalData is not null, "No existing data.");

        GlobalData previous = TheWritableGlobalData;
        GlobalData @override = new()
        {
            _userDataDir = previous._userDataDir,
            _userDataLeafName = previous._userDataLeafName,
            _next = previous,
        };

        TheWritableGlobalData = @override;

        return @override;
    }
}
