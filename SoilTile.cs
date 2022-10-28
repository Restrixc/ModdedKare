using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class SoilTile : MonoBehaviourPun, IPunObservable, ISavable
{
	public delegate void FarmTileClearedAction(SoilTile tile);

	private Plant planted;

	[SerializeField]
	private bool hasDebris = false;

	[SerializeField]
	private List<GameObject> debris;

	[SerializeField]
	private PhotonGameObjectReference plantPrefab;

	public static event FarmTileClearedAction tileCleared;

	[PunRPC]
	public void SetDebris(bool newHasDebris)
	{
		hasDebris = newHasDebris;
		foreach (GameObject obj in debris)
		{
			obj.SetActive(hasDebris);
		}
		if (!hasDebris)
		{
			SoilTile.tileCleared?.Invoke(this);
		}
	}

	public bool GetPlantable()
	{
		return (planted == null || planted.plant.possibleNextGenerations.Length == 0) && !hasDebris;
	}

	[PunRPC]
	public void SetPlantedRPC(int viewID)
	{
		if (viewID == -1)
		{
			planted = null;
		}
		PhotonView view = PhotonNetwork.GetPhotonView(viewID);
		if (!(view == null) && view.TryGetComponent<Plant>(out var plant))
		{
			if (planted != null && planted.photonView.IsMine)
			{
				PhotonNetwork.Destroy(planted.gameObject);
			}
			planted = plant;
		}
	}

	public Vector3 GetPlantPosition()
	{
		return base.transform.position + Vector3.up * 0.35f;
	}

	private void Awake()
	{
		SetDebris(hasDebris);
	}

	public bool GetDebris()
	{
		return hasDebris;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(hasDebris);
		}
		else
		{
			SetDebris((bool)stream.ReceiveNext());
		}
	}

	[PunRPC]
	public void PlantRPC(int seed, short plantID, KoboldGenes myGenes)
	{
		PhotonView seedView = PhotonNetwork.GetPhotonView(seed);
		if (seedView != null && seedView.IsMine)
		{
			PhotonNetwork.Destroy(seedView);
		}
		if (PhotonNetwork.IsMasterClient)
		{
			GameObject obj = PhotonNetwork.InstantiateRoomObject(plantPrefab.photonName, GetPlantPosition(), Quaternion.LookRotation(Vector3.forward, Vector3.up), 0, new object[2] { plantID, myGenes });
			base.photonView.RPC("SetPlantedRPC", RpcTarget.All, obj.GetComponent<Plant>().photonView.ViewID);
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(hasDebris);
		if (planted == null)
		{
			writer.Write(-1);
		}
		else
		{
			writer.Write(planted.photonView.ViewID);
		}
	}

	public void Load(BinaryReader reader)
	{
		SetDebris(reader.ReadBoolean());
		int viewID = reader.ReadInt32();
		SetPlantedRPC(viewID);
	}

	private void OnValidate()
	{
		plantPrefab.OnValidate();
	}
}
