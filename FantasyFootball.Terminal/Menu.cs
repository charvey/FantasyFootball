using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal
{
    public class Menu
    {
        #region Fields

        protected string _text;
        protected Action<MenuState> Operation;
        protected List<Menu> SubMenus;

        #endregion

        public void SpawnThread(Action a)
        {
            Thread thread = new Thread(new ThreadStart(a));
            thread.Start();
        }

        #region Constructors

        public Menu()
            : this(string.Empty)
        {
        }

        public Menu(string text)
        {
            this._text = text;
        }

        public Menu(string text, Action<MenuState> operation)
            : this(text)
        {
            this.Operation = operation;
        }

        public Menu(string text, List<Menu> subMenus)
            : this(text)
        {
            this.SubMenus = subMenus;
        }

        #endregion

        #region Defaults

        public virtual string GetText()
        {
            return _text;
        }

        public virtual List<Menu> GetSubMenus()
        {
            return SubMenus;
        }

        public virtual void Display(MenuState state)
        {
            Console.Clear();
            if (Operation != null)
            {
                Operation(state);
            }
            else
            {
                Menu choice;
                int input;

                while (true)
                {
                    var subMenus = GetSubMenus();
                    input = Options(GetText(), subMenus.Select(m => m.GetText()).Concat(new[] { "Exit" }).ToArray());

                    if (input == subMenus.Count + 1)
                    {
                        return;
                    }
                    else if (1 <= input && input <= subMenus.Count)
                    {
                        choice = subMenus[input - 1];
                    }
                    else
                    {
                        choice = null;
                    }

                    //Do Choice
                    Console.WriteLine();
                    if (choice != null)
                    {
                        choice.Display(state);
                    }
                }
            }
        }

        #endregion

        #region UI

        public bool Confirm(string prompt)
        {
            Console.WriteLine(prompt);
            do
            {
                string response = Console.ReadKey().KeyChar.ToString().ToUpper();
                Console.WriteLine();
                if (response == "Y")
                {
                    return true;
                }
                else if (response == "N")
                {
                    return false;
                }
                Console.WriteLine("Please enter Y/N");
            } while (true);
        }

        public static int Options(string p, params string[] o)
        {
            Console.WriteLine(p);
            for (int i = 0; i < o.Length; i++)
            {
                Console.WriteLine((i + 1) + ". " + o[i]);
            }
            do
            {
                int option;
                string response;
                if (o.Length > 9)
                {
                    response = Console.ReadLine();
                }
                else
                {
                    response = Console.ReadKey(true).KeyChar.ToString();
                    Console.WriteLine();
                }
                if (int.TryParse(response, out option) && (0 < option && option <= o.Length))
                {
                    return option;
                }
                Console.WriteLine("Please enter 1-" + o.Length);
            } while (true);
        }

        public static string Prompt(string p)
        {
            return PromptFor<string>(p);
        }

        public static T PromptFor<T>(string prompt) where T : IConvertible
        {
            while (true)
            {
                Console.WriteLine(prompt);

                try
                {
                    return (T)Convert.ChangeType(Console.ReadLine(), typeof(T));
                }
                catch (Exception)
                {
                    Console.WriteLine("Input must be of type: " + typeof(T).Name);
                }
            }
        }

        #endregion
    }
}