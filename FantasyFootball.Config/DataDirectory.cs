
namespace FantasyFootball.Config
{
    public static class DataDirectory
    {
        public static string Path
        {
            get
            {
                var root = @"C:\FF_Data";
#if DEBUG
                var folder="debug";
#else
                var folder = "release";
#endif
                return System.IO.Path.Combine(root, folder);
            }
        }

        public static string FilePath(string filename)
        {
            return System.IO.Path.Combine(Path, filename);
        }
    }
}
