// -----------------------------------------------------------------------
// <copyright file="TransferException.cs" company="Sage.Net">
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

namespace Sage.Net.GameEngine.Common.Exceptions.TransferExceptions;

/// <summary>The base exception for all transfer operations exceptions.</summary>
public class TransferException : SageException
{
    /// <summary>Initializes a new instance of the <see cref="TransferException"/> class.</summary>
    public TransferException() { }

    /// <summary>Initializes a new instance of the <see cref="TransferException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public TransferException(string? message)
        : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="TransferException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TransferException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
