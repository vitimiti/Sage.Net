// -----------------------------------------------------------------------
// <copyright file="RandomValue.cs" company="Sage.Net">
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

namespace Sage.Net.Core.GameEngine.Common;

/// <summary>Provides methods for initializing and managing random seed values used in the game engine.</summary>
/// <remarks>This class ensures the reproducibility of random sequences in both deterministic and non-deterministic modes, dependent on the compilation flags.</remarks>
public static class RandomValue
{
    private static readonly long[] TheGameClientSeed =
    [
        0xF22D_0E56,
        0x8831_26E9,
        0xC624_DD2F,
        0x0702_C49C,
        0x9E35_3F7D,
        0x6FDF_3B64,
    ];

    private static readonly long[] TheGameAudioSeed =
    [
        0xF22D_0E56,
        0x8831_26E9,
        0xC624_DD2F,
        0x0702_C49C,
        0x9E35_3F7D,
        0x6FDF_3B64,
    ];

    private static readonly long[] TheGameLogicSeed =
    [
        0xF22D_0E56,
        0x8831_26E9,
        0xC624_DD2F,
        0x0702_C49C,
        0x9E35_3F7D,
        0x6FDF_3B64,
    ];

    private static long _theGameLogicBaseSeed;

    /// <summary>Initializes the random seed values used in the game engine.</summary>
    /// <remarks>This method configures the random seed values to ensure reproducibility of random sequences. In deterministic mode, all seed values are set to zero, while in non-deterministic mode, they are based on the current Unix time in seconds.</remarks>
    public static void InitializeRandom()
    {
#if DETERMINISTIC
        SeedRandom(0, TheGameClientSeed);
        SeedRandom(0, TheGameAudioSeed);
        SeedRandom(0, TheGameLogicSeed);
        _theGameLogicBaseSeed = 0;
#else
        var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        SeedRandom(seconds, TheGameAudioSeed);
        SeedRandom(seconds, TheGameClientSeed);
        SeedRandom(seconds, TheGameLogicSeed);
        _theGameLogicBaseSeed = seconds;
#endif
    }

    private static void SeedRandom(long seed, Span<long> seeds)
    {
        var ax = seed;
        ax += 0xF22D_0E56;
        seeds[0] = ax;
        ax += unchecked(0x8831_26E9 - 0xF22D_0E56);
        seeds[1] = ax;
        ax += 0xC624_DD2F - 0x8831_26E9;
        seeds[2] = ax;
        ax += unchecked(0x0702_C49C - 0xC624_DD2F);
        seeds[3] = ax;
        ax += 0x9E35_3F7D - 0x0702_C49C;
        seeds[4] = ax;
        ax += unchecked(0x6FDF_3B64 - 0x9E35_3F7D);
        seeds[5] = ax;
    }
}
