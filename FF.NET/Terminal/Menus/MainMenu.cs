using CHarveyUtil.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terminal.Modules;

namespace Terminal.Menus
{
    public class MainMenu : Menu
    {
        public MainMenu()
            : base("Main Menu", new List<Menu>
            {
                new Menu("DO ALL THE THINGS!!!",s=>Module.Get<YFFPlayerStats>()),
                new Menu("Load Modules",
                    Assembly.GetExecutingAssembly().GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Module)) && type.IsClass)
                        .Select(type=>new Menu(type.Name,(Action<MenuState>)(s=>Module.Get(type.Name))))
                        .ToList()
                    )
            })
        {

        }

        public void Display(){
            this.Display(new MenuState());
        }
    }
}
