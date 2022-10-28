using System.IO;
using Photon.Pun;
using UnityEngine;

public class SmoothRigidbodyPhoton : MonoBehaviourPun, IPunObservable, ISavable
{
	private struct Frame
	{
		public Vector3 position;

		public Quaternion rotation;

		public float time;

		public Frame(Vector3 pos, Quaternion rotation, float time)
		{
			position = pos;
			this.rotation = rotation;
			this.time = time;
		}
	}

	private Frame lastFrame;

	private Frame newFrame;

	private bool init = false;

	private Rigidbody body;

	private void Awake()
	{
		body = GetComponent<Rigidbody>();
		lastFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
		newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
	}

	private void LateUpdate()
	{
		if (base.photonView.IsMine)
		{
			body.isKinematic = false;
			return;
		}
		body.isKinematic = true;
		float time = Time.time - 1f / (float)PhotonNetwork.SerializationRate;
		float diff = newFrame.time - lastFrame.time;
		if (diff == 0f)
		{
			body.transform.position = newFrame.position;
			body.transform.rotation = newFrame.rotation;
		}
		else
		{
			float t = (time - lastFrame.time) / diff;
			body.transform.position = Vector3.LerpUnclamped(lastFrame.position, newFrame.position, Mathf.Clamp(t, -0.25f, 1.25f));
			body.transform.rotation = Quaternion.LerpUnclamped(lastFrame.rotation, newFrame.rotation, Mathf.Clamp(t, -0.25f, 1.25f));
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(body.transform.position);
			stream.SendNext(body.transform.rotation);
			lastFrame = newFrame;
			newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
			return;
		}
		lastFrame = newFrame;
		newFrame = new Frame((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext(), Time.time);
		if (!init)
		{
			lastFrame = newFrame;
			init = true;
		}
	}

	public void Save(BinaryWriter writer)
	{
		Vector3 position = body.transform.position;
		writer.Write(position.x);
		writer.Write(position.y);
		writer.Write(position.z);
	}

	public void Load(BinaryReader reader)
	{
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float z = reader.ReadSingle();
		body.transform.position = new Vector3(x, y, z);
	}
}
