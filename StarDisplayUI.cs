using TMPro;
using UnityEngine;

public class StarDisplayUI : MonoBehaviour
{
	[SerializeField]
	private TMP_Text text;

	private void OnEnable()
	{
		ObjectiveManager.AddObjectiveSwappedListener(OnObjectiveSwapped);
		OnObjectiveSwapped(ObjectiveManager.GetCurrentObjective());
	}

	private void OnDisable()
	{
		ObjectiveManager.RemoveObjectiveSwappedListener(OnObjectiveSwapped);
	}

	private void OnObjectiveSwapped(DragonMailObjective obj)
	{
		text.text = $"{ObjectiveManager.GetStars()}";
	}
}
