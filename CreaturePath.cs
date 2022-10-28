using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;

public class CreaturePath : CatmullDisplay
{
	[SerializeField]
	[Range(-3f, 3f)]
	private float tension = 0.5f;

	[SerializeField]
	private Transform[] followTransforms;

	[SerializeField]
	private PhotonGameObjectReference creatureToSpawn;

	[SerializeField]
	private float spawnDelay = 240f;

	private WaitForSeconds waitForSeconds;

	private PhotonView trackedCreature;

	public PhotonView photonView { get; private set; }

	private void OnValidate()
	{
		Transform[] array = followTransforms;
		if (array != null && array.Length > 2)
		{
			if (path == null)
			{
				path = new CatmullSpline();
			}
			List<Vector3> points = new List<Vector3>();
			Transform[] array2 = followTransforms;
			foreach (Transform t in array2)
			{
				points.Add(t.position);
			}
			path.SetWeightsFromPoints(points, tension);
		}
		creatureToSpawn.OnValidate();
	}

	private void Start()
	{
		List<Vector3> points = new List<Vector3>();
		Transform[] array = followTransforms;
		foreach (Transform t in array)
		{
			points.Add(t.position);
		}
		path.SetWeightsFromPoints(points, tension);
		waitForSeconds = new WaitForSeconds(spawnDelay);
		photonView = GetComponentInParent<PhotonView>();
		StartCoroutine(Think());
	}

	public CatmullSpline GetSpline()
	{
		return path;
	}

	[PunRPC]
	private void SetCreature(int viewID)
	{
		trackedCreature = PhotonNetwork.GetPhotonView(viewID);
	}

	private IEnumerator Think()
	{
		while (base.isActiveAndEnabled)
		{
			yield return waitForSeconds;
			if (trackedCreature == null && photonView.IsMine)
			{
				GameObject obj = PhotonNetwork.Instantiate(creatureToSpawn.photonName, path.GetPositionFromT(0f), Quaternion.identity, 0, new object[1] { photonView.ViewID });
				photonView.RPC("SetCreature", RpcTarget.All, obj.GetPhotonView().ViewID);
			}
		}
	}
}
