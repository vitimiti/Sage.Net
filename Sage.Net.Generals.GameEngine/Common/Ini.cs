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

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a simple INI file parser.</summary>
public partial class Ini
{
    /// <summary>Defines a process used to parse and handle fields in a given context.</summary>
    /// <param name="ini">The INI instance.</param>
    /// <param name="instance">The instance to be processed.</param>
    /// <param name="store">The store to be updated.</param>
    /// <param name="userData">User data.</param>
    public delegate void FieldParseProcess(Ini ini, ref object? instance, ref object? store, object? userData);

    /// <summary>The maximum number of characters per line.</summary>
    public const int MaxCharsPerLine = 1028;

    private const string BlockEnd = "END";
    private const string NoBlock = "NO_BLOCK";

    private string? _currentTokenLine;
    private int _currentTokenIndex;

    /// <summary>The block parse delegate.</summary>
    /// <param name="ini">The INI instance.</param>
    public delegate void BlockParse(Ini ini);

    public delegate void BuildMultiIniField(MultiIniFieldParse p);

    private static readonly Dictionary<string, BlockParse> TypeTable = [];

    /// <summary>Gets the separators.</summary>
    public static IList<char> Separators => [' ', '\n', '\r', '\t', '='];

    /// <summary>Gets the separators with percent.</summary>
    public static IList<char> SeparatorsPercent => [' ', '\n', '\r', '\t', '=', '%'];

    /// <summary>Gets the separators with colon.</summary>
    public static IList<char> SeparatorsColon => [' ', '\n', '\r', '\t', '=', ':'];

    /// <summary>Gets the separators with quote.</summary>
    public static IList<char> SeparatorsQuote => ['\"', '\n', '='];

    /// <summary>Gets or sets the file name.</summary>
    public string FileName { get; protected set; } = "None";

    /// <summary>Gets or sets the load type.</summary>
    public IniLoadType LoadType { get; protected set; } = IniLoadType.Invalid;

    /// <summary>Gets or sets the line number.</summary>
    public uint LineNumber { get; protected set; }

    /// <summary>Gets or sets a value indicating whether the end of file has been reached.</summary>
    public bool EndOfFile { get; protected set; }

    /// <summary>Gets or sets the INI stream.</summary>
    protected MemoryStream? IniStream { get; set; }

#if DEBUG
    /// <summary>Gets or sets the current block start.</summary>
    protected string? CurrentBlockStart { get; set; }
#endif

    private static TransferOperation? Transfer { get; set; }

    /// <summary>Prepares the specified file for reading and parsing as an INI file.</summary>
    /// <param name="filePath">The full path to the file to be prepared for reading.</param>
    /// <param name="loadType">Specifies how the INI data should be processed, using the <see cref="IniLoadType"/> enumeration.</param>
    /// <exception cref="FileLoadException">Thrown when another file is already open or cannot be properly loaded.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file cannot be found or opened.</exception>
    protected void PrepareFile(string filePath, IniLoadType loadType)
    {
        if (IniStream is not null)
        {
            var message = $"Cannot open file '{filePath}', file already open.";
            Debug.Fail(message);
            throw new FileLoadException(message);
        }

        FileStream? fileStream;

        try
        {
            fileStream = File.OpenRead(filePath);
        }
        catch (Exception ex)
        {
            var message = $"Cannot open file '{filePath}'.";
            Debug.Fail(message, ex.ToString());
            throw new FileNotFoundException(message, filePath, ex);
        }

        IniStream = new MemoryStream();
        fileStream.CopyTo(IniStream);
        fileStream.Dispose();

        LoadType = loadType;
    }

    /// <summary>Unprepares the INI file for reading and parsing.</summary>
    protected void UnprepareFile()
    {
        IniStream?.Dispose();
        IniStream = null;

        FileName = "None";
        LoadType = IniLoadType.Invalid;
        LineNumber = 0;
        EndOfFile = false;

        Transfer = null;
    }

    /// <summary>Reads a line from the INI file.</summary>
    /// <returns>The line read from the file.</returns>
    protected string ReadLine()
    {
        Debug.Assert(IniStream is not null, $"INI file '{FileName}' is not open.");

        if (EndOfFile)
        {
            return string.Empty;
        }

        const int maxCharsPerLine = 0x0404;
        var buffer = ArrayPool<byte>.Shared.Rent(maxCharsPerLine);

        try
        {
            var length = FillBuffer(buffer, maxCharsPerLine);
            if (length == 0 && EndOfFile)
            {
                return string.Empty;
            }

            LineNumber++;

            if (length == maxCharsPerLine)
            {
                Debug.Fail(
                    $"Buffer too small ({maxCharsPerLine}) and was truncated, increase {nameof(maxCharsPerLine)}"
                );
            }

            var strLen = 0;
            while (strLen < length && buffer[strLen] != 0)
            {
                strLen++;
            }

            var str = Encoding.ASCII.GetString(buffer, 0, strLen);
            if (Transfer is null)
            {
                return str;
            }

            var strBytes = Encoding.ASCII.GetBytes(str);
            unsafe
            {
                fixed (byte* pStrBytes = strBytes)
                {
                    Transfer.TransferUser(pStrBytes, strBytes.Length);
                }
            }

            return Encoding.ASCII.GetString(strBytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static BlockParse? FindBlockParse(string token) =>
        (
            from block in TypeTable
            where block.Key.Equals(token, StringComparison.Ordinal)
            select block.Value
        ).FirstOrDefault();

    private static bool IsEndToken(string token) => token.Equals(BlockEnd, StringComparison.OrdinalIgnoreCase);

    private static bool TryGetFirstToken(string? line, [NotNullWhen(true)] out string? token)
    {
        token = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var parts = line.Split(Separators.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        token = parts[0];
        return true;
    }

    private int FillBuffer(byte[] buffer, int maxCharsPerLine)
    {
        var length = 0;
        while (length < maxCharsPerLine)
        {
            var b = IniStream!.ReadByte();

            // EOF?
            if (b == -1)
            {
                EndOfFile = true;
                break;
            }

            var c = (byte)b;

            // CR?
            if (c == (byte)'\n')
            {
                // Stop reading, do not add newline to buffer
                break;
            }

            Debug.Assert(
                c != (byte)'\t',
                $"Tab characters are not allowed in INI files ({FileName}). Please, check your editor settings. Line Number {LineNumber}."
            );

            c = c switch
            {
                (byte)';' => 0,
                > 0 and < 32 => (byte)' ',
                _ => c,
            };

            buffer[length++] = c;
        }

        return length;
    }

    [MemberNotNull(nameof(_currentTokenLine))]
    private void PrepareNextLine()
    {
        // Ensure we have a line to parse
        while (_currentTokenLine is null || _currentTokenIndex >= _currentTokenLine.Length)
        {
            var line = ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                _currentTokenLine = line;
                _currentTokenIndex = 0;
                return;
            }

            // If EOF and nothing more, throw like INI_INVALID_DATA
            if (EndOfFile)
            {
                throw new InvalidDataException(
                    $"Unexpected end of INI while reading token (File: '{FileName}', Line: {LineNumber})."
                );
            }
        }
    }

    private void SkipSeparators(char[] seps)
    {
        while (
            _currentTokenIndex < _currentTokenLine!.Length
            && Array.IndexOf(seps, _currentTokenLine[_currentTokenIndex]) >= 0
        )
        {
            _currentTokenIndex++;
        }
    }

    private bool TryExtractToken(char[] seps, [NotNullWhen(true)] out string? token)
    {
        var start = _currentTokenIndex;
        while (
            _currentTokenIndex < _currentTokenLine!.Length
            && Array.IndexOf(seps, _currentTokenLine[_currentTokenIndex]) < 0
        )
        {
            _currentTokenIndex++;
        }

        if (start < _currentTokenIndex)
        {
            token = _currentTokenLine[start.._currentTokenIndex];
            return true;
        }

        token = null;
        return false;
    }

#pragma warning disable IDE0022 // Use expression body for methods
    private string GetCurrentBlockForError()
    {
#if DEBUG
        return CurrentBlockStart ?? NoBlock;
#else
        return NoBlock;
#endif
    }
#pragma warning restore IDE0022 // Use expression body for methods

    private void ParseKnownField(
        FieldParse fp,
        int index,
        ref object what,
        MultiIniFieldParse parseTable,
        string fieldForErrorMessage
    )
    {
        try
        {
            var instance = what;

            var byteOffset = fp.Offset + (int)parseTable.GetNthExtraOffset(index);
            var store = byteOffset == 0 ? what : new OffsetStore(what, byteOffset);

            fp.Parse(this, ref instance, ref store, fp.UserData);

            if (instance is not null)
            {
                what = instance;
            }
        }
        catch (Exception ex)
        {
            var message =
                $"[LINE: {LineNumber} - FILE: '{FileName}'] Error reading field '{fieldForErrorMessage}' of block '{GetCurrentBlockForError()}'";

            Debug.Fail(message, ex.ToString());
            throw new InvalidOperationException(message, ex);
        }
    }

    private bool TryParseKnownField(string field, ref object what, MultiIniFieldParse parseTable)
    {
        for (var i = 0; i < parseTable.Count; i++)
        {
            FieldParse fp = parseTable.GetNthFieldParse(i);
            if (!fp.Token.Equals(field, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ParseKnownField(fp, i, ref what, parseTable, field);
            return true;
        }

        return false;
    }

    private void ThrowUnknownField(string field)
    {
        var message =
            $"[LINE: {LineNumber} - FILE: '{FileName}'] Unknown field '{field}' in block '{GetCurrentBlockForError()}'";
        Debug.Fail(message);
        throw new InvalidOperationException(message);
    }

    private void ThrowMissingEndToken()
    {
        var message =
            $"Error parsing block '{GetCurrentBlockForError()}', in INI file '{FileName}'. Missing '{BlockEnd}' token.";
        Debug.Fail(message);
        throw new InvalidDataException(message);
    }

    private sealed record OffsetStore(object Base, int ByteOffset);
}
