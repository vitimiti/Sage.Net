// -----------------------------------------------------------------------
// <copyright file="Ini.cs" company="Sage.Net">
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
using Sage.Net.GameEngine.Common.Exceptions.IniExceptions;

namespace Sage.Net.GameEngine.Common;

/// <summary>
/// Represents a parser callback that reads a block from an INI file using the provided <see cref="Ini"/> reader.
/// </summary>
/// <param name="ini">The INI reader positioned at the beginning of the block to parse.</param>
public delegate void IniBlockParse(Ini ini);

/// <summary>
/// Base INI reader that supports loading one or multiple INI files and dispatching parsing to registered block parsers.
/// </summary>
public class Ini : IDisposable
{
    /// <summary>Determines the maximum number of characters allowed per line in an INI file.</summary>
    public const int MaxCharsPerLine = 1028;

#if RTS_ZERO_HOUR
    /// <summary>Size of the buffered read chunk used by <see cref="ReadLine()"/> when buffered IO is enabled.</summary>
    protected const int ReadBufferSize = 8192;
#endif

    // TODO: complete this table
    private static readonly BlockParse[] TheTypeTable =
    [
        new("AIData", null!),
        new("Animation", null!),
        new("Armor", null!),
        new("AudioEvent", null!),
        new("AudioSettings", null!),
        new("Bridge", null!),
        new("Campaign", null!),
#if RTS_ZERO_HOUR
        new("ChallengeGenerals", null!),
#endif
        new("CommandButton", null!),
        new("CommandMap", null!),
        new("CommandSet", null!),
        new("ControlBarScheme", null!),
        new("ControlBarResizer", null!),
        new("CrateData", null!),
        new("Credits", null!),
        new("WindowTransition", null!),
        new("DamageFX", null!),
        new("DialogEvent", null!),
        new("DrawGroupInfo", null!),
        new("EvaEvent", null!),
        new("FXList", null!),
        new("GameData", null!),
        new("InGameUI", null!),
        new("Locomotor", null!),
        new("Language", null!),
        new("MapCache", null!),
        new("MapData", null!),
        new("MappedImage", null!),
        new("MiscAudio", null!),
        new("Mouse", null!),
        new("MouseCursor", null!),
        new("MultiplayerColor", null!),
#if RTS_ZERO_HOUR
        new("MultiplayerStartingMoneyChoice", null!),
#endif
        new("OnlineChatColors", null!),
        new("MultiplayerSettings", null!),
        new("MusicTrack", null!),
        new("Object", null!),
        new("ObjectCreationList", null!),
        new("ObjectReskin", null!),
        new("ParticleSystem", null!),
        new("PlayerTemplate", null!),
        new("Road", null!),
        new("Science", null!),
        new("Rank", null!),
        new("SpecialPower", null!),
        new("ShellMenuScheme", null!),
        new("Terrain", null!),
        new("Upgrade", null!),
        new("Video", null!),
        new("WaterSet", null!),
        new("WaterTransparency", null!),
#if RTS_ZERO_HOUR
        new("Weather", null!),
#endif
        new("Weapon", null!),
        new("WebpageURL", null!),
        new("HeaderTemplate", null!),
        new("StaticGameLOD", null!),
        new("DynamicGameLOD", null!),
        new("LODPreset", null!),
        new("BenchProfile", null!),
        new("ReallyLowMHz", null!),
#if RTS_ZERO_HOUR
        new("ScriptAction", null!),
        new("ScriptCondition", null!),
#endif
    ];

#if RTS_ZERO_HOUR
    private readonly byte[] _readBuffer = new byte[ReadBufferSize];
    private int _readBufferNext;
    private int _readBufferUsed;
#endif
    private readonly byte[] _buffer = new byte[MaxCharsPerLine];

#if DEBUG_CRASHING
    private readonly byte[] _currentBlockStart = new byte[MaxCharsPerLine];
#endif

    /// <summary>Gets the default token separators used when tokenizing a line.</summary>
    public static IList<char> Separators => " \n\r\t=".ToCharArray();

    /// <summary>Gets the token separators including the percent character.</summary>
    public static IList<char> SeparatorsPercent => " \n\r\t=%".ToCharArray();

    /// <summary>Gets the token separators including the colon character.</summary>
    public static IList<char> SeparatorsColon => "\n\r\t=:".ToCharArray();

    /// <summary>Gets the token separators used for quoted values.</summary>
    public static IList<char> SeparatorsQuote => "\"\n=".ToCharArray();

    /// <summary>Gets or sets the name of the currently opened INI file, or <c>"None"</c> when no file is opened.</summary>
    public string FileName { get; protected set; } = "None";

    /// <summary>Gets or sets the current <see cref="IniLoadType"/> for the opened file.</summary>
    public IniLoadType LoadType { get; protected set; } = IniLoadType.Invalid;

    /// <summary>Gets or sets the line number of the last line read from the current file.</summary>
    public uint LineNumber { get; protected set; }

    /// <summary>Gets or sets a value indicating whether the end of the current file has been reached.</summary>
    public bool EndOfFile { get; protected set; }

    /// <summary>Gets the token that denotes the end of a block in the INI file.</summary>
    protected static string BlockEndToken => "END";

    /// <summary>Gets or sets the stream of the currently opened INI file.</summary>
    protected Stream? IniFile { get; set; }

    /// <summary>Gets the line buffer used by <see cref="ReadLine()"/>.</summary>
    protected ReadOnlySpan<byte> Buffer => _buffer;

#if DEBUG_CRASHING
    /// <summary>Gets a copy of the raw bytes at the start of the most recently parsed block.</summary>
    protected ReadOnlySpan<byte> CurrentBlockStart => _currentBlockStart;
#endif

    /// <summary>Gets or sets the transfer operation.</summary>
    private static TransferOperation? Transfer { get; set; }

    /// <summary>Determines whether the specified declaration is of the specified type.</summary>
    /// <param name="blockType">The type of the declaration to check.</param>
    /// <param name="blockName">The name of the declaration to check.</param>
    /// <param name="bufferToCheck">The buffer to check.</param>
    /// <returns><see langword="true"/> if the declaration is of the specified type; otherwise, <see langword="false"/>.</returns>
    public static bool IsDeclarationOfType(string blockType, string blockName, ReadOnlySpan<byte> bufferToCheck)
    {
        if (bufferToCheck.IsEmpty || string.IsNullOrEmpty(blockType) || string.IsNullOrEmpty(blockName))
        {
            return false;
        }

        var len = bufferToCheck.IndexOf((byte)0);
        if (len < 0)
        {
            len = bufferToCheck.Length;
        }

        var i = 0;

        while (i < len && IsAsciiWhiteSpace(bufferToCheck[i]))
        {
            i++;
        }

        if (i + blockType.Length > len)
        {
            return false;
        }

        if (!EqualsAsciiIgnoreCase(bufferToCheck.Slice(i, blockType.Length), blockType))
        {
            return false;
        }

        i += blockType.Length;

        if (i >= len || !IsAsciiWhiteSpace(bufferToCheck[i]))
        {
            return false;
        }

        i++;

        while (i < len && IsAsciiWhiteSpace(bufferToCheck[i]))
        {
            i++;
        }

        if (i + blockName.Length > len)
        {
            return false;
        }

        if (!EqualsAsciiIgnoreCase(bufferToCheck.Slice(i, blockName.Length), blockName))
        {
            return false;
        }

        i += blockName.Length;

        while (i < len && IsAsciiWhiteSpace(bufferToCheck[i]))
        {
            i++;
        }

        return i == len;
    }

    /// <summary>Determines whether the specified position marks the end of a block.</summary>
    /// <param name="bufferToCheck">The buffer to check.</param>
    /// <returns><see langword="true"/> if the position is at the end of the block; otherwise, <see langword="false"/>.</returns>
    public static bool IsEndOfBLock(ReadOnlySpan<byte> bufferToCheck)
    {
        if (bufferToCheck.IsEmpty)
        {
            return false;
        }

        var len = bufferToCheck.IndexOf((byte)0);
        if (len < 0)
        {
            len = bufferToCheck.Length;
        }

        var i = 0;

        while (i < len && IsAsciiWhiteSpace(bufferToCheck[i]))
        {
            i++;
        }

        if (i + BlockEndToken.Length > len)
        {
            return false;
        }

        if (!EqualsAsciiIgnoreCase(bufferToCheck.Slice(i, BlockEndToken.Length), BlockEndToken))
        {
            return false;
        }

        i += BlockEndToken.Length;

        while (i < len)
        {
            if (!IsAsciiWhiteSpace(bufferToCheck[i]))
            {
                return false;
            }

            i++;
        }

        return true;
    }

    /// <summary>Releases all resources used by this <see cref="Ini"/> instance.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Loads a single INI file and all INI files from the directory with the same base name.</summary>
    /// <param name="fileDirectoryName">The INI file or directory base name.</param>
    /// <param name="loadType">The context in which the INI is being loaded.</param>
    /// <param name="transfer">Optional transfer operation to mirror the read data to.</param>
    /// <param name="recursive">Whether to recurse into subdirectories when scanning the directory.</param>
    /// <returns>The number of files successfully read. Throws if nothing was read.</returns>
    /// <remarks>If <paramref name="fileDirectoryName"/> ends with <c>.ini</c>, that file is loaded, then the directory with the same name is scanned.</remarks>
    /// <exception cref="IniCantOpenFileException">Thrown when no INI files could be found or opened.</exception>
    public uint LoadFileDirectory(
        string fileDirectoryName,
        IniLoadType loadType,
        TransferOperation? transfer,
        bool recursive = true
    )
    {
        ArgumentNullException.ThrowIfNull(fileDirectoryName);

        const string ext = ".ini";

        var filesRead = 0U;
        var iniDir = fileDirectoryName;
        var iniFile = fileDirectoryName;

        if (iniDir.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            iniDir = iniDir[..^ext.Length];
        }

        if (!iniFile.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            iniFile += ext;
        }

        if (File.Exists(iniFile))
        {
            filesRead += Load(iniFile, loadType, transfer);
        }

        filesRead += LoadDirectory(iniDir, loadType, transfer, recursive);
        return filesRead == 0 ? throw new IniCantOpenFileException($"Cannot find INI file '{iniFile}'") : filesRead;
    }

    /// <summary>Loads all <c>.ini</c> files from the specified directory.</summary>
    /// <param name="directoryName">The directory to scan.</param>
    /// <param name="loadType">The context in which the INI files are being loaded.</param>
    /// <param name="transfer">Optional transfer operation to mirror the read data to.</param>
    /// <param name="recursive">Whether to recurse into subdirectories.</param>
    /// <returns>The number of files successfully read.</returns>
    /// <exception cref="IniInvalidDirectoryException">Thrown when the directory path is empty or does not exist.</exception>
    public uint LoadDirectory(
        string directoryName,
        IniLoadType loadType,
        TransferOperation? transfer,
        bool recursive = true
    )
    {
        if (string.IsNullOrEmpty(directoryName))
        {
            throw new IniInvalidDirectoryException("Directory name is empty.");
        }

        if (!Directory.Exists(directoryName))
        {
            throw new IniInvalidDirectoryException($"Directory '{directoryName}' does not exist.");
        }

        var root = Path.GetFullPath(directoryName);
        if (!root.EndsWith(Path.DirectorySeparatorChar))
        {
            root += Path.DirectorySeparatorChar;
        }

        var files = Directory
            .GetFiles(
                root,
                "*.ini",
                new EnumerationOptions
                {
                    RecurseSubdirectories = recursive,
                    MatchCasing = MatchCasing.CaseInsensitive,
                    IgnoreInaccessible = true,
                }
            )
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var filesRead = (
            from file in files
            let relative = Path.GetRelativePath(root, file)
            where
                !relative.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relative.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            select file
        ).Aggregate(0U, (current, file) => current + Load(file, loadType, transfer));

        return (
            from file in files
            let relative = Path.GetRelativePath(root, file)
            where
                relative.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || relative.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            select file
        ).Aggregate(filesRead, (current, file) => current + Load(file, loadType, transfer));
    }

    /// <summary>Loads and parses a single INI file.</summary>
    /// <param name="fileName">The path to the INI file to open.</param>
    /// <param name="loadType">The context in which the INI is being loaded.</param>
    /// <param name="transfer">Optional transfer operation to mirror the read data to.</param>
    /// <returns><c>1</c> when the file was read successfully.</returns>
    /// <exception cref="IniCantOpenFileException">Thrown when the file cannot be opened.</exception>
    /// <exception cref="IniUnknownTokenException">Thrown when an unknown block token is encountered.</exception>
    /// <exception cref="IniException">Thrown when a parser reports an error while reading the file.</exception>
    public uint Load(string fileName, IniLoadType loadType, TransferOperation? transfer)
    {
        Transfer = transfer;
        PrepareFile(fileName, loadType);

        try
        {
            Debug.Assert(!EndOfFile, "EOF at the beginning!");
            while (!EndOfFile)
            {
                ReadLine();
                var lineLen = 0;
                while (lineLen < _buffer.Length && _buffer[lineLen] != 0)
                {
                    lineLen++;
                }

                var currentLine = Encoding.ASCII.GetString(_buffer, 0, lineLen);
                var firstToken = currentLine
                    .Split(Separators.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(firstToken))
                {
                    continue;
                }

                IniBlockParse? parse = FindBlockParse(firstToken);
                if (parse is not null)
                {
#if DEBUG_CRASHING
                    Array.Copy(_buffer, _currentBlockStart, int.Min(_currentBlockStart.Length, _buffer.Length));
#endif
                    try
                    {
                        parse(this);
                    }
                    catch (Exception ex)
                    {
                        var message = $"Error parsing INI file '{FileName}' (Line: '{currentLine}')";
                        Debug.Fail(message, ex.ToString());
                        throw new IniException(message, ex);
                    }
#if DEBUG_CRASHING
                    Array.Clear(_currentBlockStart, 0, _currentBlockStart.Length);
                    var noBlock = "NO_BLOCK"u8.ToArray();
                    Array.Copy(noBlock, 0, _currentBlockStart, 0, int.Min(noBlock.Length, _currentBlockStart.Length));
#endif
                }
                else
                {
                    var message = $"[LINE: {LineNumber} - FILE: '{FileName}'] Unknown block '{firstToken}'";
                    Debug.Fail(message);
                    throw new IniUnknownTokenException(message);
                }
            }
        }
        finally
        {
            UnprepareFile();
        }

        return 1;
    }

    /// <summary>Determines whether the provided file name has the <c>.ini</c> extension (case-insensitive).</summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns><see langword="true"/> if the file name ends with <c>.ini</c>; otherwise, <see langword="false"/>.</returns>
    protected static bool IsValidIniFileName(string? fileName) =>
        fileName?.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>Releases the unmanaged resources used by the <see cref="Ini"/> and optionally releases the managed resources.</summary>
    /// <param name="disposing">When <see langword="true"/>, release both managed and unmanaged resources; otherwise release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            IniFile?.Dispose();
        }
    }

    /// <summary>Opens the specified INI file for reading and prepares internal buffers.</summary>
    /// <param name="fileName">Path to the INI file to open.</param>
    /// <param name="loadType">The context in which the INI is being loaded.</param>
    /// <exception cref="IniFileAlreadyOpenException">Thrown when a file is already opened.</exception>
    /// <exception cref="IniCantOpenFileException">Thrown when the file cannot be opened.</exception>
    protected void PrepareFile(string fileName, IniLoadType loadType)
    {
        if (IniFile is not null)
        {
            var message = $"Cannot open file '{fileName};, file already opened.";
            Debug.Fail(message);
            throw new IniFileAlreadyOpenException(message);
        }

        try
        {
            IniFile = File.OpenRead(fileName);
        }
        catch (Exception ex)
        {
            var message = $"Cannot open file '{fileName}'.";
            Debug.Fail(message, ex.ToString());
            throw new IniCantOpenFileException(message, ex);
        }

        MemoryStream ms = new();
        IniFile.CopyTo(ms);
        IniFile.Dispose();

        IniFile = ms;
        FileName = fileName;
        LoadType = loadType;

        Array.Clear(_buffer);

#if RTS_ZERO_HOUR
        _readBufferNext = 0;
        _readBufferUsed = 0;
#endif
    }

    /// <summary>Closes the current file and resets all internal state and buffers.</summary>
    protected void UnprepareFile()
    {
        IniFile?.Dispose();
        IniFile = null;

        FileName = "None";
        LoadType = IniLoadType.Invalid;

        EndOfFile = false;
        LineNumber = 0;

        Transfer = null;

#if RTS_ZERO_HOUR
        _readBufferNext = 0;
        _readBufferUsed = 0;
#endif
    }

    /// <summary>Reads the next line from the current INI file into <see cref="Buffer"/>, normalizing whitespace and stripping comments.</summary>
    protected void ReadLine()
    {
        Debug.Assert(IniFile is not null, "File is not open.");

        if (EndOfFile)
        {
            _buffer[0] = 0;
            return;
        }

#if RTS_ZERO_HOUR
        ReadLineRtsZeroHour();
#else
        ReadLineUnbuffered();
#endif

        TransferLineIfNeeded();
    }

    private static IniBlockParse? FindBlockParse(string token) =>
        TheTypeTable.FirstOrDefault(x => x.Token.Equals(token, StringComparison.Ordinal))?.Parse;

    private static IniFieldParse? FindFieldParse(
        FieldParse[] parseTable,
        string token,
        out int offset,
        out object? userData
    )
    {
        offset = 0;
        userData = null;

        int parseIndex;
        for (parseIndex = 0; parseIndex < parseTable.Length; parseIndex++)
        {
            if (!parseTable[parseIndex].Token.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            offset = parseTable[parseIndex].Offset;
            userData = parseTable[parseIndex].UserData;
            return parseTable[parseIndex].Parse;
        }

        if (!string.IsNullOrEmpty(parseTable[parseIndex].Token) || parseTable[parseIndex].Parse is null)
        {
            return null;
        }

        offset = parseTable[parseIndex].Offset;
        userData = token;
        return parseTable[parseIndex].Parse;
    }

    private static bool EqualsAsciiIgnoreCase(ReadOnlySpan<byte> span, string text)
    {
        if (span.Length != text.Length)
        {
            return false;
        }

        for (var j = 0; j < text.Length; j++)
        {
            var ch = text[j];
            if (ch > 0x7F)
            {
                return false;
            }

            if (ToUpperAscii(span[j]) != ToUpperAscii((byte)ch))
            {
                return false;
            }
        }

        return true;
    }

    private static byte ToUpperAscii(byte b) => b is >= (byte)'a' and <= (byte)'z' ? (byte)(b - 32) : b;

    private static bool IsAsciiWhiteSpace(byte b) =>
        b is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n' or (byte)'\f' or (byte)'\v';

#if !RTS_ZERO_HOUR
    private static byte NormalizeChar(byte ch) => !char.IsWhiteSpace((char)ch) ? ch : (byte)' ';
#endif

    private static int GetNullTerminatedLength(byte[] buffer)
    {
        var len = 0;
        while (len < buffer.Length && buffer[len] != 0)
        {
            len++;
        }

        return len;
    }

#if RTS_ZERO_HOUR
    private int ReadByteBuffered()
    {
        Debug.Assert(IniFile is not null, "File is not open.");

        if (_readBufferNext != _readBufferUsed)
        {
            return _readBuffer[_readBufferNext++];
        }

        _readBufferNext = 0;
        _readBufferUsed = IniFile.Read(_readBuffer, 0, _readBuffer.Length);
        return _readBufferUsed == 0 ? -1 : _readBuffer[_readBufferNext++];
    }

    private void ReadLineRtsZeroHour()
    {
        var outIndex = 0;
        while (outIndex != MaxCharsPerLine)
        {
            var b = ReadByteBuffered();
            if (b < 0)
            {
                EndOfFile = true;
                _buffer[outIndex] = 0;
                break;
            }

            var ch = (byte)b;
            _buffer[outIndex] = ch;

            if (ch == (byte)'\n')
            {
                _buffer[outIndex] = 0;
                break;
            }

            Debug.Assert(
                ch != (byte)'\t',
                $"Tab characters are not allowed in INI files ({FileName}). Line Number {LineNumber}."
            );

            if (ch == (byte)';')
            {
                _buffer[outIndex] = 0;
            }
            else if (ch > 0 && ch < 32)
            {
                _buffer[outIndex] = (byte)' ';
            }

            outIndex++;
        }

        if (outIndex < MaxCharsPerLine)
        {
            _buffer[outIndex] = 0;
        }

        LineNumber++;
        if (outIndex == MaxCharsPerLine)
        {
            Debug.Fail($"Buffer too small ({MaxCharsPerLine}) and was truncated, increase {nameof(MaxCharsPerLine)}.");
        }
    }
#else
    private void EnsureDoubleNullTerminator(int i)
    {
        if (i + 1 < MaxCharsPerLine)
        {
            _buffer[i + 1] = 0;
        }
    }

    private void ReadLineUnbuffered()
    {
        var isComment = false;
        var i = 0;

        while (i < MaxCharsPerLine)
        {
            var b = IniFile!.ReadByte();
            EndOfFile = b < 0;

            if (EndOfFile)
            {
                _buffer[i] = 0;
                EnsureDoubleNullTerminator(i);
                break;
            }

            var ch = (byte)b;

            Debug.Assert(
                ch != (byte)'\t',
                $"Tab characters are not allowed in INI files ({FileName}). Line Number {LineNumber}."
            );

            ch = NormalizeChar(ch);

            if (ch == (byte)';')
            {
                isComment = true;
            }

            _buffer[i] = isComment ? (byte)0 : ch;

            if (ch == (byte)'\n')
            {
                EnsureDoubleNullTerminator(i);
                break;
            }

            i++;
        }

        LineNumber++;
        if (i == MaxCharsPerLine)
        {
            Debug.Fail($"Buffer too small ({MaxCharsPerLine}) and was truncated, increase {nameof(MaxCharsPerLine)}.");
        }
    }
#endif

    private unsafe void TransferLineIfNeeded()
    {
        if (Transfer is null)
        {
            return;
        }

        var len = GetNullTerminatedLength(_buffer);

        fixed (byte* p = _buffer)
        {
            Transfer.TransferUser(p, len);
        }
    }

    private sealed record BlockParse(string Token, IniBlockParse Parse);
}
