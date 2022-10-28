using UnityEngine;

public class MailboxUsable : GenericUsable
{
	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private AudioPack mailWaiting;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private GameObject spaceBeam;

	private AudioSource mailWaitingSource;

	private bool hasMail = true;

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	private void Start()
	{
		if (mailWaitingSource == null)
		{
			mailWaitingSource = base.gameObject.AddComponent<AudioSource>();
			mailWaitingSource.playOnAwake = false;
			mailWaitingSource.maxDistance = 12f;
			mailWaitingSource.minDistance = 1f;
			mailWaitingSource.rolloffMode = AudioRolloffMode.Linear;
			mailWaitingSource.spatialBlend = 1f;
			mailWaitingSource.loop = true;
		}
		spaceBeam.SetActive(value: true);
		mailWaiting.Play(mailWaitingSource);
		ObjectiveManager.AddObjectiveSwappedListener(OnObjectiveSwapped);
		OnObjectiveSwapped(ObjectiveManager.GetCurrentObjective());
	}

	private void OnDestroy()
	{
		ObjectiveManager.RemoveObjectiveSwappedListener(OnObjectiveSwapped);
	}

	private void OnObjectiveSwapped(DragonMailObjective obj)
	{
		if (obj != null || !ObjectiveManager.HasMail())
		{
			hasMail = false;
			mailWaitingSource.Stop();
			mailWaitingSource.enabled = false;
			animator.SetBool("HasMail", value: false);
			animator.SetTrigger("GetMail");
			spaceBeam.SetActive(value: false);
		}
		else
		{
			hasMail = true;
			mailWaitingSource.enabled = true;
			mailWaiting.Play(mailWaitingSource);
			animator.SetBool("HasMail", value: true);
			spaceBeam.SetActive(value: true);
		}
	}

	public override bool CanUse(Kobold k)
	{
		return hasMail && ObjectiveManager.GetCurrentObjective() == null;
	}

	public override void Use()
	{
		base.Use();
		hasMail = false;
		ObjectiveManager.GetMail();
		mailWaitingSource.Stop();
		mailWaitingSource.enabled = false;
		animator.SetBool("HasMail", value: false);
		animator.SetTrigger("GetMail");
	}
}
