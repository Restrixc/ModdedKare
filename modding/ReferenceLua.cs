using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace modding
{
    public static class ReferenceLua
    {
        public static bool HasLoadedMods = false;
        public static Dictionary<string, List<Action<HookVariables>>> Hook = new Dictionary<string, List<Action<HookVariables>>>();
        public static void CallFunction(string l, HookVariables hv = null)
        {
            if (Hook.ContainsKey(l))
            {
                foreach(var hook in Hook[l])
                {
                    hook.Invoke(hv);
                }
            }
        }
        public static void AddHook(string l, Action<HookVariables> v)
        {
            if (Hook.ContainsKey(l))
            {
                Hook[l].Add(v);
            }
            else
            {
                Hook.Add(l, new List<Action<HookVariables>>());
                Hook[l].Add(v);
            }
        }
    }

    public class HookVariables
    {

    }

    public class KoboldHookReference : HookVariables
    {
        public Kobold kobold;
        public KoboldGenes genes;
        public UnityEngine.Transform transform;
    }
    
}
