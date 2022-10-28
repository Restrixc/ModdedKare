using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BucketWeapon : GenericWeapon
{
	public delegate void FoodCreateAction(BucketWeapon bucket, ScriptableReagent food);

	[SerializeField]
	private PhotonGameObjectReference bucketSplashProjectile;

	[SerializeField]
	private GenericReagentContainer container;

	[SerializeField]
	private Animator bucketAnimator;

	[SerializeField]
	private GameObject defaultBucketDisplay;

	[SerializeField]
	private Rigidbody body;

	private static readonly int Fire = Animator.StringToHash("Fire");

	private Kobold playerFired;

	[SerializeField]
	private AudioPack bucketSlosh;

	private AudioSource audioSource;

	private WaitForSeconds waitForSeconds;

	[SerializeField]
	private int projectileCount = 1;

	[SerializeField]
	private float projectileVolume = 10f;

	private GameObject currentDisplay;

	public static event FoodCreateAction foodCreated;

	private void Start()
	{
		if (audioSource == null)
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.maxDistance = 20f;
			audioSource.minDistance = 0.2f;
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.spatialBlend = 1f;
			audioSource.loop = false;
		}
		container.OnChange.AddListener(OnReagentsChanged);
		audioSource.enabled = false;
		waitForSeconds = new WaitForSeconds(5f);
		defaultBucketDisplay.SetActive(value: true);
		OnReagentsChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
	}

	private void OnDestroy()
	{
		container.OnChange.RemoveListener(OnReagentsChanged);
	}

	private void OnReagentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		GameObject bestDisplay = null;
		float bestVolume = 0f;
		short bestID = -1;
		foreach (Reagent reagent in contents)
		{
			if (!(ReagentDatabase.GetReagent(reagent.id).GetDisplayPrefab() == null) && !(reagent.volume < 5f) && reagent.volume > bestVolume)
			{
				bestDisplay = ReagentDatabase.GetReagent(reagent.id).GetDisplayPrefab();
				bestVolume = reagent.volume;
				bestID = reagent.id;
			}
		}
		if ((bestDisplay == null && currentDisplay != null) || (currentDisplay != null && bestDisplay != null && !currentDisplay.name.Contains(bestDisplay.name)))
		{
			Object.Destroy(currentDisplay);
			defaultBucketDisplay.SetActive(value: true);
		}
		if (bestDisplay != null && currentDisplay == null)
		{
			BucketWeapon.foodCreated?.Invoke(this, ReagentDatabase.GetReagent(bestID));
			currentDisplay = Object.Instantiate(bestDisplay, base.transform);
			defaultBucketDisplay.SetActive(value: false);
		}
	}

	[PunRPC]
	protected override void OnFireRPC(int viewID)
	{
		base.OnFireRPC(viewID);
		bucketAnimator.SetTrigger(Fire);
		playerFired = PhotonNetwork.GetPhotonView(viewID).GetComponentInParent<Kobold>();
	}

	public void OnFireComplete()
	{
		if (!base.photonView.IsMine || container.volume < 0.1f)
		{
			return;
		}
		for (int i = 0; i < projectileCount; i++)
		{
			Vector3 velocity = GetWeaponBarrelTransform().forward * 10f;
			if (playerFired != null)
			{
				velocity += playerFired.body.velocity * 0.5f;
			}
			velocity += Random.insideUnitSphere * i * 2f;
			GameObject obj = PhotonNetwork.Instantiate(bucketSplashProjectile.photonName, GetWeaponBarrelTransform().position, GetWeaponBarrelTransform().rotation, 0, new object[3]
			{
				container.Spill(projectileVolume),
				velocity,
				container.GetGenes()
			});
			obj.GetComponent<Projectile>().LaunchFrom(body);
		}
		audioSource.enabled = true;
		bucketSlosh.Play(audioSource);
	}

	private IEnumerator WaitSomeTimeThenDisableAudio()
	{
		yield return waitForSeconds;
		audioSource.enabled = false;
	}

	private void OnValidate()
	{
		bucketSplashProjectile.OnValidate();
	}
}
