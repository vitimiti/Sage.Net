// -----------------------------------------------------------------------
// <copyright file="LogContext.cs" company="Sage.Net">
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

using Microsoft.Extensions.Logging;

namespace Sage.Net.LoggerHelper;

/// <summary>
/// Provides utility methods for creating scoped logging contexts with correlation identifiers.
/// </summary>
public static class LogContext
{
    private const string CorrelationIdKey = "CorrelationId";

    /// <summary>
    /// Creates a logging scope for a specific operation and associates it with a unique correlation identifier.
    /// </summary>
    /// <param name="logger">The logger instance where the scoped context is applied.</param>
    /// <param name="operationName">The name of the operation to associate with the logging scope.</param>
    /// <returns>Returns an <see cref="IDisposable"/> instance representing the logging scope. If no scope is created, null is returned.</returns>
    public static IDisposable? BeginOperation(ILogger logger, string operationName)
    {
        ArgumentNullException.ThrowIfNull(logger);

        var correlationId = Guid.NewGuid().ToString("N")[..8];
        return logger.BeginScope(
            new Dictionary<string, object> { ["Operation"] = operationName, [CorrelationIdKey] = correlationId }
        );
    }
}
