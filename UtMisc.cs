using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Lingo;

namespace TankIconMaker
{
    static partial class Ut
    {
        public static readonly string[] RomanNumerals = new[] { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

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

        /// <summary>
        /// Enumerates the rows of a CSV file. Each row is represented by a tuple containing the line number and
        /// an array of fields. Does not handle all valid CSV files: for example, multi-line field values are not supported.
        /// </summary>
        public static IEnumerable<Tuple<int, string[]>> ReadCsvLines(string filename)
        {
            int num = 0;
            foreach (var line in File.ReadLines(filename))
            {
                num++;
                var fields = parseCsvLine(line);
                if (fields == null)
                    throw new Exception(App.Translation.Error.DataFile_CsvParse.Fmt(num));
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

        /// <summary>Returns one of the specified values based on which country this value represents.</summary>
        public static T Pick<T>(this Country country, T ussr, T germany, T usa, T france, T china, T uk)
        {
            switch (country)
            {
                case Country.USSR: return ussr;
                case Country.Germany: return germany;
                case Country.USA: return usa;
                case Country.France: return france;
                case Country.China: return china;
                case Country.UK: return uk;
                default: throw new Exception();
            }
        }

        /// <summary>Returns one of the specified values based on which tank class this value represents.</summary>
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

        /// <summary>Returns one of the specified values based on which tank category this value represents.</summary>
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

        /// <summary>Attempts to locate the World of Tanks installation directory. Returns the root of the C: drive in case of failure.</summary>
        public static string FindTanksDirectory()
        {
            string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{1EAC1D02-C6AC-4FA6-9A44-96258C37C812}_is1", "InstallLocation", null) as string;
            if (path == null)
                path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{1EAC1D02-C6AC-4FA6-9A44-96258C37C812}_is1", "InstallLocation", null) as string;
            if (path == null || !Directory.Exists(path))
                return "C:\\"; // could do a more thorough search through the Uninstall keys - not sure if the GUID is fixed or not.
            else
                return path.TrimEnd('\\');
        }

        /// <summary>
        /// Returns the first item whose <paramref name="maxOf"/> selector is maximal in this collection, or null if the collection is empty.
        /// </summary>
        public static TItem MaxOrDefault<TItem, TSelector>(this IEnumerable<TItem> collection, Func<TItem, TSelector> maxOf)
        {
            return collection.MaxAll(maxOf).FirstOrDefault();
        }

        /// <summary>
        /// Enumerates all the items whose <paramref name="maxOf"/> selector is maximal in this collection.
        /// </summary>
        public static IEnumerable<TItem> MaxAll<TItem, TSelector>(this IEnumerable<TItem> collection, Func<TItem, TSelector> maxOf)
        {
            var comparer = Comparer<TSelector>.Default;
            var largest = default(TSelector);
            var result = new List<TItem>();
            bool any = false;
            foreach (var item in collection)
            {
                var current = maxOf(item);
                var compare = comparer.Compare(current, largest);
                if (!any || compare > 0)
                {
                    any = true;
                    largest = current;
                    result.Clear();
                    result.Add(item);
                }
                else if (compare == 0)
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Reduces the size of a stack trace by removing all lines which are outside this program's namespace and
        /// leaving only relative source file names.
        /// </summary>
        public static string CollapseStackTrace(string stackTrace)
        {
            var lines = stackTrace.Split('\n');
            var result = new StringBuilder();
            bool needEllipsis = true;
            string fileroot = null;
            try { fileroot = Path.GetDirectoryName(new StackFrame(true).GetFileName()) + @"\"; }
            catch { }
            foreach (var line in lines)
            {
                if (line.Contains(typeof(Ut).Namespace))
                {
                    result.AppendLine("  " + (fileroot == null ? line : line.Replace(fileroot, "")).Trim());
                    needEllipsis = true;
                }
                else if (needEllipsis)
                {
                    result.AppendLine("  ...");
                    needEllipsis = false;
                }
            }
            return result.ToString();
        }

        /// <summary>Provides a function delegate that accepts only value types as return types.</summary>
        /// <remarks>This type was introduced to make <see cref="ObjectExtensions.NullOr{TInput,TResult}(TInput,FuncStruct{TInput,TResult})"/>
        /// work without clashing with <see cref="ObjectExtensions.NullOr{TInput,TResult}(TInput,FuncClass{TInput,TResult})"/>.</remarks>
        public delegate TResult FuncStruct<in TInput, TResult>(TInput input) where TResult : struct;
        /// <summary>Provides a function delegate that accepts only reference types as return types.</summary>
        /// <remarks>This type was introduced to make <see cref="ObjectExtensions.NullOr{TInput,TResult}(TInput,FuncClass{TInput,TResult})"/>
        /// work without clashing with <see cref="ObjectExtensions.NullOr{TInput,TResult}(TInput,FuncStruct{TInput,TResult})"/>.</remarks>
        public delegate TResult FuncClass<in TInput, TResult>(TInput input) where TResult : class;

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult NullOr<TInput, TResult>(this TInput input, FuncClass<TInput, TResult> lambda) where TResult : class
        {
            return input == null ? null : lambda(input);
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult? NullOr<TInput, TResult>(this TInput input, Func<TInput, TResult?> lambda) where TResult : struct
        {
            return input == null ? null : lambda(input);
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult? NullOr<TInput, TResult>(this TInput input, FuncStruct<TInput, TResult> lambda) where TResult : struct
        {
            return input == null ? null : (TResult?) lambda(input);
        }

        public static int ModPositive(int value, int modulus)
        {
            int result = value % modulus;
            return result >= 0 ? result : (result + modulus);
        }

        public static string MakeRelativePath(string path)
        {
            try
            {
                if (PathUtil.IsSubpathOfOrSame(path, PathUtil.AppPath))
                    return PathUtil.ToggleRelative(PathUtil.AppPath, path);
            }
            catch { }
            if (App.LastGameInstallSettings == null)
                return path;
            try
            {
                if (PathUtil.IsSubpathOfOrSame(path, Path.Combine(App.LastGameInstallSettings.Path, App.LastGameInstallSettings.GameVersion.PathMods)))
                    return PathUtil.ToggleRelative(Path.Combine(App.LastGameInstallSettings.Path, App.LastGameInstallSettings.GameVersion.PathMods), path);
            }
            catch { }
            try
            {
                if (PathUtil.IsSubpathOfOrSame(path, App.LastGameInstallSettings.Path))
                    return PathUtil.ToggleRelative(App.LastGameInstallSettings.Path, path);
            }
            catch { }
            return path;
        }

        /// <summary>
        /// Determines whether the specified file contains the specified text. The file doesn’t have to exist.
        /// </summary>
        public static bool FileContains(string fileName, string content)
        {
            if (content == null)
                return false;
            try
            {
                foreach (var line in File.ReadLines(fileName))
                    if (line.Contains(content))
                        return true;
                return false;
            }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
        }

        /// <summary>
        /// Generates a representation of the specified byte array as hexadecimal numbers (“hexdump”).
        /// </summary>
        public static string ToHex(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            char[] charArr = new char[data.Length * 2];
            var j = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = (byte) (data[i] >> 4);
                charArr[j] = (char) (b < 10 ? '0' + b : 'W' + b);   // 'a'-10 = 'W'
                j++;
                b = (byte) (data[i] & 0xf);
                charArr[j] = (char) (b < 10 ? '0' + b : 'W' + b);
                j++;
            }
            return new string(charArr);
        }

        /// <summary>Copies <paramref name="len"/> bytes from one location to another. Works fastest if <paramref name="len"/> is divisible by 16.</summary>
        public static unsafe void MemSet(byte* dest, byte value, int len)
        {
            ushort ushort_ = (ushort) (value | (value << 8));
            uint uint_ = (uint) (ushort_ | (ushort_ << 16));
            ulong ulong_ = uint_ | ((ulong) uint_ << 32);
            if (len >= 16)
            {
                do
                {
                    *(ulong*) dest = ulong_;
                    *(ulong*) (dest + 8) = ulong_;
                    dest += 16;
                }
                while ((len -= 16) >= 16);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *(ulong*) dest = ulong_;
                    dest += 8;
                }
                if ((len & 4) != 0)
                {
                    *(uint*) dest = uint_;
                    dest += 4;
                }
                if ((len & 2) != 0)
                {
                    *(ushort*) dest = ushort_;
                    dest += 2;
                }
                if ((len & 1) != 0)
                    *dest = value;
            }
        }

        /// <summary>Copies <paramref name="len"/> bytes from one location to another. Works fastest if <paramref name="len"/> is divisible by 16.</summary>
        public static unsafe void MemCpy(byte* dest, byte* src, int len)
        {
            if (len >= 16)
            {
                do
                {
                    *(long*) dest = *(long*) src;
                    *(long*) (dest + 8) = *(long*) (src + 8);
                    dest += 16;
                    src += 16;
                }
                while ((len -= 16) >= 16);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *(long*) dest = *(long*) src;
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *(int*) dest = *(int*) src;
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *(short*) dest = *(short*) src;
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                    *dest = *src;
            }
        }

        /// <summary>Copies <paramref name="len"/> bytes from one location to another. Works fastest if <paramref name="len"/> is divisible by 16.</summary>
        public static unsafe void MemCpy(byte[] dest, byte* src, int len)
        {
            if (len > dest.Length)
                throw new ArgumentOutOfRangeException("len");
            fixed (byte* destPtr = dest)
                MemCpy(destPtr, src, len);
        }

        /// <summary>Copies <paramref name="len"/> bytes from one location to another. Works fastest if <paramref name="len"/> is divisible by 16.</summary>
        public static unsafe void MemCpy(byte* dest, byte[] src, int len)
        {
            if (len > src.Length)
                throw new ArgumentOutOfRangeException("len");
            fixed (byte* srcPtr = src)
                MemCpy(dest, srcPtr, len);
        }

        public static Language GetOsLanguage()
        {
            try
            {
                var curUiLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

                // Special case for the languages we supply, so that if there are several with the same 2-letter code
                // we can pick the right one.
                switch (curUiLanguage)
                {
                    case "en": return Language.EnglishUK;
                    case "ru": return Language.Russian;
                    case "de": return Language.German;
                }

                var t = typeof(Language);
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var a = f.GetCustomAttributes<LanguageInfoAttribute>().FirstOrDefault();
                    if (a != null && a.LanguageCode == curUiLanguage)
                        return (Language) f.GetValue(null);
                }
            }
            catch { }
            return Language.EnglishUK;
        }
    }

    /// <summary>
    /// Enables scheduling tasks to execute in a thread of a different priority than Normal. Only non-critical tasks
    /// should be scheduled using this scheduler because any remaining queued tasks will be dropped on program exit.
    /// </summary>
    public class PriorityScheduler : TaskScheduler
    {
        // http://stackoverflow.com/a/9056702/33080
        public static PriorityScheduler AboveNormal = new PriorityScheduler(ThreadPriority.AboveNormal);
        public static PriorityScheduler BelowNormal = new PriorityScheduler(ThreadPriority.BelowNormal);
        public static PriorityScheduler Lowest = new PriorityScheduler(ThreadPriority.Lowest);

        private BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
        private Thread[] _threads;
        private ThreadPriority _priority;
        private readonly int _maximumConcurrencyLevel = Math.Max(1, Environment.ProcessorCount);

        public PriorityScheduler(ThreadPriority priority)
        {
            _priority = priority;
        }

        public override int MaximumConcurrencyLevel
        {
            get { return _maximumConcurrencyLevel; }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);

            if (_threads == null)
            {
                _threads = new Thread[_maximumConcurrencyLevel];
                for (int i = 0; i < _threads.Length; i++)
                {
                    int local = i;
                    _threads[i] = new Thread(() =>
                    {
                        foreach (Task t in _tasks.GetConsumingEnumerable())
                            base.TryExecuteTask(t);
                    });
                    _threads[i].Name = string.Format("PriorityScheduler: {0}", i);
                    _threads[i].Priority = _priority;
                    _threads[i].IsBackground = true;
                    _threads[i].Start();
                }
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false; // we might not want to execute task that should schedule as high or low priority inline
        }
    }

    sealed class TypeInfo<T>
    {
        public Type Type;
        public Func<T> Constructor;
        public string Name { get; set; }
        public string Description { get; set; }
    }

    interface IHasTypeNameDescription
    {
        string TypeName { get; }
        string TypeDescription { get; }
    }

    /// <summary>
    /// Encapsulates a file name which may refer to a file inside a container file, or just to a file directly. The parts
    /// are separated with a ":". Helps avoid issues with Path.Combine complaining about invalid filenames.
    /// </summary>
    struct CompositeFilename
    {
        /// <summary>Container file path, or null if none.</summary>
        public string Container { get; private set; }
        /// <summary>Referenced file path.</summary>
        public string File { get; private set; }

        public override string ToString() { return Container + ":" + File; }

        public CompositeFilename(params string[] path)
            : this()
        {
            var builder = new StringBuilder(256);
            Container = null;
            foreach (var part in path)
            {
                var parts = Regex.Split(part, @"(?<=..):");
                if (parts.Length > 2)
                    throw new ArgumentException("Multiple \":\" separators are not allowed.");
                else if (parts.Length == 2)
                {
                    if (Container != null)
                        throw new ArgumentException("Multiple \":\" separators are not allowed.");
                    if (builder.Length > 0)
                        builder.Append('\\');
                    builder.Append(parts[0]);
                    Container = builder.ToString();
                    builder.Clear();
                    builder.Append(parts[1]);
                }
                else
                {
                    if (builder.Length > 0)
                        builder.Append('\\');
                    builder.Append(part);
                }
            }
            File = builder.ToString();
        }
    }

    [TypeConverter(typeof(BoolWithPassthroughTranslation.Conv))]
    enum BoolWithPassthrough
    {
        No,
        Yes,
        Passthrough,
    }
}
