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
using System.Text;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a simple INI file parser.</summary>
public class Ini
{
    /// <summary>The maximum number of characters per line.</summary>
    public const int MaxCharsPerLine = 1028;

    /// <summary>The block parse delegate.</summary>
    /// <param name="ini">The INI instance.</param>
    public delegate void BlockParse(Ini ini);

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

    /// <summary>Loads an INI file or all files in a directory with the specified load type and transfer operation.</summary>
    /// <param name="fileDirName">The path to the INI file or directory to be loaded.</param>
    /// <param name="loadType">Specifies how the INI data should be processed, using the <see cref="IniLoadType"/> enumeration.</param>
    /// <param name="transfer">Optional transfer operation for custom data handling.</param>
    /// <param name="recurse">Indicates whether to recursively load files in subdirectories if the target is a directory. Default is true.</param>
    /// <returns>The total number of files successfully loaded.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileDirName"/> is null.</exception>
    /// <exception cref="FileLoadException">Thrown if no files could be loaded from the specified file or directory.</exception>
    public uint LoadFileDirectory(
        string fileDirName,
        IniLoadType loadType,
        TransferOperation? transfer,
        bool recurse = true
    )
    {
        ArgumentNullException.ThrowIfNull(fileDirName);

        const string ext = ".ini";

        var filesRead = 0U;
        var iniDir = fileDirName;
        var iniFile = fileDirName;

        if (iniDir.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
        {
            iniDir = iniDir[..^ext.Length];
        }

        if (!iniFile.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
        {
            iniFile += ext;
        }

        if (File.Exists(iniFile))
        {
            filesRead += Load(iniFile, loadType, transfer);
        }

        filesRead += LoadDirectory(iniDir, loadType, transfer, recurse);
        return filesRead == 0
            ? throw new FileLoadException($"Unable to open the INI file/directory {iniFile}")
            : filesRead;
    }

    /// <summary>Loads all INI files in a specified directory using the provided load type and transfer operation.</summary>
    /// <param name="dirName">The path to the directory containing INI files to be loaded.</param>
    /// <param name="loadType">Specifies the processing method for the INI data, using the <see cref="IniLoadType"/> enumeration.</param>
    /// <param name="transfer">An optional transfer operation for tailored data handling.</param>
    /// <param name="recurse">Indicates whether to traverse subdirectories to load files recursively. Default is true.</param>
    /// <returns>The total number of INI files successfully loaded.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="dirName"/> is null, empty, or only whitespace.</exception>
    public uint LoadDirectory(string dirName, IniLoadType loadType, TransferOperation? transfer, bool recurse = true)
    {
        if (string.IsNullOrWhiteSpace(dirName))
        {
            throw new InvalidOperationException("Invalid directory name.");
        }

        dirName += Path.DirectorySeparatorChar;
        var files = Directory.GetFiles(
            dirName,
            ".ini",
            new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = recurse }
        );

        var filesRead = files
            .Where(file =>
                !file.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || !file.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            )
            .Aggregate(0U, (current, file) => current + Load(file, loadType, transfer));

        return files
            .Where(file =>
                file.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || file.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            )
            .Aggregate(filesRead, (current, file) => current + Load(file, loadType, transfer));
    }

    /// <summary>Processes and loads an INI file based on the specified load type and optional transfer operation.</summary>
    /// <param name="filePath">The full path to the INI file to be loaded.</param>
    /// <param name="loadType">Specifies the method of processing the file, using the <see cref="IniLoadType"/> enumeration.</param>
    /// <param name="transfer">An optional transfer operation for custom data handling during the load process.</param>
    /// <returns>Always <c>1</c>, as long as the file could be processed.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an unknown block is encountered or the file cannot be processed as expected.</exception>
    public uint Load(string filePath, IniLoadType loadType, TransferOperation? transfer)
    {
        Transfer = transfer;
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
                            throw new InvalidOperationException(message, ex);
                        }

#if DEBUG
                        CurrentBlockStart = "NO_BLOCK";
#endif
                    }
                    else
                    {
                        var message = $"[LINE: {LineNumber} - FILE: '{FileName}'] Unknown block '{token}'";
                        Debug.Fail(message);
                        throw new InvalidOperationException(message);
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
}
