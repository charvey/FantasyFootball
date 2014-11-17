using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Terminal
{
    public static class StaleDetector
    {
        public static bool IsStale(string filepath)
        {
            return IsStale(filepath, TimeSpan.Zero);
        }

        public static bool IsStale(string filepath, bool force)
        {
            return true;
        }

        public static bool IsStale(string filepath, TimeSpan time)
        {
            if (!File.Exists(filepath)) return true;

            FileInfo fileInfo = new FileInfo(filepath);
            DateTime expires = fileInfo.LastWriteTime.Add(time);
            //time = TimeSpan.FromMinutes(fileInfo.Length % time.TotalMinutes);
            //expires = expires.Add(time);
            return expires < DateTime.Now;
        }
    }
}
