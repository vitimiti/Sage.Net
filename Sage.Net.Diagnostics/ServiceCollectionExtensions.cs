// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sage.Net">
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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sage.Net.Diagnostics;

/// <summary>
/// Provides extension methods for configuring and managing services within a service collection
/// related to Sage.Net diagnostics functionality.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Extension methods.")]
[SuppressMessage(
    "csharpsquid",
    "S1144:Unused private types or members should be removed",
    Justification = "Extension methods."
)]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Extension methods for registering Sage.Net diagnostics services.
    /// </summary>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers diagnostics related services and binds <see cref="DumpOptions"/> and <see cref="ProfilerOptions"/> from configuration.
        /// </summary>
        /// <param name="configuration">The application configuration root used to bind options.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is <see langword="null"/>.</exception>
        public IServiceCollection AddSageDiagnostics(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _ = services.Configure<DumpOptions>(configuration.GetSection("Sage:Diagnostics:Dumps"));
            _ = services.Configure<ProfilerOptions>(configuration.GetSection("Sage:Diagnostics:Profiling"));

            _ = services.AddSingleton<IDumpService, DumpService>();
            _ = services.AddSingleton<Profiler>();

            return services;
        }
    }
}
