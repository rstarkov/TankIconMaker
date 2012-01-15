using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Microsoft.Win32;

namespace TankIconMaker
{
    static partial class Ut
    {
        /// <summary>Shorthand for string.Format, with a more natural ordering (since formatting is typically an afterthought).</summary>
        public static string Fmt(this string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }

        /// <summary>Shorthand for comparing strings ignoring case. Suitable for things like filenames, but not address books.</summary>
        public static bool EqualsNoCase(this string string1, string string2)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(string1, string2);
        }

        public static IEnumerable<Tuple<int, string[]>> ReadCsvLines(string filename)
        {
            int num = 0;
            foreach (var line in File.ReadLines(filename))
            {
                num++;
                var fields = parseCsvLine(line);
                if (fields == null)
                    throw new Exception(string.Format("Couldn't parse line {0}.", num));
                yield return Tuple.Create(num, fields);
            }
        }

        private static string[] parseCsvLine(string line)
        {
            var fields = Regex.Matches(line, @"(^|(?<=,)) *(?<quote>""?)(("""")?[^""]*?)*?\k<quote> *($|(?=,))").Cast<Match>().Select(m => m.Value).ToArray();
            if (line != string.Join(",", fields))
                return null;
            return fields.Select(f => f.Contains('"') ? Regex.Replace(f, @"^ *""(.*)"" *$", "$1").Replace(@"""""", @"""") : f).ToArray();
        }

        public static T Pick<T>(this Country country, T ussr, T germany, T usa, T france, T china)
        {
            switch (country)
            {
                case Country.USSR: return ussr;
                case Country.Germany: return germany;
                case Country.USA: return usa;
                case Country.France: return france;
                case Country.China: return china;
                default: throw new Exception();
            }
        }

        public static T Pick<T>(this Class class_, T light, T medium, T heavy, T destroyer, T artillery)
        {
            switch (class_)
            {
                case Class.Light: return light;
                case Class.Medium: return medium;
                case Class.Heavy: return heavy;
                case Class.Destroyer: return destroyer;
                case Class.Artillery: return artillery;
                default: throw new Exception();
            }
        }

        public static T Pick<T>(this Category class_, T normal, T premium, T special)
        {
            switch (class_)
            {
                case Category.Normal: return normal;
                case Category.Premium: return premium;
                case Category.Special: return special;
                default: throw new Exception();
            }
        }

        public static string FindTanksDirectory()
        {
            string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{1EAC1D02-C6AC-4FA6-9A44-96258C37C812}_is1", "InstallLocation", null) as string;
            if (path == null)
                path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{1EAC1D02-C6AC-4FA6-9A44-96258C37C812}_is1", "InstallLocation", null) as string;
            if (path == null || !Directory.Exists(path))
                return "C:\\"; // could do a more thorough search through the Uninstall keys - not sure if the GUID is fixed or not.
            else
                return path;
        }
    }

    /// <summary>A crutch that enables a sensible way to bind to a dependency property with a custom conversion.</summary>
    class LambdaConverter<TSource, TResult> : IValueConverter
    {
        private Func<TSource, TResult> _lambda;

        public LambdaConverter(Func<TSource, TResult> lambda)
        {
            _lambda = lambda;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _lambda((TSource) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>A crutch that enables a sensible way to bind to a dependency property with a custom conversion.</summary>
    static class LambdaConverter
    {
        /// <summary>Creates a new converter using the specified lambda to perform the conversion.</summary>
        public static LambdaConverter<TSource, TResult> New<TSource, TResult>(Func<TSource, TResult> lambda)
        {
            return new LambdaConverter<TSource, TResult>(lambda);
        }
    }

}
