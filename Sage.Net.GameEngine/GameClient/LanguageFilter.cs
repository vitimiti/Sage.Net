// -----------------------------------------------------------------------
// <copyright file="LanguageFilter.cs" company="Sage.Net">
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

using System.Text;
using Sage.Net.Extensions;
using Sage.Net.GameEngine.Common;

namespace Sage.Net.GameEngine.GameClient;

/// <summary>Provides simple profanity filtering for chat and UI text.</summary>
/// <remarks>
/// <para>The filter loads an obfuscated bad-words list from <see cref="BadWordFileName"/> during <see cref="Initialize"/>. Words are stored normalized via <see cref="UnHaxor(string)"/> to account for common leetspeak substitutions (for example, <c>ph</c> -&gt; <c>f</c>, <c>3</c> -&gt; <c>e</c>, <c>@</c> -&gt; <c>a</c>), and a few characters are ignored.</para>
/// <para>The <see cref="FilterLine(string)"/> method scans a line, splitting by typical delimiters, and replaces any token matching the normalized bad-words list with asterisks.</para>
/// </remarks>
public class LanguageFilter : SubsystemBase
{
    /// <summary>XOR key used to decode characters stored in the bad-words data file.</summary>
    public const int XorKey = 0x5555;

    /// <summary>File name containing the obfuscated bad-words list.</summary>
    public const string BadWordFileName = "langdata.dat";

    /// <summary>Gets or sets the singleton-like reference to the active language filter instance, when used by the game.</summary>
    public static LanguageFilter? TheLanguageFilter { get; set; }

    /// <summary>Gets the normalized full-word blacklist. Key is the normalized token, value is unused.</summary>
    protected OrderedDictionary<string, bool> WordList { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the normalized substring blacklist.</summary>
    protected OrderedDictionary<string, bool> SubWordList { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes the filter by loading and decoding the bad-words list from <see cref="BadWordFileName"/>.</summary>
    public override void Initialize()
    {
        WordList.Clear();

        using FileStream file1 = File.OpenRead(BadWordFileName);
        using BinaryReader reader = new(file1, Encoding.Unicode, leaveOpen: true);

        while (reader.TryReadWord(out IList<char> rawWord))
        {
            if (rawWord.Count == 0)
            {
                continue;
            }

            Span<char> decodedChars = new char[rawWord.Count];
            for (var i = 0; i < rawWord.Count; i++)
            {
                decodedChars[i] = (char)(rawWord[i] ^ XorKey);
            }

            var decoded = new string(decodedChars);
            var normalized = UnHaxor(decoded);

            WordList[normalized] = true;
        }
    }

    /// <summary>Re-initializes the filter, equivalent to calling <see cref="Initialize"/>.</summary>
    public override void Reset() => Initialize();

    /// <summary>No-op for this subsystem; the filter has no per-frame work.</summary>
    public override void UpdateCore() { }

    /// <summary>Replaces any blacklisted words found in the provided text with asterisks.</summary>
    /// <param name="line">Input text to filter. Must not be <see langword="null"/>.</param>
    /// <returns>The filtered text with offending tokens replaced by '*'.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="line"/> is <see langword="null"/>.</exception>
    public string FilterLine(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ReadOnlySpan<char> delimiters = " ;,.!?:=\\/><`~()&^%#\n\t";
        var buffer = line.ToCharArray();

        var i = 0;
        while (i < buffer.Length)
        {
            while (i < buffer.Length && delimiters.Contains(buffer[i]))
            {
                i++;
            }

            if (i >= buffer.Length)
            {
                break;
            }

            var tokenStart = i;
            while (i < buffer.Length && !delimiters.Contains(buffer[i]))
            {
                i++;
            }

            var tokenLength = i - tokenStart;
            if (tokenLength <= 0)
            {
                continue;
            }

            var token = new string(buffer, tokenStart, tokenLength);
            var normalized = UnHaxor(token);

            if (!WordList.ContainsKey(normalized))
            {
                continue;
            }

            for (var j = tokenStart; j < tokenStart + tokenLength; j++)
            {
                buffer[j] = '*';
            }
        }

        return new string(buffer);
    }

    /// <summary>Normalizes a token by converting common leetspeak/haxor substitutions to letters and removing certain ignorable characters.</summary>
    /// <param name="word">The word to normalize. Must not be <see langword="null"/>.</param>
    /// <returns>The normalized word suitable for dictionary lookups.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="word"/> is <see langword="null"/>.</exception>
    protected static string UnHaxor(string word)
    {
        ArgumentNullException.ThrowIfNull(word);

        char[] ignoredChars = ['0', '_', '*', '\'', '"'];

        var length = word.Length;
        StringBuilder newWord = new();
        for (var i = 0; i < length; i++)
        {
            var c = word[i];
            switch (c)
            {
                case 'p' or 'P' when i + 1 < length && word[i + 1] is 'h' or 'H':
                    _ = newWord.Append('f');
                    i++; // Skip the h
                    break;
                case 'p' or 'P':
                    _ = newWord.Append(c);
                    break;
                case '1':
                    _ = newWord.Append('l');
                    break;
                case '3':
                    _ = newWord.Append('e');
                    break;
                case '4':
                    _ = newWord.Append('a');
                    break;
                case '5':
                    _ = newWord.Append('s');
                    break;
                case '6':
                    _ = newWord.Append('b');
                    break;
                case '7':
                    _ = newWord.Append('t');
                    break;
                case '0':
                    _ = newWord.Append('o');
                    break;
                case '@':
                    _ = newWord.Append('a');
                    break;
                case '$':
                    _ = newWord.Append('s');
                    break;
                case '+':
                    _ = newWord.Append('t');
                    break;
                default:
                {
                    if (!ignoredChars.Contains(c))
                    {
                        _ = newWord.Append(c);
                    }

                    break;
                }
            }
        }

        return newWord.ToString();
    }
}
