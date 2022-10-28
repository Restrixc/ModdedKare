using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateRoomOnPress : MonoBehaviour
{
	public TMP_InputField roomNameField;

	public TMP_Dropdown saveDropdown;

	public Toggle isPrivate;

	public Slider maxPlayersField;

	public LocalizedString newGameString;

	public Sprite saveIcon;

	public void OnEnable()
	{
		if (newGameString != null && !(saveDropdown == null))
		{
			roomNameField.Select();
		}
	}

	public IEnumerator CreateRoomRoutine()
	{
		yield return GameManager.instance.StartCoroutine(SingletonScriptableObject<NetworkManager>.instance.EnsureOnlineAndReadyToLoad());
		PhotonNetwork.CreateRoom(roomNameField.text, new RoomOptions
		{
			MaxPlayers = (byte)maxPlayersField.value,
			IsVisible = !isPrivate.isOn,
			CleanupCacheOnLeave = false
		});
	}

	public void CreateRoom()
	{
		GameManager.instance.StartCoroutine(CreateRoomRoutine());
	}

	public IEnumerator JoinRoomRoutine(string roomName)
	{
		Popup p = SingletonScriptableObject<PopupHandler>.instance.SpawnPopup("Connect");
		yield return GameManager.instance.StartCoroutine(SingletonScriptableObject<NetworkManager>.instance.EnsureOnlineAndReadyToLoad());
		PhotonNetwork.JoinRoom(roomName);
		yield return new WaitUntil(() => PhotonNetwork.InRoom && SceneManager.GetActiveScene().name == "MainMap");
		SingletonScriptableObject<PopupHandler>.instance.ClearPopup(p);
	}

	public void JoinRoom()
	{
		GameManager.instance.StartCoroutine(JoinRoomRoutine(roomNameField.text));
	}
}
