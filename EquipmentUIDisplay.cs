using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class EquipmentUIDisplay : MonoBehaviourPun
{
	[Serializable]
	public class EquipmentSlotDisplay
	{
		public Image targetImage;

		public Sprite defaultSprite;

		public Image containerImage;

		public Equipment.EquipmentSlot slot;

		[HideInInspector]
		public Equipment equipped;
	}

	public Transform targetDisplay;

	public LocalizedString noneString;

	public LocalizeStringEvent detailDescription;

	public LocalizeStringEvent detailTitle;

	public Sprite noneSprite;

	public Image detailedDisplay;

	public KoboldInventory inventory;

	public GameObject inventoryUIPrefab;

	public List<EquipmentSlotDisplay> slots = new List<EquipmentSlotDisplay>();

	private List<GameObject> spawnedUI = new List<GameObject>();

	private void Awake()
	{
		KoboldInventory koboldInventory = inventory;
		koboldInventory.equipmentChanged = (KoboldInventory.EquipmentChangedEvent)Delegate.Combine(koboldInventory.equipmentChanged, new KoboldInventory.EquipmentChangedEvent(UpdateDisplay));
		foreach (EquipmentSlotDisplay slot in slots)
		{
			EventTrigger et = slot.targetImage.gameObject.AddComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerClick;
			entry.callback.AddListener(delegate
			{
				DisplayDetail(slot.equipped);
			});
			et.triggers.Add(entry);
		}
	}

	private void OnEnable()
	{
		UpdateDisplay(inventory.GetAllEquipment());
	}

	private void OnDestroy()
	{
		if (inventory != null)
		{
			KoboldInventory koboldInventory = inventory;
			koboldInventory.equipmentChanged = (KoboldInventory.EquipmentChangedEvent)Delegate.Remove(koboldInventory.equipmentChanged, new KoboldInventory.EquipmentChangedEvent(UpdateDisplay));
		}
	}

	public void DisplayDetail(Equipment e)
	{
		if (e == null)
		{
			detailedDisplay.sprite = noneSprite;
			detailDescription.StringReference = noneString;
			detailTitle.StringReference = noneString;
		}
		else
		{
			detailedDisplay.sprite = e.sprite;
			detailDescription.StringReference = e.localizedDescription;
			detailTitle.StringReference = e.localizedName;
		}
	}

	public void UpdateDisplay(List<Equipment> equipment)
	{
		foreach (GameObject g in spawnedUI)
		{
			UnityEngine.Object.Destroy(g);
		}
		foreach (EquipmentSlotDisplay slot2 in slots)
		{
			slot2.containerImage.color = Color.white;
			slot2.targetImage.sprite = slot2.defaultSprite;
			slot2.targetImage.color = new Color(0.5f, 0.5f, 0.8f, 0.25f);
			slot2.equipped = null;
		}
		foreach (Equipment e in equipment)
		{
			foreach (EquipmentSlotDisplay slot in slots)
			{
				if (slot.slot == e.slot)
				{
					slot.containerImage.color = Color.yellow;
					slot.equipped = e;
					slot.targetImage.sprite = e.sprite;
					slot.targetImage.color = Color.white;
				}
			}
			GameObject ui = UnityEngine.Object.Instantiate(inventoryUIPrefab, targetDisplay);
			ui.transform.Find("Label").GetComponent<LocalizeStringEvent>().StringReference = e.localizedName;
			Button DropButton = ui.transform.Find("DropButton").GetComponent<Button>();
			DropButton.interactable = e.canManuallyUnequip;
			DropButton.onClick.AddListener(delegate
			{
				if (inventory.photonView.IsMine)
				{
					inventory.RemoveEquipment(e, dropOnGround: true);
				}
				DisplayDetail(null);
			});
			ui.transform.Find("InspectButton").GetComponent<Button>().onClick.AddListener(delegate
			{
				DisplayDetail(e);
			});
			ui.transform.Find("Icon").GetComponent<Image>().sprite = e.sprite;
			EventTrigger et = ui.transform.Find("Icon").gameObject.AddComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerClick;
			entry.callback.AddListener(delegate
			{
				DisplayDetail(e);
			});
			et.triggers.Add(entry);
			spawnedUI.Add(ui);
		}
		if (spawnedUI.Count <= 0 || spawnedUI[0] == null)
		{
			return;
		}
		Selectable[] componentsInChildren = spawnedUI[0].GetComponentsInChildren<Selectable>();
		foreach (Selectable selectable in componentsInChildren)
		{
			if (!(selectable == null) && selectable.IsInteractable())
			{
				selectable.Select();
				break;
			}
		}
	}
}
