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

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Sage.Net.Core.GameEngine.Common;
using Sage.Net.Generals.Libraries.BaseTypes;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Provides functionality for managing global data.</summary>
public partial class GlobalData : SubsystemBase
{
    private const int MaxGlobalLights = 3;

    private static readonly FieldParse[] GlobalDataFieldParseTable = [];

    private readonly ILogger _logger;
    private readonly RgbColor[] _terrainAmbient = new RgbColor[MaxGlobalLights];
    private readonly RgbColor[] _terrainDiffuse = new RgbColor[MaxGlobalLights];
    private readonly TerrainLighting[][] _terrainLightning;
    private readonly Coord3D[] _terrainLightPos = new Coord3D[MaxGlobalLights];
    private readonly TerrainLighting[][] _terrainObjectsLightning;

    private GlobalData? _next;

    /// <summary>Initializes a new instance of the <see cref="GlobalData"/> class.</summary>
    /// <param name="logger">The logger to use for logging.</param>
    public GlobalData(ILogger logger)
    {
        _logger = logger;
        TheOriginal ??= this;

        _terrainLightning = new TerrainLighting[Enum.GetValues<TimeOfDay>().Length][];
        _terrainObjectsLightning = new TerrainLighting[Enum.GetValues<TimeOfDay>().Length][];
        for (var i = 0; i < MaxGlobalLights; i++)
        {
            _terrainLightning[i] = new TerrainLighting[MaxGlobalLights];
            _terrainObjectsLightning[i] = new TerrainLighting[MaxGlobalLights];
        }

        TerrainLightning = new JaggedArrayList<TerrainLighting>(_terrainLightning);
        TerrainObjectsLightning = new JaggedArrayList<TerrainLighting>(_terrainObjectsLightning);

        _ = SetTimeOfDay(TimeOfDay.Afternoon);
    }

    /// <summary>Gets or sets the writable global data.</summary>
    public static GlobalData? TheWritableGlobalData { get; set; }

    /// <summary>Gets the global data.</summary>
    public static GlobalData? TheGlobalData => TheWritableGlobalData;

    /// <summary>Gets or sets the command line data.</summary>
    public CommandLineData CommandLineData { get; set; } = new();

    /// <summary>Gets or sets the executable CRC.</summary>
    public uint ExeCrc { get; set; }

    /// <summary>Gets or sets a value indicating whether the shell map is on.</summary>
    public bool ShellMapOn { get; set; } = true;

    /// <summary>Gets the terrain ambient colors.</summary>
    public IList<RgbColor> TerrainAmbient => _terrainAmbient;

    /// <summary>Gets the terrain diffuse colors.</summary>
    public IList<RgbColor> TerrainDiffuse => _terrainDiffuse;

    /// <summary>Gets the terrain lighting colors.</summary>
    public IList<IList<TerrainLighting>> TerrainLightning { get; }

    /// <summary>Gets the terrain light positions.</summary>
    public IList<Coord3D> TerrainLightPosition => _terrainLightPos;

    /// <summary>Gets the terrain objects lighting colors.</summary>
    public IList<IList<TerrainLighting>> TerrainObjectsLightning { get; }

    /// <summary>Gets or sets the time of day.</summary>
    public TimeOfDay TimeOfDay { get; set; } = TimeOfDay.Afternoon;

    /// <summary>Gets or sets the viewport height scale.</summary>
    public float ViewportHeightScale { get; set; } = .8F;

    /// <summary>Gets or sets a value indicating whether the game is running in windowed mode.</summary>
    public bool Windowed { get; set; }

    private static GlobalData? TheOriginal { get; set; }

    /// <summary>Parses the game data definition.</summary>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="ini">The game data definition to parse.</param>
    public static void ParseGameDataDefinition(ILogger logger, Ini ini)
    {
        ArgumentNullException.ThrowIfNull(ini);

        if (TheWritableGlobalData is not null && ini.LoadType is not IniLoadType.Multifile)
        {
            if (ini.LoadType is IniLoadType.CreateOverrides)
            {
                _ = NewOverride();
            }
        }
        else
        {
            TheWritableGlobalData ??= new GlobalData(logger);
        }

        object writableGlobalData = TheWritableGlobalData;
        ini.InitializeFromIni(ref writableGlobalData, GlobalDataFieldParseTable);
        TheWritableGlobalData = (GlobalData)writableGlobalData;
    }

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

    /// <summary>Updates the time of day and adjusts terrain lighting accordingly.</summary>
    /// <param name="timeOfDay">The desired time of day to set, represented by the <see cref="TimeOfDay"/> enum.</param>
    /// <returns>Returns <see langword="true"/> if the time of day was successfully updated; otherwise, <see langword="false"/>.</returns>
    public bool SetTimeOfDay(TimeOfDay timeOfDay)
    {
        if ((int)timeOfDay >= Enum.GetValues<TimeOfDay>().Length || timeOfDay < TimeOfDay.Morning)
        {
            return false;
        }

        TimeOfDay = timeOfDay;
        for (var i = 0; i < MaxGlobalLights; i++)
        {
            _terrainAmbient[i] = _terrainLightning[(int)timeOfDay][i].Ambient;
            _terrainDiffuse[i] = _terrainLightning[(int)timeOfDay][i].Diffuse;
            _terrainLightPos[i] = _terrainLightning[(int)timeOfDay][i].LightPosition;
        }

        return true;
    }

    /// <summary>Parses and applies custom definitions related to the global data configuration.</summary>
    public void ParseCustomDefinition()
    {
        if (AddonCompatibility.HasFullViewportDat())
        {
            ViewportHeightScale = 1F;
        }
    }

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

    private static GlobalData? NewOverride()
    {
        Debug.Assert(TheWritableGlobalData is not null, "No existing data.");

        GlobalData previous = TheWritableGlobalData;
        GlobalData ovr = previous.CloneForOverride();

        ovr._next = previous;
        TheWritableGlobalData = ovr;
        return ovr;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "This is not meant to throw!"
    )]
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

    private GlobalData CloneForOverride() =>
        new(_logger)
        {
            CommandLineData = CommandLineData.CloneForGlobalDataOverride(),
            ExeCrc = ExeCrc,
            Windowed = Windowed,
        };

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, Message = "EXE CRC is 0x{ExeCrc:X8}")]
        public static partial void CrcValue(ILogger logger, uint exeCrc);

        [LoggerMessage(LogLevel.Debug, Message = "EXE+Version({Major}.{Minor})+SCB CRC is 0x{ExeCrc:X8}")]
        public static partial void FinalCrcValue(ILogger logger, uint major, uint minor, uint exeCrc);
    }

    private sealed class ArrayListAdapter<T>(T[] actualArray) : IList<T>
    {
        private const string ErrorMessage = "Fixed-size.";

        public int Count => actualArray.Length;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => actualArray[index];
            set => actualArray[index] = value;
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)actualArray).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item) => Array.IndexOf(actualArray, item);

        public bool Contains(T item) => ((IEnumerable<T>)actualArray).Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => actualArray.CopyTo(array, arrayIndex);

        public void Insert(int index, T item) => throw new NotSupportedException(ErrorMessage);

        public void RemoveAt(int index) => throw new NotSupportedException(ErrorMessage);

        public void Add(T item) => throw new NotSupportedException(ErrorMessage);

        public void Clear() => throw new NotSupportedException(ErrorMessage);

        public bool Remove(T item) => throw new NotSupportedException(ErrorMessage);
    }

    private sealed class JaggedArrayList<T> : IList<IList<T>>
    {
        private const string ErrorMessage = "Fixed-size.";

        private readonly IList<T>[] _rows;

        public JaggedArrayList(T[][] data)
        {
            _rows = new IList<T>[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
#pragma warning disable IDE0028 // Collection initialization can be simplified
                _rows[i] = new ArrayListAdapter<T>(data[i]);
#pragma warning restore IDE0028 // Collection initialization can be simplified
            }
        }

        public int Count => _rows.Length;

        public bool IsReadOnly => false;

        public IList<T> this[int index]
        {
            get => _rows[index];
            set => throw new NotSupportedException("Cannot replace rows of a fixed-size jagged array.");
        }

        public IEnumerator<IList<T>> GetEnumerator() => ((IEnumerable<IList<T>>)_rows).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(IList<T> item) => Array.IndexOf(_rows, item);

        public void Insert(int index, IList<T> item) => throw new NotSupportedException(ErrorMessage);

        public void RemoveAt(int index) => throw new NotSupportedException(ErrorMessage);

        public void Add(IList<T> item) => throw new NotSupportedException(ErrorMessage);

        public void Clear() => throw new NotSupportedException(ErrorMessage);

        public bool Contains(IList<T> item) => ((ICollection<IList<T>>)_rows).Contains(item);

        public void CopyTo(IList<T>[] array, int arrayIndex) => _rows.CopyTo(array, arrayIndex);

        public bool Remove(IList<T> item) => throw new NotSupportedException(ErrorMessage);
    }
}
