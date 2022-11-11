using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace cmod
{
    public static class ModCapabilities
    {
        public static List<Assembly> loadedMods = new List<Assembly>();
        public static Dictionary<string, List<Action<object[]>>> loadedActions = new Dictionary<string, List<Action<object[]>>>();
        private static IEnumerable<Type> getLoadables(Assembly assembly)
        {
            //if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
        public static void RegisterAllEvents()
        {
            string g = "";
            try
            {
                foreach (var e in System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory().Replace('\\','/') + "/mods/"))
                {
                    if (e.Contains(".dll"))
                    {
                        g += e;
                        loadedMods.Add(Assembly.LoadFile(e));
                        
                    }
                }
                if (true)
                {
                    foreach (var asm in loadedMods)
                    {
                        g += asm.GetName().Name + "\n";
                        foreach (var t in asm.GetTypes().Where(t=> t.Namespace == "KoboldMod"))
                        {
                            if(t == null) { continue; }
                            g += t.Name + "\n";
                            foreach (var meth in t.GetMethods())
                            {
                                if (meth == null) { continue; }
                                g += meth.Name + "\n";
                                if (!meth.IsStatic) { continue; }
                                ModEventAttribute mea =
                                            (ModEventAttribute)Attribute.GetCustomAttribute(meth, typeof(ModEventAttribute));
                                if (mea != null)
                                {
                                    System.IO.File.WriteAllText(meth.Name, "uwu");
                                    if (!loadedActions.ContainsKey(mea.Name))
                                    {
                                        loadedActions.Add(mea.Name, new List<Action<object[]>>());
                                    }
                                    loadedActions[mea.Name].Add((v) => { meth.Invoke(null, v); });
                                }
                                
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {
                
            }
            
        }
        public static void RunEvent(string ev, object[] vars = null)
        {
            if (loadedActions.ContainsKey(ev))
            {
                foreach(var m in loadedActions[ev])
                {
                    var eg = new object[0];
                    if(vars != null) { eg = vars; }
                    m.Invoke(eg);
                }
            }
        }
    }
    public class ModEventAttribute : Attribute
    {
        public string Name;
        public string FromMod;
        public ModEventAttribute(string name, string mod)
        {
            Name = name; FromMod = mod;
        }
    }
}
