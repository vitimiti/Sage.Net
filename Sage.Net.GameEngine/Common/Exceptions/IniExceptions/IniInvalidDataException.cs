// -----------------------------------------------------------------------
// <copyright file="IniInvalidDataException.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common.Exceptions.IniExceptions;

/// <summary>Thrown when attempting to process invalid data from an open INI file.</summary>
public class IniInvalidDataException : IniException
{
    /// <summary>Initializes a new instance of the <see cref="IniInvalidDataException"/> class.</summary>
    public IniInvalidDataException() { }

    /// <summary>Initializes a new instance of the <see cref="IniInvalidDataException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public IniInvalidDataException(string? message)
        : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="IniInvalidDataException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public IniInvalidDataException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
