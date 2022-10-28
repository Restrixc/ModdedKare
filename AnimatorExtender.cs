using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorExtender : MonoBehaviour
{
	private Animator targetAnimator;

	private AudioSource source;

	private void Start()
	{
		source = GetComponent<AudioSource>();
		targetAnimator = GetComponent<Animator>();
	}

	public void ToggleFloat(string name)
	{
		targetAnimator.SetFloat(name, (targetAnimator.GetFloat(name) <= 0f) ? 1f : 0f);
	}

	public void SetBool(string boolName)
	{
		targetAnimator.SetBool(boolName, value: true);
	}

	public void ResetBool(string boolName)
	{
		targetAnimator.SetBool(boolName, value: false);
	}

	public void PlayAudioPack(AudioPack pack)
	{
		pack.Play(source);
	}
}
