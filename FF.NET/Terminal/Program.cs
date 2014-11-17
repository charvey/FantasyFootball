using CHarveyUtil.Terminal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terminal.Menus;

namespace Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            run();
            MainMenu mainmenu = new MainMenu();
            mainmenu.Display();
        }

        static void run()
        {
            using (var file = File.OpenWrite("info.csv")) {
                using(var stream=new StreamWriter(file)) {
                    foreach (string[] line in File.ReadAllLines("w11.csv").Select(l => l.Split(',')))
                    {
                        stream.WriteLine(string.Join(",", newFields(line)));
                    }
                }
            }
        }

        static IEnumerable<string> newFields(string[] line)
        {
            yield return line[0];
            yield return line[1];
            yield return line[2];
            yield return line[3];

            var importantWeeks = line.Skip(4).Skip(10).Take(6);
            foreach(var week in importantWeeks)
            {
                yield return week;
            }
        }
    }
}
