using System;
using System.Collections.Generic;

namespace FantasyFootball.Terminal
{
    public class MenuState
    {
        private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

        public void Store(String name, object value)
        {
            _state[name] = value;
        }

        public T Load<T>(String name) where T : class
        {
            T output;
            Load(name, out output);
            return output;
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
