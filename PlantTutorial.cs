using UnityEngine;

public class PlantTutorial : MonoBehaviour
{
	[SerializeField]
	private Canvas waterCanvas;

	[SerializeField]
	private Canvas timeCanvas;

	private GenericReagentContainer container;

	private Plant plant;

	private void Start()
	{
		plant = GetComponent<Plant>();
		container = GetComponent<GenericReagentContainer>();
		container.OnFilled.AddListener(OnFilled);
		plant.switched += OnSwitched;
	}

	private void OnDestroy()
	{
		if (plant != null)
		{
			plant.switched -= OnSwitched;
			container.OnFilled.RemoveListener(OnFilled);
		}
	}

	private void OnFilled(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		if (plant.plant.possibleNextGenerations.Length == 0)
		{
			waterCanvas.gameObject.SetActive(value: false);
			timeCanvas.gameObject.SetActive(value: false);
		}
		else
		{
			timeCanvas.gameObject.SetActive(value: true);
			waterCanvas.gameObject.SetActive(value: false);
		}
	}

	private void OnSwitched()
	{
		if (plant.plant.possibleNextGenerations.Length == 0)
		{
			waterCanvas.gameObject.SetActive(value: false);
			timeCanvas.gameObject.SetActive(value: false);
		}
		else
		{
			timeCanvas.gameObject.SetActive(value: false);
			waterCanvas.gameObject.SetActive(value: true);
		}
	}
}
