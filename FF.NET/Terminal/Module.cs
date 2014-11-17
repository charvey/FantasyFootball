using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Terminal
{
    public abstract class Module
    {
        protected Dictionary<string, Module> DependencyModules = new Dictionary<string, Module>();

        protected Module()
        {
            foreach (string dependency in Dependencies)
            {
                DependencyModules[dependency] = Get(dependency);
            }
        }

        public static T Get<T>() where T : Module
        {
            Type type = typeof(T);
            return Get(type.Name) as T;
        }
        public static Module Get(string name)
        {
            Module module;
            if (!moduleInstances.TryGetValue(name, out module))
            {
                Type moduleType;
                if (!moduleTypes.TryGetValue(name, out moduleType))
                {
                    throw new ArgumentException(name + " module does not exist");
                }
                ConstructorInfo constructor = moduleType.GetConstructor(Type.EmptyTypes);
                module = (Module)constructor.Invoke(new object[] { });
                Console.WriteLine("Initializing " + moduleType.Name);
                module.Initialize();
                Console.WriteLine(moduleType.Name + " Initialized");
                moduleInstances[name] = module;
            }
            return module;
        }

        protected abstract List<string> Dependencies { get; }
        protected abstract void Initialize();

        private static Dictionary<string, Module> moduleInstances = new Dictionary<string, Module>();
        private static Dictionary<string, Type> moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsSubclassOf(typeof(Module)) && type.IsClass)
            .ToDictionary(type => type.Name);
    }
}
