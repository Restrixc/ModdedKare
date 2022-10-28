using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelByName : MonoBehaviour
{
	public void LoadLevel(string s)
	{
		SceneManager.LoadScene(s);
	}
}
