using System.Collections.Generic;
using System.IO;
using JigglePhysics;
using Naelstrof.BodyProportion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Ragdoller : MonoBehaviourPun, IPunObservable, ISavable, IOnPhotonViewOwnerChange, IPhotonViewCallback
{
	public delegate void RagdollEventHandler(bool ragdolled);

	private class SavedJointAnchor
	{
		private ConfigurableJoint joint;

		private Vector3 jointAnchor;

		public SavedJointAnchor(ConfigurableJoint joint)
		{
			this.joint = joint;
			jointAnchor = joint.connectedAnchor;
		}

		public void Set()
		{
			joint.connectedAnchor = jointAnchor;
		}
	}

	private class RigidbodyNetworkInfo
	{
		private struct Packet
		{
			public float time;

			public Vector3 networkedPosition;

			public Quaternion networkedRotation;

			public Packet(float t, Vector3 p, Quaternion rot)
			{
				time = t;
				networkedPosition = p;
				networkedRotation = rot;
			}
		}

		private Packet lastPacket;

		private Packet nextPacket;

		public Rigidbody body { get; private set; }

		public RigidbodyNetworkInfo(Rigidbody body)
		{
			this.body = body;
			lastPacket = new Packet(Time.time, body.transform.position, body.transform.rotation);
			nextPacket = new Packet(Time.time, body.transform.position, body.transform.rotation);
		}

		public void SetNetworkPosition(Vector3 position, Quaternion rotation, float time)
		{
			lastPacket = nextPacket;
			nextPacket = new Packet(time, position, rotation);
		}

		public void UpdateState(bool ours, bool ragdolled)
		{
			if (ours)
			{
				body.isKinematic = !ragdolled;
				body.interpolation = (ragdolled ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
				return;
			}
			body.isKinematic = true;
			body.interpolation = RigidbodyInterpolation.None;
			if (ragdolled)
			{
				float time = Time.time - 1f / (float)PhotonNetwork.SerializationRate;
				float diff = nextPacket.time - lastPacket.time;
				if (diff != 0f)
				{
					float t = (time - lastPacket.time) / diff;
					body.transform.position = Vector3.LerpUnclamped(lastPacket.networkedPosition, nextPacket.networkedPosition, Mathf.Clamp(t, -0.25f, 1.25f));
					body.transform.rotation = Quaternion.LerpUnclamped(lastPacket.networkedRotation, nextPacket.networkedRotation, Mathf.Clamp(t, -0.25f, 1.25f));
				}
			}
		}
	}

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private KoboldCharacterController controller;

	[SerializeField]
	private Rigidbody[] ragdollBodies;

	[SerializeField]
	private Rigidbody body;

	private CollisionDetectionMode oldCollisionMode;

	[SerializeField]
	private BodyProportionBase bodyProportion;

	private int ragdollCount;

	[SerializeField]
	private Transform hip;

	[SerializeField]
	private JiggleRigBuilder tailRig;

	private Kobold kobold;

	[SerializeField]
	private LODGroup group;

	private bool locked;

	private List<SavedJointAnchor> jointAnchors;

	private List<RigidbodyNetworkInfo> rigidbodyNetworkInfos;

	public bool ragdolled { get; private set; }

	public event RagdollEventHandler RagdollEvent;

	public Rigidbody[] GetRagdollBodies()
	{
		return ragdollBodies;
	}

	private void FixedUpdate()
	{
		if (ragdolled)
		{
			Vector3 diff = base.transform.position - hip.position;
			base.transform.position -= diff * 0.5f;
			hip.position += diff * 0.5f;
		}
	}

	public void SetLocked(bool newLockState)
	{
		locked = newLockState;
		if (locked && ragdolled)
		{
			SetRagdolled(ragdolled: false, ragdollCount);
		}
		else if (ragdollCount > 0)
		{
			SetRagdolled(ragdolled: true, ragdollCount);
		}
	}

	private void Awake()
	{
		kobold = GetComponent<Kobold>();
		jointAnchors = new List<SavedJointAnchor>();
		Rigidbody[] array = ragdollBodies;
		foreach (Rigidbody ragdollBody in array)
		{
			if (ragdollBody.TryGetComponent<ConfigurableJoint>(out var joint))
			{
				jointAnchors.Add(new SavedJointAnchor(joint));
				joint.autoConfigureConnectedAnchor = false;
			}
		}
		rigidbodyNetworkInfos = new List<RigidbodyNetworkInfo>();
		Rigidbody[] array2 = ragdollBodies;
		foreach (Rigidbody ragdollBody2 in array2)
		{
			rigidbodyNetworkInfos.Add(new RigidbodyNetworkInfo(ragdollBody2));
		}
	}

	[PunRPC]
	public void PushRagdoll()
	{
		ragdollCount++;
		ragdollCount = Mathf.Max(0, ragdollCount);
		if (!locked)
		{
			if (ragdollCount > 0 && !ragdolled)
			{
				Ragdoll();
			}
			else if (ragdollCount == 0 && ragdolled)
			{
				StandUp();
			}
		}
	}

	[PunRPC]
	public void PopRagdoll()
	{
		ragdollCount--;
		ragdollCount = Mathf.Max(0, ragdollCount);
		if (!locked)
		{
			if (ragdollCount > 0 && !ragdolled)
			{
				Ragdoll();
			}
			else if (ragdollCount == 0 && ragdolled)
			{
				StandUp();
			}
		}
	}

	private void LateUpdate()
	{
		foreach (RigidbodyNetworkInfo networkInfo in rigidbodyNetworkInfos)
		{
			networkInfo.UpdateState(base.photonView.IsMine, ragdolled);
		}
	}

	private void Ragdoll()
	{
		if (ragdolled)
		{
			return;
		}
		foreach (DickInfo.DickSet dickSet in kobold.activeDicks)
		{
			foreach (Kobold.PenetrableSet penn in kobold.penetratables)
			{
				if (penn.penetratable.name.Contains("Mouth"))
				{
					dickSet.dick.RemoveIgnorePenetrable(penn.penetratable);
				}
			}
		}
		LOD[] lODs = group.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			LOD lod = lODs[i];
			Renderer[] renderers = lod.renderers;
			foreach (Renderer renderer in renderers)
			{
				if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
				{
					skinnedMeshRenderer.updateWhenOffscreen = true;
				}
			}
		}
		group.ForceLOD(0);
		tailRig.enabled = false;
		animator.enabled = false;
		bodyProportion.enabled = false;
		controller.enabled = false;
		Rigidbody[] array = ragdollBodies;
		foreach (Rigidbody b in array)
		{
			b.velocity = body.velocity;
			b.isKinematic = false;
			b.collisionDetectionMode = CollisionDetectionMode.Continuous;
			b.interpolation = RigidbodyInterpolation.Interpolate;
		}
		oldCollisionMode = body.collisionDetectionMode;
		body.collisionDetectionMode = CollisionDetectionMode.Discrete;
		body.isKinematic = true;
		body.detectCollisions = false;
		Physics.SyncTransforms();
		bodyProportion.ScaleSkeleton();
		Physics.SyncTransforms();
		foreach (SavedJointAnchor savedJointAnchor in jointAnchors)
		{
			savedJointAnchor.Set();
		}
		body.isKinematic = true;
		this.RagdollEvent?.Invoke(ragdolled: true);
		ragdolled = true;
	}

	private void SetRagdolled(bool ragdolled, int newRagdollCount = 0)
	{
		if (ragdolled)
		{
			Ragdoll();
		}
		else
		{
			StandUp();
		}
		ragdollCount = newRagdollCount;
	}

	private void StandUp()
	{
		if (!ragdolled)
		{
			return;
		}
		foreach (DickInfo.DickSet dickSet in kobold.activeDicks)
		{
			foreach (Kobold.PenetrableSet penn in kobold.penetratables)
			{
				if (penn.penetratable.name.Contains("Mouth"))
				{
					dickSet.dick.AddIgnorePenetrable(penn.penetratable);
				}
			}
		}
		LOD[] lODs = group.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			LOD lod = lODs[i];
			Renderer[] renderers = lod.renderers;
			foreach (Renderer renderer in renderers)
			{
				if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
				{
					skinnedMeshRenderer.updateWhenOffscreen = false;
				}
			}
		}
		group.ForceLOD(-1);
		tailRig.enabled = true;
		Vector3 diff = hip.position - body.transform.position;
		body.transform.position += diff;
		hip.position -= diff;
		body.transform.position += Vector3.up * 0.5f;
		Vector3 forward = hip.forward;
		float? y = 0f;
		Vector3 facingDir = forward.With(null, y).normalized;
		body.transform.forward = facingDir;
		body.isKinematic = false;
		body.detectCollisions = true;
		body.collisionDetectionMode = oldCollisionMode;
		Vector3 averageVel = Vector3.zero;
		Rigidbody[] array = ragdollBodies;
		foreach (Rigidbody b in array)
		{
			averageVel += b.velocity;
		}
		averageVel /= (float)ragdollBodies.Length;
		body.velocity = averageVel;
		controller.enabled = true;
		Rigidbody[] array2 = ragdollBodies;
		foreach (Rigidbody b2 in array2)
		{
			b2.interpolation = RigidbodyInterpolation.None;
			b2.collisionDetectionMode = CollisionDetectionMode.Discrete;
			b2.isKinematic = true;
		}
		animator.enabled = true;
		bodyProportion.enabled = true;
		controller.enabled = true;
		this.RagdollEvent?.Invoke(ragdolled: false);
		ragdolled = false;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(ragdolled);
			if (ragdolled)
			{
				for (int k = 0; k < rigidbodyNetworkInfos.Count; k++)
				{
					Rigidbody ragbody = rigidbodyNetworkInfos[k].body;
					stream.SendNext(ragbody.transform.position);
					stream.SendNext(ragbody.transform.rotation);
					rigidbodyNetworkInfos[k].SetNetworkPosition(ragbody.transform.position, ragbody.transform.rotation, Time.time);
				}
			}
			return;
		}
		float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
		if ((bool)stream.ReceiveNext())
		{
			for (int j = 0; j < ragdollBodies.Length; j++)
			{
				rigidbodyNetworkInfos[j].SetNetworkPosition((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext(), Time.time);
			}
		}
		else
		{
			for (int i = 0; i < ragdollBodies.Length; i++)
			{
				rigidbodyNetworkInfos[i].SetNetworkPosition(ragdollBodies[i].transform.position, ragdollBodies[i].transform.rotation, Time.time);
			}
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(ragdolled);
	}

	public void Load(BinaryReader reader)
	{
		SetRagdolled(reader.ReadBoolean());
	}

	public void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		if (newOwner == PhotonNetwork.LocalPlayer)
		{
			ragdollCount = 0;
		}
	}
}
