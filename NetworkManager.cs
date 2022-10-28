using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkManager : SingletonScriptableObject<NetworkManager>, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback, IPunOwnershipCallbacks
{
	public ServerSettings settings;

	[NonSerialized]
	private List<Transform> spawnPoints = new List<Transform>();

	public bool online => !PhotonNetwork.OfflineMode && PhotonNetwork.PlayerList.Length > 1;

	public bool offline => !online;

	public IEnumerator JoinLobbyRoutine(string region)
	{
		if (PhotonNetwork.OfflineMode)
		{
			PhotonNetwork.OfflineMode = false;
		}
		if (PhotonNetwork.IsConnected && settings.AppSettings.FixedRegion != region)
		{
			PhotonNetwork.Disconnect();
			yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
		}
		if (!PhotonNetwork.IsConnected)
		{
			PhotonNetwork.AutomaticallySyncScene = true;
			settings.AppSettings.FixedRegion = region;
			PhotonNetwork.ConnectUsingSettings();
		}
		yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady || (PhotonNetwork.IsConnected && PhotonNetwork.InRoom));
		if (!PhotonNetwork.InLobby)
		{
			PhotonNetwork.JoinLobby();
		}
	}

	public void JoinLobby(string region)
	{
		GameManager.instance.StartCoroutine(JoinLobbyRoutine(region));
	}

	public void QuickMatch()
	{
		GameManager.instance.StartCoroutine(QuickMatchRoutine());
	}

	public IEnumerator QuickMatchRoutine()
	{
		SingletonScriptableObject<PopupHandler>.instance.SpawnPopup("Connect");
		yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
		PhotonNetwork.JoinRandomRoom();
	}

	public void CreatePublicRoom()
	{
		GameManager.instance.StartCoroutine(CreatePublicRoomRoutine());
	}

	public IEnumerator CreatePublicRoomRoutine()
	{
		SingletonScriptableObject<PopupHandler>.instance.SpawnPopup("Connect");
		yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
		PhotonNetwork.CreateRoom(null, new RoomOptions
		{
			MaxPlayers = 8,
			CleanupCacheOnLeave = false
		});
	}

	public void JoinMatch(string roomName)
	{
		GameManager.instance.StartCoroutine(JoinMatchRoutine(roomName));
	}

	public IEnumerator JoinMatchRoutine(string roomName)
	{
		SingletonScriptableObject<PopupHandler>.instance.SpawnPopup("Connect");
		yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
		PhotonNetwork.JoinRoom(roomName);
	}

	public IEnumerator EnsureOfflineAndReadyToLoad()
	{
		PhotonPeer.RegisterType(typeof(ReagentContents), 82, ReagentContents.SerializeReagentContents, ReagentContents.DeserializeReagentContents);
		PhotonPeer.RegisterType(typeof(KoboldGenes), 71, KoboldGenes.Serialize, KoboldGenes.Deserialize);
		if (PhotonNetwork.InRoom)
		{
			PhotonNetwork.LeaveRoom();
			yield return LevelLoader.instance.LoadLevel("ErrorScene");
		}
		if (PhotonNetwork.InLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		PhotonNetwork.Disconnect();
		yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
		PhotonNetwork.OfflineMode = true;
		PhotonNetwork.EnableCloseConnection = true;
	}

	public IEnumerator EnsureOnlineAndReadyToLoad(bool shouldLeaveRoom = true)
	{
		if (PhotonNetwork.InRoom && shouldLeaveRoom)
		{
			PhotonNetwork.LeaveRoom();
			yield return LevelLoader.instance.LoadLevel("ErrorScene");
		}
		PhotonNetwork.OfflineMode = false;
		PhotonPeer.RegisterType(typeof(ReagentContents), 82, ReagentContents.SerializeReagentContents, ReagentContents.DeserializeReagentContents);
		PhotonPeer.RegisterType(typeof(KoboldGenes), 71, KoboldGenes.Serialize, KoboldGenes.Deserialize);
		if (!PhotonNetwork.IsConnected)
		{
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.ConnectUsingSettings();
		}
		yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
		PhotonNetwork.EnableCloseConnection = true;
	}

	public void StartSinglePlayer()
	{
		GameManager.instance.StartCoroutine(SinglePlayerRoutine());
	}

	public IEnumerator SinglePlayerRoutine()
	{
		yield return GameManager.instance.StartCoroutine(EnsureOfflineAndReadyToLoad());
		PhotonNetwork.OfflineMode = true;
		PhotonNetwork.JoinRandomRoom();
		yield return new WaitUntil(() => SceneManager.GetSceneByName("MainMap").isLoaded);
	}

	public void LeaveLobby()
	{
		if (PhotonNetwork.InLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
	}

	public void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster() was called by PUN.");
		Debug.Log("Using version " + PhotonNetwork.NetworkingClient.AppVersion);
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
		if (GameManager.instance != null)
		{
			GameManager.instance.StartCoroutine(OnDisconnectRoutine(cause));
		}
	}

	public IEnumerator OnDisconnectRoutine(DisconnectCause cause)
	{
		if (cause != DisconnectCause.DisconnectByClientLogic && cause != 0)
		{
			yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
			SingletonScriptableObject<PopupHandler>.instance.SpawnPopup("Disconnect", solo: true, cause.ToString());
		}
	}

	public IEnumerator OnJoinRoomFailedRoutine(short returnCode, string message)
	{
		yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
		SingletonScriptableObject<PopupHandler>.instance.ClearAllPopups();
		SingletonScriptableObject<PopupHandler>.instance.SpawnPopup("Disconnect", solo: true, "Error " + returnCode + ": " + message);
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRoomFailed() was called by PUN." + message);
		GameManager.instance.StartCoroutine(OnJoinRoomFailedRoutine(returnCode, message));
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.");
		GameManager.instance.StartCoroutine(CreatePublicRoomRoutine());
	}

	public IEnumerator SpawnControllablePlayerRoutine()
	{
		yield return new WaitUntil(() => !LevelLoader.loadingLevel);
		if (PhotonNetwork.LocalPlayer.TagObject != null && PhotonNetwork.LocalPlayer.TagObject as Kobold != null)
		{
			yield break;
		}
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			if (spawnPoints[i] == null)
			{
				spawnPoints.RemoveAt(i--);
			}
		}
		if (spawnPoints.Count == 0)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("PlayerSpawn");
			foreach (GameObject g in array)
			{
				spawnPoints.Add(g.transform);
			}
		}
		Vector3 pos = Vector3.zero;
		if (spawnPoints.Count > 0)
		{
			pos = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count - 1)].position;
		}
		GameObject player = PhotonNetwork.Instantiate("Kobold", pos, Quaternion.identity, 0, new object[2]
		{
			PlayerKoboldLoader.GetPlayerGenes(),
			true
		});
		player.GetComponentInChildren<PlayerPossession>(includeInactive: true).gameObject.SetActive(value: true);
		SingletonScriptableObject<PopupHandler>.instance.ClearAllPopups();
	}

	public void SpawnControllablePlayer()
	{
		GameManager.instance.StartCoroutine(SpawnControllablePlayerRoutine());
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
		SpawnControllablePlayer();
		SingletonScriptableObject<PopupHandler>.instance.ClearAllPopups();
		GameManager.instance.Pause(pause: false);
	}

	public void OnPlayerEnteredRoom(Player other)
	{
		Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
		}
	}

	public void OnPlayerLeftRoom(Player other)
	{
		Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName);
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
		}
	}

	public void OnConnected()
	{
		Debug.Log("Connected.");
	}

	public void OnCreatedRoom()
	{
		if (SceneManager.GetActiveScene().name != "MainMap" && SceneManager.GetActiveScene().name != "MainMapRedo")
		{
			LevelLoader.instance.LoadLevel("MainMap");
		}
	}

	public void OnLeftRoom()
	{
		Debug.Log("Left room");
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("Master switched!" + newMasterClient);
	}

	public void OnJoinedLobby()
	{
		Debug.Log("Joined lobby");
	}

	public void OnLeftLobby()
	{
		Debug.Log("Left lobby i guess");
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach (RoomInfo room in roomList)
		{
			Debug.Log("Got room info list:" + room);
		}
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		Debug.Log("Friends update:" + friendList);
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		Debug.Log("Custom auth i guess" + data);
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		Debug.Log("Custom auth failed" + debugMessage);
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
		Debug.Log("lobby update " + lobbyStatistics);
	}

	public void OnErrorInfo(ErrorInfo errorInfo)
	{
		Debug.Log("Photon error: " + errorInfo);
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
	}

	public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
	}

	public void OnWebRpcResponse(OperationResponse response)
	{
	}

	public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
		Kobold i = targetView.GetComponent<Kobold>();
		if (i != (Kobold)PhotonNetwork.LocalPlayer.TagObject)
		{
			targetView.TransferOwnership(requestingPlayer);
		}
		else if (!i.GetComponentInChildren<PlayerInput>().actions["Jump"].IsPressed() || requestingPlayer == PhotonNetwork.LocalPlayer)
		{
			targetView.TransferOwnership(requestingPlayer);
		}
	}

	public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
	}

	public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
	{
	}
}
