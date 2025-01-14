﻿using System;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Marten.Services.Json;

// OD: Should not be needed after .NET 6.0 (based on https://github.com/dotnet/runtime/issues/782#issuecomment-673029718)
// Taken from: https://github.com/dotnet/corefx/pull/41354/files

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
internal sealed class JsonSnakeCaseNamingPolicy: JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        // Allocates a string builder with the guessed result length,
        // where 5 is the average word length in English, and
        // max(2, length / 5) is the number of underscores.
        var builder = new StringBuilder(name.Length + Math.Max(2, name.Length / 5));
        UnicodeCategory? previousCategory = null;

        for (var currentIndex = 0; currentIndex < name.Length; currentIndex++)
        {
            var currentChar = name[currentIndex];
            if (currentChar == '_')
            {
                builder.Append('_');
                previousCategory = null;
                continue;
            }

            var currentCategory = char.GetUnicodeCategory(currentChar);

            switch (currentCategory)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == UnicodeCategory.SpaceSeparator ||
                        previousCategory == UnicodeCategory.LowercaseLetter ||
                        (previousCategory != UnicodeCategory.DecimalDigitNumber &&
                         currentIndex > 0 &&
                         currentIndex + 1 < name.Length &&
                         char.IsLower(name[currentIndex + 1])))
                    {
                        builder.Append('_');
                    }

                    currentChar = char.ToLower(currentChar);
                    break;

                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == UnicodeCategory.SpaceSeparator)
                    {
                        builder.Append('_');
                    }

                    break;

                case UnicodeCategory.Surrogate:
                    break;

                default:
                    if (previousCategory != null)
                    {
                        previousCategory = UnicodeCategory.SpaceSeparator;
                    }

                    continue;
            }

            builder.Append(currentChar);
            previousCategory = currentCategory;
        }

        return builder.ToString();
    }
}
