using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xabe.VideoConverter
{
    public static partial class Extensions
    {
        public static bool IsTvShow(this FileInfo file)
        {
            return file.Name.IsTvShow();
        }

        public static string GetYear(this FileInfo file)
        {
            return file.Name.GetYear();
        }

        public static string GetNormalizedName(this FileInfo file)
        {
            return file.Name.GetNormalizedName();
        }

        public static string RemoveTvShowInfo(this FileInfo file)
        {
            return file.Name.RemoveTvShowInfo();
        }

        public static int GetSeason(this FileInfo file)
        {
            return file.Name.GetSeason();
        }

        public static string GetNormalizedName(this string name)
        {
            return NameNormalizer.GetNormalizedName(name);
        }

        public static string ChangeExtension(this FileInfo file, string extension = ".mp4")
        {
            return file.Name.ChangeExtension(extension);
        }

        public static string
            GetTvShowInfo(this FileInfo file)
        {
            return file.Name.GetTvShowInfo();
        }

        public static bool IsTvShow(this string name)
        {
            return Regex.IsMatch(name, @"^.*S\d\dE\d\d", RegexOptions.IgnoreCase);
        }

        public static string RemoveTvShowInfo(this string name)
        {
            return name.Replace(name.GetTvShowInfo(), "")
                       .Trim();
        }

        public static int GetSeason(this string name)
        {
            string info = name.GetTvShowInfo();
            info = info.Substring(1, 2);
            return int.Parse(info);
        }

        public static string GetTvShowInfo(this string name)
        {
            string tvInfo = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Last();
            return tvInfo;
        }

        public static string GetYear(this string name)
        {
            string yearWithExtension = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                           .Last();
            string year = yearWithExtension.Split('.')
                                           .First();
            return year;
        }

        public static string ChangeExtension(this string name, string extension = ".mp4")
        {
            return Path.GetFileNameWithoutExtension(name) + extension;
        }
    }
}
