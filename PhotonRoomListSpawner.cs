using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotonRoomListSpawner : MonoBehaviourPunCallbacks, ILobbyCallbacks, IInRoomCallbacks
{
	public GameObject roomPrefab;

	public GameObject hideOnRoomsFound;

	private List<GameObject> roomPrefabs = new List<GameObject>();

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		Debug.Log("PhotonRoomListSpawner :: Connected to master");
	}

	public override void OnLeftLobby()
	{
		ClearRoomList();
		Debug.Log("[PhotonRoomListSpawner] :: Player left lobby");
	}

	private IEnumerator RefreshRoomRoutine()
	{
		while (base.isActiveAndEnabled)
		{
			if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
			{
				PhotonNetwork.JoinLobby();
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(RefreshRoomRoutine());
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		Debug.Log("[PhotonRoomListSpawner] :: Got room list update from master server");
		ClearRoomList();
		foreach (RoomInfo info in roomList)
		{
			if (!info.RemovedFromList)
			{
				GameObject room = Object.Instantiate(roomPrefab, base.transform);
				roomPrefabs.Add(room);
				room.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = info.Name;
				room.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = info.PlayerCount + "/" + info.MaxPlayers;
				if (info.IsOpen)
				{
					room.transform.Find("Image").gameObject.SetActive(value: false);
				}
				room.GetComponent<Button>().onClick.AddListener(delegate
				{
					SingletonScriptableObject<NetworkManager>.instance.JoinMatch(info.Name);
				});
			}
		}
		hideOnRoomsFound.SetActive(roomList.Count == 0);
	}

	private void ClearRoomList()
	{
		foreach (GameObject g in roomPrefabs)
		{
			Object.Destroy(g);
		}
		roomPrefabs.Clear();
	}
}
