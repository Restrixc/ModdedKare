using modding;
using UnityEngine;

public class QuitFrom18Check : MonoBehaviour
{
	public static bool HasLoadedMods = false;
	public void Awake()
	{
        if (!HasLoadedMods)
        {
			cmod.ModCapabilities.RegisterAllEvents();

			//var x = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Replace('\\', '/');
			//System.Reflection.Assembly.LoadFile( x + "/MoonSharp.Interpreter.dll");
			//print(x);
			//Now introduce all the lua shit

			//Load all the mods and shit from here!
			cmod.ModCapabilities.RunEvent("Load");
		}
	}
	public void Quit()
	{
		GameManager.instance.Quit();
	}
}
