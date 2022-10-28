using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveUIDisplay : MonoBehaviour
{
	public Transform targetPanel;

	public GameObject savePrefab;

	public GameObject noSavesText;

	public CanvasGroup saveCheckmark;

	public CanvasGroup failCross;

	private List<GameObject> saveList = new List<GameObject>();

	private void OnEnable()
	{
		RefreshUI();
	}

	public void RefreshUI(bool shouldRefresh = true)
	{
		foreach (GameObject g in saveList)
		{
			UnityEngine.Object.Destroy(g);
		}
		SaveManager.Init();
		List<SaveManager.SaveData> saveData = SaveManager.GetSaveDatas();
		if (saveData.Count != 0)
		{
			noSavesText.SetActive(value: false);
			foreach (SaveManager.SaveData save in saveData)
			{
				GameObject newSaveItem = UnityEngine.Object.Instantiate(savePrefab, targetPanel);
				newSaveItem.transform.Find("SaveImageBorder").transform.GetChild(0).GetComponent<RawImage>().texture = save.image;
				newSaveItem.transform.Find("SaveNameImage").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileName(save.fileName);
				newSaveItem.transform.Find("LoadDeletePanel").transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
				{
					SaveManager.Load(save.fileName);
				});
				newSaveItem.transform.Find("LoadDeletePanel").transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
				{
					RefreshUI(SaveManager.RemoveSave(save.fileName));
				});
				saveList.Add(newSaveItem);
			}
			if (!(saveList[0] != null))
			{
				return;
			}
			Transform find = saveList[0].transform.Find("LoadDeletePanel");
			if (find != null)
			{
				Transform child = find.GetChild(0);
				if (child != null)
				{
					child.GetComponent<Button>().Select();
				}
			}
		}
		else
		{
			noSavesText.SetActive(value: true);
		}
	}

	public void AddNewSave()
	{
		DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		SaveManager.Save(((int)(DateTime.UtcNow - epochStart).TotalSeconds).ToString(), delegate
		{
			StartCoroutine(FadeGroup(saveCheckmark));
			RefreshUI();
		});
	}

	public IEnumerator FadeGroup(CanvasGroup group)
	{
		group.alpha = 1f;
		yield return new WaitForSecondsRealtime(1f);
		while (group.alpha != 0f)
		{
			group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.unscaledDeltaTime);
			yield return new WaitForEndOfFrame();
		}
	}
}
