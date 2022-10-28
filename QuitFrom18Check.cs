using modding;
using UnityEngine;

public class QuitFrom18Check : MonoBehaviour
{
	public void Awake()
	{
		System.IO.File.WriteAllText("uwu.txt", "FUCKING DLL SHIT");

        //var x = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace('\\', '/');
        //System.Reflection.Assembly.LoadFile( x + "/MoonSharp.Interpreter.dll");
        //print(x);
        //Now introduce all the lua shit

        //Load all the mods and shit from here!
        if (!modding.ReferenceLua.HasLoadedMods)
        {
            modding.ReferenceLua.HasLoadedMods = true;
            modding.ReferenceLua.AddHook("OnCharacterSpawn", (hv) =>
            {
                KoboldHookReference khr = (KoboldHookReference)hv;
                
            });
        }

	}
	public void Quit()
	{
		GameManager.instance.Quit();
	}
}
