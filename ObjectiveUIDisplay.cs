using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUIDisplay : MonoBehaviour
{
	[SerializeField]
	private Image starImage;

	[SerializeField]
	private Animator scrollAnimator;

	[SerializeField]
	private TMP_Text title;

	[SerializeField]
	private TMP_Text description;

	[SerializeField]
	private AudioPack paperRustle;

	private AudioSource paperRustleSource;

	private static readonly int Rollout = Animator.StringToHash("Rollout");

	private void OnEnable()
	{
		if (paperRustleSource == null)
		{
			paperRustleSource = base.gameObject.AddComponent<AudioSource>();
			paperRustleSource.playOnAwake = false;
			paperRustleSource.spatialBlend = 0f;
			paperRustleSource.loop = false;
			paperRustleSource.enabled = false;
		}
		ObjectiveManager.AddObjectiveSwappedListener(OnObjectiveSwapped);
		ObjectiveManager.AddObjectiveUpdatedListener(OnObjectiveUpdated);
		OnObjectiveSwapped(ObjectiveManager.GetCurrentObjective());
	}

	private void OnDisable()
	{
		ObjectiveManager.RemoveObjectiveSwappedListener(OnObjectiveSwapped);
		ObjectiveManager.RemoveObjectiveUpdatedListener(OnObjectiveUpdated);
	}

	private void OnObjectiveUpdated(DragonMailObjective objective)
	{
		if (objective != null)
		{
			title.text = objective.GetTitle();
			description.text = objective.GetTextBody();
		}
	}

	private void OnObjectiveSwapped(DragonMailObjective objective)
	{
		if (base.isActiveAndEnabled)
		{
			StopAllCoroutines();
			StartCoroutine(ObjectiveSwapRoutine(objective));
			return;
		}
		if (objective == null)
		{
			scrollAnimator.SetBool(Rollout, value: false);
			return;
		}
		title.text = objective.GetTitle();
		description.text = objective.GetTextBody();
		starImage.gameObject.SetActive(!objective.autoAdvance);
		scrollAnimator.SetBool(Rollout, value: true);
	}

	private IEnumerator ObjectiveSwapRoutine(DragonMailObjective newObjective)
	{
		if (newObjective == null)
		{
			scrollAnimator.SetBool(Rollout, value: false);
			yield break;
		}
		if (scrollAnimator.GetBool(Rollout))
		{
			scrollAnimator.SetBool(Rollout, value: false);
			yield return new WaitForSeconds(1.5f);
		}
		scrollAnimator.SetBool(Rollout, value: true);
		starImage.gameObject.SetActive(!newObjective.autoAdvance);
		title.text = newObjective.GetTitle();
		description.text = newObjective.GetTextBody();
		scrollAnimator.SetBool(Rollout, value: true);
		paperRustleSource.enabled = true;
		paperRustle.Play(paperRustleSource);
		yield return new WaitForSeconds(paperRustleSource.clip.length + 0.1f);
		paperRustleSource.enabled = false;
	}
}
