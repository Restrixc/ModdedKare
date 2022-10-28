using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Grabber : MonoBehaviourPun
{
	private class GrabInfo
	{
		private DriverConstraint driverConstraint;

		private ConfigurableJoint joint;

		private float springStrength;

		private float dampingStrength;

		private bool valid = true;

		private Transform grabber;

		private float grabTime;

		public IGrabbable grabbable { get; private set; }

		public Rigidbody body { get; private set; }

		public CollisionDetectionMode collisionDetectionMode { get; private set; }

		public RigidbodyInterpolation interpolation { get; private set; }

		public Kobold kobold { get; private set; }

		public Kobold owner { get; private set; }

		public GenericWeapon weapon { get; private set; }

		private void RecursiveSetLayer(Transform t, int fromLayer, int toLayer)
		{
			for (int i = 0; i < t.childCount; i++)
			{
				RecursiveSetLayer(t.GetChild(i), fromLayer, toLayer);
			}
			if (t.gameObject.layer == fromLayer)
			{
				t.gameObject.layer = toLayer;
			}
		}

		public GrabInfo(Kobold owner, Transform grabber, IGrabbable grabbable, float springStrength, float dampingStrength)
		{
			grabTime = Time.time;
			this.grabber = grabber;
			this.owner = owner;
			this.grabbable = grabbable;
			this.springStrength = springStrength;
			this.dampingStrength = dampingStrength;
			body = grabbable.transform.GetComponentInParent<Rigidbody>();
			if (body == null)
			{
				valid = false;
				return;
			}
			collisionDetectionMode = body.collisionDetectionMode;
			interpolation = body.interpolation;
			body.collisionDetectionMode = CollisionDetectionMode.Continuous;
			body.interpolation = RigidbodyInterpolation.Interpolate;
			driverConstraint = body.gameObject.AddComponent<DriverConstraint>();
			driverConstraint.springStrength = springStrength;
			driverConstraint.body = body;
			driverConstraint.connectedBody = grabber;
			driverConstraint.dampingStrength = dampingStrength;
			driverConstraint.softness = 1f;
			grabbable.photonView.RequestOwnership();
			weapon = grabbable.transform.GetComponentInParent<GenericWeapon>();
			if (weapon != null)
			{
				driverConstraint.angleSpringStrength = 32f;
				driverConstraint.angleDamping = 0.1f;
				driverConstraint.angleSpringSoftness = 60f;
				driverConstraint.connectedAnchor = weapon.GetWeaponHoldPosition();
			}
			kobold = grabbable.transform.GetComponentInParent<Kobold>();
			if (kobold != null)
			{
				Rigidbody ragdollBody = kobold.GetComponent<Ragdoller>().GetRagdollBodies()[0];
				joint = AddJoint(ragdollBody, ragdollBody.position);
			}
			RecursiveSetLayer(body.transform, LayerMask.NameToLayer("UsablePickups"), LayerMask.NameToLayer("PlayerNocollide"));
			valid = true;
		}

		public bool Valid()
		{
			bool v = (Component)grabbable != null && driverConstraint != null && valid;
			if (v && Time.time - grabTime > 2f)
			{
				v &= grabbable.photonView.IsMine;
			}
			return v;
		}

		public void Activate()
		{
			if (weapon != null)
			{
				weapon.OnFire(owner);
				return;
			}
			body.velocity += grabber.transform.forward * 10f;
			Release();
		}

		public void StopActivate()
		{
			if (weapon != null)
			{
				weapon.OnEndFire(owner);
			}
		}

		public void Release()
		{
			if (!valid)
			{
				return;
			}
			if (driverConstraint != null)
			{
				UnityEngine.Object.Destroy(driverConstraint);
			}
			if (joint != null)
			{
				UnityEngine.Object.Destroy(joint);
			}
			if ((Component)grabbable != null && body != null)
			{
				grabbable.photonView.RPC("OnReleaseRPC", RpcTarget.All, owner.photonView.ViewID, body.velocity);
				RecursiveSetLayer(body.transform, LayerMask.NameToLayer("PlayerNocollide"), LayerMask.NameToLayer("UsablePickups"));
				if (kobold != null && owner != null)
				{
					owner.GetComponent<Grabber>().AddGivebackKobold(kobold);
				}
			}
			valid = false;
		}

		public void Set(Vector3 position, Quaternion viewRot)
		{
			driverConstraint.anchor = body.transform.InverseTransformPoint(grabbable.GrabTransform().position);
			if (weapon != null)
			{
				Quaternion fq = Quaternion.FromToRotation(weapon.GetWeaponBarrelTransform().forward, viewRot * Vector3.forward) * Quaternion.FromToRotation(weapon.GetWeaponBarrelTransform().up, viewRot * Vector3.up);
				driverConstraint.forwardVector = fq * body.transform.forward;
				driverConstraint.upVector = fq * body.transform.up;
				driverConstraint.connectedAnchor = weapon.GetWeaponHoldPosition();
			}
			else
			{
				driverConstraint.connectedAnchor = Vector3.zero;
			}
			if (joint != null)
			{
				joint.connectedAnchor = position;
			}
		}

		private ConfigurableJoint AddJoint(Rigidbody targetBody, Vector3 worldPosition)
		{
			ConfigurableJoint configurableJoint = targetBody.gameObject.AddComponent<ConfigurableJoint>();
			configurableJoint.axis = Vector3.up;
			configurableJoint.secondaryAxis = Vector3.right;
			configurableJoint.connectedBody = null;
			configurableJoint.autoConfigureConnectedAnchor = false;
			configurableJoint.breakForce = float.MaxValue;
			JointDrive drive = configurableJoint.xDrive;
			drive.positionSpring = springStrength * 10f;
			drive.positionDamper = 2f;
			configurableJoint.xDrive = drive;
			configurableJoint.yDrive = drive;
			configurableJoint.zDrive = drive;
			SoftJointLimit linearLimit = configurableJoint.linearLimit;
			linearLimit.limit = 1f;
			linearLimit.bounciness = 0f;
			SoftJointLimitSpring spring = configurableJoint.linearLimitSpring;
			spring.spring = springStrength;
			spring.damper = 2f;
			configurableJoint.linearLimitSpring = spring;
			configurableJoint.linearLimit = linearLimit;
			configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
			configurableJoint.massScale = 1f;
			configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
			configurableJoint.projectionAngle = 10f;
			configurableJoint.projectionDistance = 0.5f;
			configurableJoint.connectedMassScale = 1f;
			configurableJoint.enablePreprocessing = false;
			configurableJoint.configuredInWorldSpace = true;
			configurableJoint.anchor = Vector3.zero;
			configurableJoint.connectedBody = null;
			configurableJoint.connectedAnchor = worldPosition;
			configurableJoint.xMotion = ConfigurableJointMotion.Limited;
			configurableJoint.yMotion = ConfigurableJointMotion.Limited;
			configurableJoint.zMotion = ConfigurableJointMotion.Limited;
			return configurableJoint;
		}
	}

	private class GiveBackKobold
	{
		public Coroutine routine;

		public Kobold kobold;
	}

	private class ColliderSorter : IComparer<Collider>
	{
		private Ray internalRay;

		private const int checkCount = 4;

		public void SetRay(Ray ray)
		{
			internalRay = ray;
		}

		public int Compare(Collider x, Collider y)
		{
			if ((object)x == y)
			{
				return 0;
			}
			if ((object)y == null)
			{
				return 1;
			}
			if ((object)x == null)
			{
				return -1;
			}
			float closestX = float.MaxValue;
			float closestY = float.MaxValue;
			if (x is MeshCollider meshCollider && !meshCollider.convex)
			{
				return -1;
			}
			if (y is MeshCollider meshCollider2 && !meshCollider2.convex)
			{
				return 1;
			}
			for (int i = 0; i < 4; i++)
			{
				float t = (float)i / 4f;
				Vector3 checkPoint = internalRay.GetPoint(t);
				closestX = Mathf.Min(closestX, Vector3.Distance(checkPoint, x.ClosestPoint(checkPoint)));
				closestY = Mathf.Min(closestY, Vector3.Distance(checkPoint, y.ClosestPoint(checkPoint)));
			}
			return closestX.CompareTo(closestY);
		}
	}

	public Kobold player;

	private int maxGrabCount = 1;

	private Rigidbody body;

	[SerializeField]
	private float springStrength = 1000f;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float dampingStrength = 0.1f;

	[SerializeField]
	private GameObject activateUI;

	[SerializeField]
	private GameObject throwUI;

	private ColliderSorter sorter;

	private bool activating;

	private List<GrabInfo> grabbedObjects;

	private List<GiveBackKobold> giveBackKobolds;

	private static Collider[] colliders = new Collider[32];

	[SerializeField]
	private Transform view;

	public void OnDestroy()
	{
		TryDrop();
	}

	public void SetMaxGrabCount(int count)
	{
		maxGrabCount = count;
	}

	public void Validate()
	{
		bool canActivate = false;
		bool canThrow = false;
		for (int i = 0; i < grabbedObjects.Count; i++)
		{
			if (grabbedObjects[i].weapon != null)
			{
				canActivate = true;
			}
			if (grabbedObjects[i].body != null)
			{
				canThrow = true;
			}
			if (!grabbedObjects[i].Valid())
			{
				grabbedObjects[i].Release();
				grabbedObjects.RemoveAt(i--);
			}
		}
		if ((activateUI.activeSelf && !canActivate) || (!activateUI.activeSelf && canActivate))
		{
			activateUI.SetActive(canActivate);
		}
		if (!canActivate)
		{
			if ((canThrow && !throwUI.activeSelf) || (!canThrow && throwUI.activeSelf))
			{
				throwUI.SetActive(canThrow);
			}
		}
		else if (throwUI.activeSelf)
		{
			throwUI.SetActive(value: false);
		}
	}

	private IEnumerator GiveBackKoboldAfterDelay(GiveBackKobold giveBackKobold)
	{
		while (giveBackKobold.kobold.photonView.IsMine)
		{
			if (giveBackKobold.kobold.ragdoller.ragdolled)
			{
				yield return new WaitForSeconds(5f);
			}
			else
			{
				yield return new WaitForSeconds(0.25f);
			}
			if (!giveBackKobold.kobold.photonView.IsMine)
			{
				continue;
			}
			Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Player p in playerList)
			{
				if (giveBackKobold.kobold.photonView.IsMine && (Kobold)p.TagObject == giveBackKobold.kobold)
				{
					giveBackKobold.kobold.photonView.TransferOwnership(p);
					break;
				}
			}
		}
		giveBackKobolds.Remove(giveBackKobold);
	}

	public void AddGivebackKobold(Kobold other)
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player p in playerList)
		{
			if ((Kobold)p.TagObject == other)
			{
				GiveBackKobold giveBackKobold = new GiveBackKobold
				{
					kobold = other
				};
				giveBackKobold.routine = StartCoroutine(GiveBackKoboldAfterDelay(giveBackKobold));
				giveBackKobolds.Add(giveBackKobold);
				break;
			}
		}
	}

	private void RemoveGivebackKobold(Kobold other)
	{
		if (!(other != null))
		{
			return;
		}
		for (int i = 0; i < giveBackKobolds.Count; i++)
		{
			if (giveBackKobolds[i].kobold == other)
			{
				if (giveBackKobolds[i].routine != null)
				{
					StopCoroutine(giveBackKobolds[i].routine);
				}
				giveBackKobolds.RemoveAt(i--);
			}
		}
	}

	public void TryDrop()
	{
		foreach (GrabInfo grab in grabbedObjects)
		{
			grab.Release();
		}
		grabbedObjects.Clear();
	}

	private void Awake()
	{
		grabbedObjects = new List<GrabInfo>();
		giveBackKobolds = new List<GiveBackKobold>();
		sorter = new ColliderSorter();
	}

	private void GetForwardAndUpVectors(GenericWeapon[] weapons, out Vector3 averageForward, out Vector3 averageUp, out Vector3 averageOffset)
	{
		averageForward = Vector3.zero;
		averageUp = Vector3.zero;
		averageOffset = Vector3.zero;
		foreach (GenericWeapon w in weapons)
		{
			averageForward += w.GetWeaponBarrelTransform().forward;
			averageOffset += w.GetWeaponHoldPosition();
			averageUp += w.GetWeaponBarrelTransform().up;
		}
		averageForward /= (float)weapons.Length;
		averageOffset /= (float)weapons.Length;
		averageUp /= (float)weapons.Length;
		averageForward = Vector3.Normalize(averageForward);
		averageUp = Vector3.Normalize(averageUp);
	}

	public void TryGrab()
	{
		if (grabbedObjects.Count >= maxGrabCount)
		{
			return;
		}
		Vector3 position = view.position;
		int hits = Physics.OverlapSphereNonAlloc(position, 1f, colliders);
		sorter.SetRay(new Ray(position, view.forward));
		Array.Sort(colliders, 0, hits, sorter);
		for (int i = 0; i < hits; i++)
		{
			IGrabbable grabbable = colliders[i].GetComponentInParent<IGrabbable>();
			if (grabbable == null || grabbable == player)
			{
				continue;
			}
			bool contains = false;
			foreach (GrabInfo grabInfo in grabbedObjects)
			{
				if (grabInfo.grabbable == grabbable)
				{
					contains = true;
					break;
				}
			}
			if (contains)
			{
				continue;
			}
			if (grabbable.CanGrab(player))
			{
				grabbable.photonView.RPC("OnGrabRPC", RpcTarget.All, base.photonView.ViewID);
				GrabInfo info = new GrabInfo(player, view, grabbable, springStrength, dampingStrength);
				if (!info.Valid())
				{
					break;
				}
				grabbedObjects.Add(info);
				RemoveGivebackKobold(info.kobold);
			}
			if (grabbedObjects.Count >= maxGrabCount)
			{
				break;
			}
		}
	}

	public void Update()
	{
		Validate();
		foreach (GrabInfo grab in grabbedObjects)
		{
			grab.Set(view.position, view.rotation);
		}
	}

	public void TryStopActivate()
	{
		Validate();
		if (activating)
		{
			activating = false;
			for (int i = 0; i < grabbedObjects.Count; i++)
			{
				grabbedObjects[i].StopActivate();
			}
		}
	}

	public void TryActivate()
	{
		Validate();
		if (!activating)
		{
			activating = true;
			for (int i = 0; i < grabbedObjects.Count; i++)
			{
				grabbedObjects[i].Activate();
			}
		}
	}
}
