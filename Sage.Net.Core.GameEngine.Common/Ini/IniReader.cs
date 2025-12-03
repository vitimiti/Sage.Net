// -----------------------------------------------------------------------
// <copyright file="IniReader.cs" company="Sage.Net">
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
using Sage.Net.Core.GameEngine.Common.Ini.IniExceptions;
using Sage.Net.Core.GameEngine.Common.Transfer;

namespace Sage.Net.Core.GameEngine.Common.Ini;

/// <summary>
/// An INI file reader.
/// </summary>
public class IniReader
{
    private string? _currentTokenLine;
    private int _currentTokenIndex;

    /// <summary>
    /// Delegate for parsing blocks.
    /// </summary>
    /// <param name="reader">The <see cref="IniReader"/> instance.</param>
    public delegate void BlockParse(IniReader reader);

    private static readonly Dictionary<string, BlockParse> TypeTable = [];

    /// <summary>
    /// Gets the separators.
    /// </summary>
    public static IList<char> Separators => [' ', '\n', '\r', '\t', '='];

    /// <summary>
    /// Gets the separators with percent.
    /// </summary>
    public static IList<char> SeparatorsPercent => [' ', '\n', '\r', '\t', '=', '%'];

    /// <summary>
    /// Gets the separators with colon.
    /// </summary>
    public static IList<char> SeparatorsColon => [' ', '\n', '\r', '\t', '=', ':'];

    /// <summary>
    /// Gets the separators with quote.
    /// </summary>
    public static IList<char> SeparatorsQuote => ['\"', '\n', '='];

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; protected set; } = "None";

    /// <summary>
    /// Gets or sets the load type.
    /// </summary>
    public IniLoadType LoadType { get; protected set; } = IniLoadType.Invalid;

    /// <summary>
    /// Gets or sets the line number.
    /// </summary>
    public uint LineNumber { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether the end of file has been reached.
    /// </summary>
    public bool EndOfFile { get; protected set; }

    /// <summary>
    /// Gets or sets the INI stream.
    /// </summary>
    protected MemoryStream? IniStream { get; set; }

#if DEBUG
    /// <summary>
    /// Gets or sets the current block start.
    /// </summary>
    protected string? CurrentBlockStart { get; set; }
#endif

    private static Xfer? Xfer { get; set; }

    /// <summary>
    /// Parses the specified token into an integer value.
    /// </summary>
    /// <param name="token">The token string to parse as an integer.</param>
    /// <returns>The parsed integer value from the token.</returns>
    /// <exception cref="IniInvalidDataException">Thrown when the token cannot be parsed as an integer.</exception>
    public static int ScanInt32([NotNull] string token) =>
        !int.TryParse(token, out var result)
            ? throw new IniInvalidDataException($"Unable to parse the token {token} as an {nameof(Int32)}")
            : result;

    /// <summary>
    /// Searches for the specified token within a list of valid names, returning its index.
    /// </summary>
    /// <param name="token">The token string to locate in the name list.</param>
    /// <param name="nameList">A list of valid names to search through. Cannot be null or empty.</param>
    /// <returns>The zero-based index of the token within the name list.</returns>
    /// <exception cref="IniInvalidNameListException">
    /// Thrown when the name list is null, empty, or if the token is not found within the list.
    /// </exception>
    public static int ScanIndexList(string token, IList<string>? nameList)
    {
        if (nameList is null || nameList.Count == 0)
        {
            const string message = "Invalid name list";
            Debug.Fail(message);
            throw new IniInvalidNameListException(message);
        }

        var count = 0;
        foreach (var name in nameList)
        {
            if (name.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                return count;
            }

            count++;
        }

        var secondMessage = $"Token {token} is not a valid member of the index list";
        Debug.Fail(secondMessage);
        throw new IniInvalidNameListException(secondMessage);
    }

    /// <summary>
    /// Loads the INI file.
    /// </summary>
    /// <param name="filePath">The path to the INI file to load.</param>
    /// <param name="loadType">The <see cref="IniLoadType"/> to load the INI file with.</param>
    /// <param name="xfer">The <see cref="Transfer.Xfer"/> based transfer class to use; or <see langword="null"/> to use none.</param>
    /// <returns>Always 1, as long as the file was loaded.</returns>
    /// <exception cref="IniException">When the line's INI token couldn't find how to parse itself.</exception>
    /// <exception cref="IniUnknownTokenException">When the line's INI token is not known.</exception>
    public uint Load(string filePath, IniLoadType loadType, Xfer? xfer)
    {
        Xfer = xfer;
        PrepareFile(filePath, loadType);

        try
        {
            Debug.Assert(!EndOfFile, "EOF reached before reading any lines.");
            while (!EndOfFile)
            {
                var currentLine = ReadLine();
                var tokens = currentLine.Split(Separators.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in tokens)
                {
                    BlockParse? parse = FindBlockParse(token);
                    if (parse is not null)
                    {
#if DEBUG
                        CurrentBlockStart = currentLine;
#endif

                        try
                        {
                            parse(this);
                        }
                        catch (Exception ex)
                        {
                            var message =
                                $"Error parsing block '{token}' in INI file '{FileName}'. Error parsing file '{FileName}' (Line: '{LineNumber}').";
                            Debug.Fail(message, ex.ToString());
                            throw new IniException(message, ex);
                        }

#if DEBUG
                        CurrentBlockStart = "NO_BLOCK";
#endif
                    }
                    else
                    {
                        var message = $"[LINE: {LineNumber} - FILE: '{FileName}'] Unknown block '{token}'";
                        Debug.Fail(message);
                        throw new IniUnknownTokenException(message);
                    }
                }
            }
        }
        finally
        {
            UnprepareFile();
        }

        return 1;
    }

    /// <summary>
    /// Loads all INI files in a directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to load the INI files from.</param>
    /// <param name="loadType">The <see cref="IniLoadType"/> to load the INI files as.</param>
    /// <param name="xfer">The <see cref="Transfer.Xfer"/> based transfer class.</param>
    /// <param name="recursive"><see langword="true"/> to load all subdirectories recursively; <see langword="false"/> otherwise. <see langword="true"/> by default.</param>
    /// <returns>The number of INI files that were loaded.</returns>
    /// <exception cref="DirectoryNotFoundException">When the given <paramref name="directoryPath"/> doesn't exist.</exception>
    public uint LoadDirectory(string directoryPath, IniLoadType loadType, Xfer? xfer, bool recursive = true)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory '{directoryPath}' does not exist.");
        }

        var fileNames = Directory.GetFiles(
            directoryPath,
            "*.ini",
            new EnumerationOptions
            {
                RecurseSubdirectories = recursive,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
            }
        );

        return fileNames.Aggregate(0U, (current, fileName) => current + Load(fileName, loadType, xfer));
    }

    /// <summary>
    /// Loads a specific INI file by name and/or INI files from a directory.
    /// </summary>
    /// <param name="fileDirectoryPath">The file/directory path to load from.</param>
    /// <param name="loadType">The <see cref="IniLoadType"/> to load the INI files as.</param>
    /// <param name="xfer">The <see cref="Transfer.Xfer"/> based transfer class.</param>
    /// <param name="recursive"><see langword="true"/> to load all subdirectories recursively; <see langword="false"/> otherwise. <see langword="true"/> by default.</param>
    /// <returns>The number of INI files that were loaded.</returns>
    /// <exception cref="FileNotFoundException">If no files could be loaded.</exception>
    /// <example>
    /// Using <c>LoadFileDirectory(Path.Combine(GameDirectory, "Data", "INI", "Armor"), IniLoadType.Overwrite, xfer: null)</c> loads <c>Data\INI\Armor.ini</c> and all INI files in <c>Data\INI\Armor</c>.
    /// </example>
    public uint LoadFileDirectory(
        [NotNull] string fileDirectoryPath,
        IniLoadType loadType,
        Xfer? xfer,
        bool recursive = true
    )
    {
        var iniDir = fileDirectoryPath;
        var iniFile = fileDirectoryPath;

        const string ext = "ini";
        if (iniDir.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            iniDir = iniDir[..^ext.Length];
        }

        if (!iniFile.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            iniFile += ".ini";
        }

        var filesRead = 0U;
        if (File.Exists(iniFile))
        {
            filesRead += Load(iniFile, loadType, xfer);
        }

        filesRead += LoadDirectory(iniDir, loadType, xfer, recursive);

        return filesRead == 0
            ? throw new FileNotFoundException($"No INI files found in '{fileDirectoryPath}'.", fileDirectoryPath)
            : filesRead;
    }

    /// <summary>
    /// Retrieves the next token from the current line or subsequent lines in the INI file,
    /// using the specified set of separators. If no separators are provided, a default set is used.
    /// </summary>
    /// <param name="separators">
    /// An optional array of characters to use as delimiters for separating tokens.
    /// If <see langword="null"/>, the default set of separators is used.
    /// </param>
    /// <returns>
    /// The next token as a string extracted from the INI file. If no more tokens are available, it returns an empty string.
    /// </returns>
    /// <exception cref="IniInvalidDataException">
    /// Thrown if the data in the INI file is invalid and cannot be parsed.
    /// </exception>
    public string GetNextToken(char[]? separators = null)
    {
        var seps = separators ?? [.. Separators];
        while (true)
        {
            PrepareNextLine();

            // Skip separators
            SkipSeparators(seps);

            // Extract token
            if (TryExtractToken(seps, out var token))
            {
                return token;
            }

            // No token on this line, move to next line
            _currentTokenLine = null;
            _currentTokenIndex = 0;
        }
    }

    /// <summary>
    /// Retrieves the next token from the current line or subsequent lines in the INI file,
    /// using the specified set of separators. If no separators are provided, a default set is used.
    /// </summary>
    /// <param name="separators">
    /// An optional array of characters to use as delimiters for separating tokens.
    /// If <see langword="null"/>, the default set of separators is used.
    /// </param>
    /// <returns>
    /// The next token as a string extracted from the INI file. If no more tokens are available, it returns <see langword="null"/>.
    /// </returns>
    public string? GetNextTokenOrNull(char[]? separators = null)
    {
        try
        {
            return GetNextToken(separators);
        }
        catch (IniInvalidDataException)
        {
            return null;
        }
    }

    /// <summary>
    /// Retrieves the next string from the input, handling quoted and unquoted tokens appropriately.
    /// </summary>
    /// <returns>The next string from the input. If the token is enclosed in quotes, the string within the quotes is returned. If no token is available, an empty string is returned.</returns>
    public string GetNextString()
    {
        var result = string.Empty;
        var token = GetNextTokenOrNull();
        if (token is null)
        {
            return result;
        }

        if (token[0] != '\"')
        {
            return token;
        }

        StringBuilder sb = new();
        if (token.Length > 1)
        {
            _ = sb.Append(token.AsSpan(1));
        }

        token = GetNextTokenOrNull([.. SeparatorsQuote]);
        if (token is not null)
        {
            if (token.Length > 1 && token[1] != '\t')
            {
                _ = sb.Append(' ');
            }

            _ = sb.Append(token);
        }
        else
        {
            if (sb.Length > 0 && sb[^1] == '"')
            {
                sb.Length--;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Checks if the file name is valid.
    /// </summary>
    /// <param name="filePath">The filepath to the file name.</param>
    /// <returns><see langword="true"/> if the file name ends with INI; <see langword="false"/> otherwise. This is case insensitive.</returns>
    protected static bool IsValidIniFileName([NotNull] string filePath) =>
        filePath.EndsWith(".ini", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Prepares the INI file for reading.
    /// </summary>
    /// <param name="filePath">The file path to prepare.</param>
    /// <param name="loadType">The <see cref="IniLoadType"/> to load the INI file as.</param>
    /// <exception cref="IniFileAlreadyOpenException">When the INI file is already open.</exception>
    /// <exception cref="FileNotFoundException">When the INI file couldn't be open.</exception>
    protected void PrepareFile(string filePath, IniLoadType loadType)
    {
        if (IniStream is not null)
        {
            var message = $"Cannot open file '{filePath}', file already open.";
            Debug.Fail(message);
            throw new IniFileAlreadyOpenException(message);
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

    /// <summary>
    /// Unprepares the INI file.
    /// </summary>
    protected void UnprepareFile()
    {
        IniStream?.Dispose();
        IniStream = null;

        FileName = "None";
        LoadType = IniLoadType.Invalid;
        LineNumber = 0;
        EndOfFile = false;

        Xfer = null;

        // Reset tokenizer state
        _currentTokenLine = null;
        _currentTokenIndex = 0;
    }

    /// <summary>
    /// Reads a line from the INI file.
    /// </summary>
    /// <returns>The <see cref="string"/> containing the next line. It may be <see cref="string.Empty"/>.</returns>
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

            // Check for max
            if (length == maxCharsPerLine)
            {
                Debug.Fail(
                    $"Buffer too small ({maxCharsPerLine}) and was truncated, increase {nameof(maxCharsPerLine)}"
                );
            }

            // Extract string up to the first null terminator (simulating C string behavior)
            // This effectively strips comments and internal nulls
            var strLen = 0;
            while (strLen < length && buffer[strLen] != 0)
            {
                strLen++;
            }

            var str = Encoding.ASCII.GetString(buffer, 0, strLen);
            if (Xfer is null)
            {
                return str;
            }

            var strBytes = Encoding.ASCII.GetBytes(str);
            Xfer.User(strBytes);
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
                throw new IniInvalidDataException(
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
}
