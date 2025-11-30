// -----------------------------------------------------------------------
// <copyright file="ArchiveFileSystem.cs" company="Sage.Net">
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
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Win32;
using Sage.Net.Core.GameEngine.Common.Audio;
using Sage.Net.Core.GameEngine.Common.Io;

namespace Sage.Net.Core.GameEngine.Common.Subsystems;

/// <summary>
/// Archive file system.
/// </summary>
public class ArchiveFileSystem : SubsystemBase
{
    /// <summary>
    /// The name of the music big file.
    /// </summary>
    public const string MusicBig = "Music.big";

    private const string BigFileIdentifier = "BIGF";

    /// <summary>
    /// Gets the archive files.
    /// </summary>
    protected Dictionary<string, BigFile> ArchiveFiles { get; } = [];

    /// <summary>
    /// Gets or sets the root directory.
    /// </summary>
    protected ArchivedDirectoryInfo RootDirectory { get; set; } = new();

    /// <summary>
    /// Opens a BIG file.
    /// </summary>
    /// <param name="fileName">The file to open.</param>
    /// <returns>A new instance of <see cref="BigFile"/> on success; <see langword="null"/> otherwise.</returns>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "We want to catch all exceptions for logging and return null."
    )]
    public static BigFile? OpenArchiveFile(string fileName)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(0x0104);
        try
        {
            using FileStream fs = File.OpenRead(fileName);

            Debug.WriteLine($"Opening BIG file {fileName}.");

            using BinaryReader reader = new(fs, Encoding.ASCII, leaveOpen: true);
            var identifier = reader.ReadBytes(4);
            if (Encoding.ASCII.GetString(identifier) != BigFileIdentifier)
            {
                Debug.Fail($"Error reading BIG file identifier in file {fileName}.");
                return null;
            }

            var archiveFileSize = reader.ReadInt32();
            Debug.WriteLine($"Size of archive file is {archiveFileSize} bytes.");

            var littleFilesCount = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
            Debug.WriteLine($"{littleFilesCount} are contained in archive.");

            // Skip to the beginning of the directory listing.
            _ = reader.BaseStream.Seek(0x10, SeekOrigin.Begin);

            BigFile archiveFile = new(fileName, string.Empty);
            for (var i = 0; i < littleFilesCount; i++)
            {
                var fileOffset = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
                var fileSize = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));

                var pathIndex = -1;
                do
                {
                    ++pathIndex;
                    _ = fs.Read(buffer, pathIndex, 1);
                } while (buffer[pathIndex] != 0);

                var fileNameIndex = pathIndex;
                while (
                    fileNameIndex >= 0
                    && buffer[fileNameIndex] != Path.DirectorySeparatorChar
                    && buffer[fileNameIndex] != Path.AltDirectorySeparatorChar
                )
                {
                    --fileNameIndex;
                }

                ArchivedFileInfo fileInfo = new(
                    Encoding
                        .ASCII.GetString(buffer, fileNameIndex + 1, pathIndex - fileNameIndex - 1)
                        .ToUpperInvariant(),
                    fileName,
                    (uint)fileOffset,
                    (uint)fileSize
                );

                buffer[fileNameIndex + 1] = 0;
                var path = Encoding.ASCII.GetString(buffer);
                var debugPath = fileInfo.FileName;
                Debug.WriteLine($"Adding file {debugPath} to archive file {fileInfo.FileName}, file number {i}");

                archiveFile.AddFile(path, fileInfo);
            }

            archiveFile.AttachFile(fs);
            return archiveFile;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cannot open file {fileName} for parsing.", ex.ToString());
            return null;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Initializes the archive file system.
    /// </summary>
    /// <remarks>
    /// This will load all BIG files from the game directory. The game directory is resolved as follows:
    /// <list type="number">
    /// <item>If running on Windows, use the registry to find the game directory.</item>
    /// <item>Otherwise, use the current directory.</item>
    /// </list>
    /// </remarks>
    public override void Initialize()
    {
        string? path = null;
        if (OperatingSystem.IsWindows())
        {
            path =
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath", null)
                    as string
                ?? Registry.GetValue(
                    @"HKEY_CURRENT_USER\SOFTWARE\Electronic Arts\EA Games\Generals",
                    "InstallPath",
                    null
                ) as string;
        }

        _ = LoadBigFilesFromDirectory(path ?? string.Empty, "*.big");
        if (!string.IsNullOrEmpty(GameEngineSystem.TheGlobalData?.ModBig))
        {
            _ = LoadBigFilesFromDirectory(path ?? string.Empty, $"*{GameEngineSystem.TheGlobalData.ModBig}");
        }
    }

    /// <inheritdoc/>
    /// <note>This is a no-op.</note>
    public override void Reset() { }

    /// <inheritdoc/>
    /// <note>This is a no-op.</note>
    public override void UpdateBase() { }

    /// <summary>
    /// Closes an archive file.
    /// </summary>
    /// <param name="fileName">The file to close.</param>
    public void CloseArchiveFile([NotNull] string fileName)
    {
        if (ArchiveFiles.Count < 1)
        {
            return;
        }

        if (fileName.Equals(MusicBig, StringComparison.OrdinalIgnoreCase))
        {
            GameEngineSystem.TheAudio!.StopAudio(AudioAffects.Music);

            // No need to turn off other audio, as the lookups will just fail.
        }

        Debug.Assert(
            fileName.Equals(MusicBig, StringComparison.OrdinalIgnoreCase),
            $"Attempting to close Archive file '{fileName}', need to add code to handle its shutdown correctly."
        );

        ArchiveFiles[fileName].Dispose();
        _ = ArchiveFiles.Remove(fileName);
    }

    /// <summary>
    /// Loads all BIG files from a directory.
    /// </summary>
    /// <param name="dir">The directory to load from.</param>
    /// <param name="fileMask">The mask to use to load the files.</param>
    /// <param name="overwrite">Whether to overwrite existing BIG files or not. <see langword="false"/> by default.</param>
    /// <returns><see langword="true"/> on success; <see langword="false"/> otherwise.</returns>
    public bool LoadBigFilesFromDirectory(string dir, string fileMask, bool overwrite = false)
    {
        var actuallyAdded = false;
        var files = Directory.GetFiles(dir, fileMask);
        foreach (var file in files)
        {
            using BigFile? archiveFile = OpenArchiveFile(file);
            if (archiveFile is null)
            {
                continue;
            }

            Debug.WriteLine($"Loading {file} into the directory tree.");
            LoadIntoDirectoryTree(archiveFile, overwrite);
            ArchiveFiles[file] = archiveFile;
            Debug.WriteLine($"{file} inserted into the archive file map.");
            actuallyAdded = true;
        }

        return actuallyAdded;
    }

    /// <summary>
    /// Loads mods into the directory tree.
    /// </summary>
    public void LoadMods()
    {
        if (!string.IsNullOrEmpty(GameEngineSystem.TheGlobalData?.ModBig))
        {
            using BigFile? archiveFile = OpenArchiveFile(GameEngineSystem.TheGlobalData.ModBig);
            if (archiveFile is not null)
            {
                Debug.WriteLine($"Loading {GameEngineSystem.TheGlobalData.ModBig} into the directory tree.");
                LoadIntoDirectoryTree(archiveFile, overwrite: true);
                ArchiveFiles[GameEngineSystem.TheGlobalData.ModBig] = archiveFile;
                Debug.WriteLine($"{GameEngineSystem.TheGlobalData.ModBig} inserted into the archive file map.");
            }
            else
            {
                Debug.WriteLine($"Could not load {GameEngineSystem.TheGlobalData.ModBig} into the directory tree.");
            }
        }

        if (string.IsNullOrEmpty(GameEngineSystem.TheGlobalData?.ModDir))
        {
            return;
        }

#if DEBUG
        var result
#else
        _
#endif
        = LoadBigFilesFromDirectory(
            GameEngineSystem.TheGlobalData.ModDir,
            $"*.{GameEngineSystem.TheGlobalData.ModBigCustomExtension ?? "big"}",
            overwrite: true
        );
        Debug.Assert(result, $"Loading mods from {GameEngineSystem.TheGlobalData.ModDir} failed.");
    }

    /// <summary>
    /// Loads a BIG file into the directory tree.
    /// </summary>
    /// <param name="archiveFile">The file to load.</param>
    /// <param name="overwrite">Whether to overwirte existing BIG files or not. <see langword="false"/> by default.</param>
    protected void LoadIntoDirectoryTree([NotNull] BigFile archiveFile, bool overwrite = false)
    {
        IEnumerable<string> fileNameList = archiveFile.GetFileListInDirectory(string.Empty, "*", true);
        foreach (var rawFileName in fileNameList)
        {
            var fileName = rawFileName.ToUpperInvariant();
            var tokens = fileName.Split('\\', '/', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
            {
                continue;
            }

            ArchivedDirectoryInfo dirInfo = GetOrCreateDirectoryPath(tokens);
            InsertFileIntoDirectory(dirInfo, tokens[^1], archiveFile, overwrite, fileName);
        }
    }

    private static void InsertFileIntoDirectory(
        ArchivedDirectoryInfo dirInfo,
        string fileToken,
        BigFile archiveFile,
        bool overwrite,
        string fullFileName
    )
    {
        if (!dirInfo.Files.TryGetValue(fileToken, out LinkedList<BigFile>? fileLocations))
        {
            fileLocations = [];
            dirInfo.Files[fileToken] = fileLocations;
        }

        if (overwrite)
        {
            fileLocations.AddFirst(archiveFile);
        }
        else
        {
            fileLocations.AddLast(archiveFile);
        }

#if DEBUG
        LogFileInsertion(fileLocations, archiveFile, overwrite, fullFileName);
#endif
    }

#if DEBUG
    private static void LogFileInsertion(
        LinkedList<BigFile> fileLocations,
        BigFile archiveFile,
        bool overwrite,
        string fileName
    )
    {
        if (fileLocations.Count >= 2)
        {
            if (overwrite)
            {
                // We added to First. The one we shadowed is Next.
                BigFile newFile = fileLocations.First!.Value;
                BigFile oldFile = fileLocations.First.Next!.Value;

                Debug.WriteLine(
                    $"Adding file {fileName}, archived in {newFile.Name}, overwriting same file in {oldFile.Name}"
                );
            }
            else
            {
                // We added to Last. The one shadowing us is Previous.
                BigFile newFile = fileLocations.Last!.Value;
                BigFile oldFile = fileLocations.Last.Previous!.Value;

                Debug.WriteLine(
                    $"Adding file {fileName}, archived in {newFile.Name}, overwritten by same file in {oldFile.Name}"
                );
            }
        }
        else
        {
            Debug.WriteLine($"Adding file {fileName}, archived in {archiveFile.Name}");
        }
    }
#endif

    private ArchivedDirectoryInfo GetOrCreateDirectoryPath(string[] tokens)
    {
        ArchivedDirectoryInfo dirInfo = RootDirectory;
        var pathAccumulator = string.Empty;

        // Traverse/Create Directories (all tokens except the last one)
        for (var i = 0; i < tokens.Length - 1; i++)
        {
            var token = tokens[i];
            pathAccumulator = Path.Combine(pathAccumulator, token);

            if (!dirInfo.Directories.TryGetValue(token, out ArchivedDirectoryInfo? nextDir))
            {
                nextDir = new ArchivedDirectoryInfo { DirectoryName = token, Path = pathAccumulator };
                dirInfo.Directories[token] = nextDir;
            }

            dirInfo = nextDir;
        }

        return dirInfo;
    }
}
