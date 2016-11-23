using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHarveyUtil.Terminal
{
    public class MenuState
    {
        private Dictionary<string, object> _state;

        public void Store(String name, object value)
        {
            _state[name] = value;
        }
        public bool Load<T>(String name, out T variable) where T : class
        {
            if (!_state.ContainsKey(name))
            {
                Console.WriteLine(name + " must be loaded before using");
                variable = null;
                return false;
            }

            variable = _state[name] as T;

            if (variable == null)
            {
                Console.WriteLine(name + " is not of type " + typeof(T));
                return false;
            }

            return true;
        }
    }
}
