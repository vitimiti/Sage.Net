// -----------------------------------------------------------------------
// <copyright file="IniGetters.cs" company="Sage.Net">
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

namespace Sage.Net.Generals.GameEngine.Common;

public partial class Ini
{
    /// <summary>Retrieves the next non-separator token from the current INI data stream.</summary>
    /// <param name="separators">An optional array of characters used to determine where tokens are delimited. Defaults to standard separators if null.</param>
    /// <returns>The next token as a string.</returns>
    public string GetNextToken(char[]? separators = null)
    {
        var seps = separators ?? [.. Separators];
        while (true)
        {
            PrepareNextLine();
            SkipSeparators(seps);
            if (TryExtractToken(seps, out var token))
            {
                return token;
            }

            // No token on this line, move to next line
            _currentTokenLine = null;
            _currentTokenIndex = 0;
        }
    }

    /// <summary>Retrieves the next non-separator token from the current INI data stream, or null if no valid token is found.</summary>
    /// <param name="separators">An optional array of characters used to determine where tokens are delimited. Defaults to standard separators if null.</param>
    /// <returns>The next token as a string, or null if an invalid data condition is encountered.</returns>
    public string? GetNextTokenOrNull(char[]? separators = null)
    {
        try
        {
            return GetNextToken(separators);
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    /// <summary>Retrieves the next ASCII string from the current INI data stream.</summary>
    /// <returns>A string representing the next ASCII sequence, or an empty string if no valid token is found.</returns>
    public string GetNextAsciiString()
    {
        var token = GetNextTokenOrNull();
        if (string.IsNullOrEmpty(token))
        {
            return string.Empty;
        }

        if (token[0] != '"')
        {
            return token;
        }

        StringBuilder sb = new(MaxCharsPerLine);
        if (token.Length > 1)
        {
            AppendWithCap(sb, token.AsSpan(1), MaxCharsPerLine);
        }

        token = GetNextToken([.. SeparatorsQuote]);
        if (token.Length > 1 && token[1] != '\t')
        {
            AppendWithCap(sb, " ".AsSpan(), MaxCharsPerLine);
        }

        AppendWithCap(sb, token.AsSpan(), MaxCharsPerLine);

        return sb.ToString();
    }

    /// <summary>Retrieves the next quoted ASCII string from the current INI data stream.</summary>
    /// <returns>A string representing the next quoted ASCII sequence, or an empty string if no valid token is found.</returns>
    public string GetNextQuotedAsciiString()
    {
        var token = GetNextTokenOrNull();
        if (string.IsNullOrEmpty(token))
        {
            return string.Empty;
        }

        if (token[0] != '"')
        {
            return token;
        }

        StringBuilder sb = new(MaxCharsPerLine);
        if (token.Length > 1)
        {
            AppendWithCap(sb, token.AsSpan(1), MaxCharsPerLine);

            if (EndsWithQuote(token))
            {
                RemoveTrailingQuoteIfPresent(sb);
                return sb.ToString();
            }
        }

        token = GetNextToken([.. SeparatorsQuote]);
        if (ShouldAppendWithSpace(token))
        {
            AppendWithCap(sb, " ".AsSpan(), MaxCharsPerLine);
            AppendWithCap(sb, token.AsSpan(), MaxCharsPerLine);
        }
        else
        {
            RemoveTrailingQuoteIfPresent(sb);
        }

        return sb.ToString();
    }

    private static void AppendWithCap(StringBuilder sb, ReadOnlySpan<char> text, int max)
    {
        var remaining = max - sb.Length;
        if (remaining <= 0)
        {
            return;
        }

        if (text.Length > remaining)
        {
            text = text[..remaining];
        }

        _ = sb.Append(text);
    }

    private static bool EndsWithQuote(string token) => token.Length > 1 && token[^1] == '"';

    private static bool ShouldAppendWithSpace(string token) => token.Length > 1 && token[1] != '\t';

    private static void RemoveTrailingQuoteIfPresent(StringBuilder sb)
    {
        if (sb.Length > 0 && sb[^1] == '"')
        {
            sb.Length--;
        }
    }
}
