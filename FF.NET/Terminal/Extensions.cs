using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal
{
    public static class Extensions
    {
        public static void AddOrSet(this Dictionary<string, int?> dict, string key, int value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = 0;
            }
            dict[key] = (dict[key] ?? 0) + value;
        }

        public static void AddOrSet(this Dictionary<string, string> dict, string key, int value)
        {
            int old;
            if (!dict.ContainsKey(key))
            {
                old = 0;
            }
            else
            {
                old = int.Parse(dict[key]);
            }
            dict[key] = (old + value).ToString();
        }

        public static TType GetOrSet<TType>(this Dictionary<string, TType> dict, string key, Func<TType> func)
            where TType : new()
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = func();
            }
            return dict[key];
        }

        public static Nullable<TType> ToOrNull<TType>(this string s)
            where TType : struct, IConvertible
        {
            TType? value = null;
            try
            {
                value = (TType)Convert.ChangeType(s, typeof(TType));
            }
            catch (Exception) { }

            return value;
        }

        public static TType ToOrDefault<TType>(this string s)
            where TType : struct, IConvertible
        {
            TType value;
            try
            {
                value = (TType)Convert.ChangeType(s, typeof(TType));
            }
            catch (Exception)
            {
                value = default(TType);
            }

            return value;
        }

        public static TType To<TType>(this string s)
            where TType : struct, IConvertible
        {
            return (TType)Convert.ChangeType(s, typeof(TType));
        }
    }
}
