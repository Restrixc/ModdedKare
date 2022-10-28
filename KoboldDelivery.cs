using System.Collections;
using System.Globalization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class KoboldDelivery : UsableMachine
{
	public delegate void SpawnedKoboldAction(Kobold kob);

	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private RotateSelectorUsable priceSelector;

	[SerializeField]
	private Transform popOutLocation;

	[SerializeField]
	private VisualEffect poof;

	[SerializeField]
	private PhotonGameObjectReference koboldPrefab;

	[SerializeField]
	private Animator targetAnimator;

	[SerializeField]
	private MoneyFloater floater;

	[SerializeField]
	private AudioPack popPack;

	private AudioSource source;

	private static readonly int Dispense = Animator.StringToHash("Dispense");

	public static event SpawnedKoboldAction spawnedKobold;

	private float GetPrice()
	{
		return 50f + (float)priceSelector.GetSelected() * 50f * (float)priceSelector.GetSelected();
	}

	private void Awake()
	{
		priceSelector.rotated += OnRotated;
		if (source == null)
		{
			source = base.gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.maxDistance = 10f;
			source.minDistance = 0.2f;
			source.rolloffMode = AudioRolloffMode.Linear;
			source.spatialBlend = 1f;
			source.loop = false;
			source.enabled = false;
		}
	}

	private void OnRotated(int newRotation)
	{
		floater.SetText(GetPrice().ToString(CultureInfo.CurrentCulture));
	}

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override bool CanUse(Kobold k)
	{
		MoneyHolder holder = k.GetComponent<MoneyHolder>();
		return constructed && holder.HasMoney(GetPrice());
	}

	public override void LocalUse(Kobold k)
	{
		if (CanUse(k))
		{
			MoneyHolder holder = k.GetComponent<MoneyHolder>();
			holder.ChargeMoney(GetPrice());
			base.LocalUse(k);
		}
	}

	public override void Use()
	{
		base.Use();
		StartCoroutine(DispenseKobold());
	}

	private IEnumerator DispenseKobold()
	{
		targetAnimator.SetTrigger(Dispense);
		yield return new WaitForSeconds(2f);
		if (base.photonView.IsMine)
		{
			poof.SendEvent("TriggerPoof");
			KoboldGenes genes = new KoboldGenes().Randomize(0.4f + (float)(priceSelector.GetSelected() * priceSelector.GetSelected()));
			GameObject obj = PhotonNetwork.InstantiateRoomObject(koboldPrefab.photonName, popOutLocation.transform.position, Quaternion.identity, 0, new object[2] { genes, false });
			KoboldDelivery.spawnedKobold?.Invoke(obj.GetComponent<Kobold>());
			source.enabled = true;
			popPack.Play(source);
			yield return new WaitForSeconds(source.clip.length + 0.1f);
			source.enabled = false;
		}
	}

	private void OnValidate()
	{
		koboldPrefab.OnValidate();
	}
}
