using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;

public class FreezerDoor : GenericDoor, IPunObservable, ISavable
{
	public PhotonGameObjectReference iceCube;

	public GameEventGeneric midnight;

	private bool iceCubeSpawned = false;

	public bool shouldSpawnIceCube { get; set; }

	public override void Start()
	{
		base.Start();
	}

	private void MidnightEvent()
	{
		if (base.photonView.IsMine)
		{
			iceCubeSpawned = false;
		}
	}

	public override void Use()
	{
		base.Use();
		if (base.photonView.IsMine && shouldSpawnIceCube && !iceCubeSpawned)
		{
			iceCubeSpawned = true;
			PhotonNetwork.Instantiate(iceCube.photonName, base.transform.position, Quaternion.identity, 0);
		}
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		shouldSpawnIceCube = reader.ReadBoolean();
		iceCubeSpawned = reader.ReadBoolean();
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		writer.Write(shouldSpawnIceCube);
		writer.Write(iceCubeSpawned);
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnPhotonSerializeView(stream, info);
		if (stream.IsWriting)
		{
			stream.SendNext(shouldSpawnIceCube);
			stream.SendNext(iceCubeSpawned);
		}
		else
		{
			shouldSpawnIceCube = (bool)stream.ReceiveNext();
			iceCubeSpawned = (bool)stream.ReceiveNext();
		}
	}

	private void OnValidate()
	{
		iceCube.OnValidate();
	}
}
