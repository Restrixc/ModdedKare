using System.Collections;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

public class QuitFromIngame : MonoBehaviour
{
	public void QuitToMenu()
	{
		GameManager.instance.StartCoroutine(QuitToMenuRoutine());
	}

	public IEnumerator QuitToMenuRoutine()
	{
		PhotonNetwork.Disconnect();
		yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
		yield return LevelLoader.instance.LoadLevel("MainMenu");
		PhotonNetwork.OfflineMode = false;
		//System.IO.File.WriteAllText("Uwu.wasd", "wasdawsdwasd");
	}
}
