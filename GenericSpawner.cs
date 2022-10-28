using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GenericSpawner : MonoBehaviourPun
{
	[SerializeField]
	private List<PhotonGameObjectReference> possibleSpawns = new List<PhotonGameObjectReference>();

	[SerializeField]
	private bool spawnOnLoad;

	private GameObject lastSpawned;

	private WaitUntil waitUntilCanSpawn;

	[SerializeField]
	private float minRespawnTime = 60f;

	[SerializeField]
	private float maxRespawnTime = 360f;

	private string GetRandomPrefab()
	{
		return possibleSpawns[Random.Range(0, possibleSpawns.Count)].photonName;
	}

	public virtual void Spawn()
	{
		if (PhotonNetwork.IsMasterClient && (!(lastSpawned != null) || !(lastSpawned.transform.DistanceTo(base.transform) < 1f)))
		{
			StartCoroutine(SpawnRoutine());
		}
	}

	public virtual bool CanSpawn()
	{
		return PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady;
	}

	public virtual IEnumerator SpawnRoutine()
	{
		yield return waitUntilCanSpawn;
		string randomPrefab = GetRandomPrefab();
		lastSpawned = PhotonNetwork.InstantiateRoomObject(randomPrefab, base.transform.position, base.transform.rotation, 0, new object[2]
		{
			new KoboldGenes().Randomize(),
			false
		});
	}

	private void OnEnable()
	{
		StartCoroutine(SpawnOccasionallyRoutine());
	}

	public virtual void Start()
	{
		waitUntilCanSpawn = new WaitUntil(CanSpawn);
		if (possibleSpawns.Count <= 0)
		{
			Debug.LogWarning("Spawner without anything to spawn...", base.gameObject);
		}
		if (spawnOnLoad)
		{
			Spawn();
		}
	}

	private IEnumerator SpawnOccasionallyRoutine()
	{
		while (base.isActiveAndEnabled)
		{
			yield return new WaitForSeconds(Random.Range(minRespawnTime, maxRespawnTime));
			Spawn();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "ico_spawn.png", allowScaling: true);
	}

	public void OnValidate()
	{
		foreach (PhotonGameObjectReference photonGameObject in possibleSpawns)
		{
			photonGameObject.OnValidate();
		}
	}
}
