// -----------------------------------------------------------------------
// <copyright file="SdlCrashDialogProvider.cs" company="Sage.Net">
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sage.Net.Diagnostics.Sdl3;

/// <summary>
/// Provides an implementation of <see cref="ICrashDialogProvider"/> that displays a crash dialog
/// using an SDL-based message box. This dialog notifies the user of a critical error, along with
/// details of the exception and the crash dump path.
/// </summary>
public partial class SdlCrashDialogProvider : ICrashDialogProvider
{
    /// <summary>
    /// Displays a crash dialog to notify the user of a critical error. The dialog includes
    /// details of the exception and the location of the generated crash dump file.
    /// </summary>
    /// <param name="ex">The exception that caused the crash. Provides details such as the exception type and message.</param>
    /// <param name="dumpPath">The file path where the crash dump has been generated. Used to inform the user about the dump location.</param>
    public void ShowCrashDialog(Exception ex, string dumpPath)
    {
        ArgumentNullException.ThrowIfNull(ex);

        const string title = "Sage.Net - Engine Crash";
        var message = $"""
            The engine has encountered a critical error and must close.

            Details: {ex.GetType().Name}
            {ex.Message}

            A crash dump has been (if nothing else failed) generated in {dumpPath}.
            """;

        _ = Sdl.ShowSimpleMessageBox(Sdl.MessageBoxError, title, message, nint.Zero);
    }

    private static partial class Sdl
    {
        public const uint MessageBoxError = 0x0010;

        [LibraryImport("SDL", EntryPoint = "SDL_ShowSimpleMessageBox", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static partial bool ShowSimpleMessageBox(uint flags, string title, string message, nint window);
    }
}
