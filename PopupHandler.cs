using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopupHandler : SingletonScriptableObject<PopupHandler>
{
	[Serializable]
	public class PopupInfo
	{
		public string name;

		public GameObject popupPrefab;
	}

	public List<PopupInfo> popupDatabase = new List<PopupInfo>();

	[NonSerialized]
	private List<GameObject> popups = new List<GameObject>();

	[NonSerialized]
	private GameObject internalCanvas;

	private GameObject canvas
	{
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}
			if (internalCanvas != null)
			{
				return internalCanvas;
			}
			internalCanvas = new GameObject("PopupCanvas");
			Canvas c = internalCanvas.AddComponent<Canvas>();
			internalCanvas.AddComponent<GraphicRaycaster>();
			c.renderMode = RenderMode.ScreenSpaceOverlay;
			c.sortingOrder = 2;
			UnityEngine.Object.DontDestroyOnLoad(internalCanvas.gameObject);
			internalCanvas.hideFlags = HideFlags.HideAndDontSave;
			return internalCanvas;
		}
	}

	private void OnEnable()
	{
		foreach (GameObject p in popups)
		{
			UnityEngine.Object.Destroy(p);
		}
		popups.Clear();
		if ((bool)internalCanvas)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(internalCanvas);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(internalCanvas);
			}
		}
	}

	public void OnDisable()
	{
		OnDestroy();
	}

	public void OnDestroy()
	{
		if (!Application.isPlaying)
		{
			foreach (GameObject p2 in popups)
			{
				UnityEngine.Object.DestroyImmediate(p2);
			}
			popups.Clear();
			if (internalCanvas != null)
			{
				UnityEngine.Object.DestroyImmediate(internalCanvas);
			}
			return;
		}
		foreach (GameObject p in popups)
		{
			UnityEngine.Object.Destroy(p);
		}
		popups.Clear();
		if (internalCanvas != null)
		{
			UnityEngine.Object.Destroy(internalCanvas);
		}
	}

	public void ClearAllPopups()
	{
		foreach (GameObject p in popups)
		{
			UnityEngine.Object.Destroy(p);
		}
		popups.Clear();
	}

	public void SpawnPopupBasic(string name)
	{
		SpawnPopup(name);
	}

	public bool PopupIsActive()
	{
		return popups.Count > 0;
	}

	public void ClearPopup(Popup p)
	{
		if (!(p == null))
		{
			popups.Remove(p.gameObject);
			UnityEngine.Object.Destroy(p.gameObject);
		}
	}

	public Popup SpawnPopup(string name, bool solo = true, string description = null, Sprite icon = null)
	{
		Popup popup = null;
		foreach (PopupInfo p2 in popupDatabase)
		{
			if (p2.name == name && canvas != null)
			{
				GameObject g = UnityEngine.Object.Instantiate(p2.popupPrefab, canvas.transform);
				popup = g.GetComponentInChildren<Popup>();
				if (popup == null)
				{
					Debug.LogError("Popup " + name + " doesn't have a popup component, that's required in order to set things like the text or image of the popup!");
				}
				break;
			}
		}
		if (popup != null)
		{
			if (description != null)
			{
				popup.description.text = description;
			}
			if (icon != null)
			{
				popup.icon.sprite = icon;
			}
			if (solo)
			{
				foreach (GameObject p in popups)
				{
					UnityEngine.Object.Destroy(p);
				}
				popups.Clear();
			}
			if (popup.cancel != null)
			{
				popup.cancel.onClick.AddListener(delegate
				{
					ClearPopup(popup);
				});
			}
			if (popup.okay != null)
			{
				popup.okay.onClick.AddListener(delegate
				{
					ClearPopup(popup);
				});
			}
			popups.Add(popup.gameObject);
		}
		return popup;
	}
}
