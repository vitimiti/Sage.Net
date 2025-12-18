// -----------------------------------------------------------------------
// <copyright file="IniLoaders.cs" company="Sage.Net">
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

namespace Sage.Net.Generals.GameEngine.Common;

public partial class Ini
{
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
                        CurrentBlockStart = NoBlock;
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
}
