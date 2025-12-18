// -----------------------------------------------------------------------
// <copyright file="ICrashDialogProvider.cs" company="Sage.Net">
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

namespace Sage.Net.Diagnostics;

/// <summary>
/// Represents a provider for displaying crash dialogs to the user.
/// </summary>
public interface ICrashDialogProvider
{
    /// <summary>
    /// Displays a crash dialog to the user with error details after a critical exception occurs.
    /// </summary>
    /// <param name="ex">The exception that caused the crash. Contains details about the error.</param>
    /// <param name="dumpPath">The path to the dump file that was written.</param>
    void ShowCrashDialog(Exception ex, string dumpPath);
}
