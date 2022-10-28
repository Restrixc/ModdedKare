using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Animator))]
public class GenericDoor : GenericUsable, IPunObservable, ISavable
{
	public AudioClip openSFX;

	public AudioClip closeSFX;

	public Sprite openSprite;

	public Sprite closeSprite;

	public VisualEffect activeWhenOpen;

	public AudioSource soundWhileOpen;

	public Animator animator;

	private AudioSource audioSource;

	private int usedCount;

	protected bool opened => usedCount % 2 != 0;

	public virtual void Start()
	{
		audioSource = GetComponent<AudioSource>();
		UpdateState();
	}

	public override Sprite GetSprite(Kobold k)
	{
		return opened ? closeSprite : openSprite;
	}

	[PunRPC]
	public override void Use()
	{
		base.Use();
		usedCount++;
		UpdateState();
	}

	private void UpdateState()
	{
		if (opened)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	protected virtual void Open()
	{
		animator.SetBool("Open", value: true);
		audioSource.Stop();
		audioSource.PlayOneShot(openSFX);
		if (activeWhenOpen != null)
		{
			activeWhenOpen.SendEvent("Fire");
		}
		if (soundWhileOpen != null)
		{
			soundWhileOpen.Play();
		}
	}

	protected virtual void Close()
	{
		animator.SetBool("Open", value: false);
		audioSource.Stop();
		audioSource.PlayOneShot(closeSFX);
		if (activeWhenOpen != null)
		{
			activeWhenOpen.Stop();
		}
		if (soundWhileOpen != null)
		{
			soundWhileOpen.Stop();
		}
	}
}
