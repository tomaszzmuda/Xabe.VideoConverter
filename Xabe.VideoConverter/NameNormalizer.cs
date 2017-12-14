﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Xabe.VideoConverter
{
    public abstract class NameNormalizer
    {
        private static readonly string[] _escapeChars = {"[", "]", "{", @"}"};

        public static string GetNormalizedName(string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            name = name.Replace(' ', '.');
            string[] words = name.Split('.', StringSplitOptions.RemoveEmptyEntries);

            var outputName = "";
            foreach(string word in words)
            {
                if(IsYear(word))
                {
                    outputName += $"({word})";
                    break;
                }
                if(HasEscapeChars(word))
                    continue;
                outputName += $"{word.FirstCharToUpper()} ";
                if(!string.IsNullOrEmpty(word) &&
                   (IsYear(word) || word.IsTvShow()))
                    break;
            }
            return outputName.Trim();
        }

        private static bool HasEscapeChars(string word)
        {
            foreach(string character in _escapeChars)
                if(word.Contains(character))
                    return true;
            return false;
        }

        private static bool IsYear(string word)
        {
            return Regex.IsMatch(word, "^\\d{4}$");
        }
    }
}
