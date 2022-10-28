using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PhotonRegionHelper : MonoBehaviourPunCallbacks
{
	public TMP_Dropdown dropdown;

	private RegionHandler cachedHandler;

	private Region selectedRegion;

	public void ChooseRegion(int id)
	{
		SingletonScriptableObject<NetworkManager>.instance.JoinLobby(dropdown.options[id].text);
	}

	public override void OnRegionListReceived(RegionHandler handler)
	{
		Debug.Log("[Photon Region Handler] :: Currently connected region: " + PhotonNetwork.CloudRegion);
		dropdown.ClearOptions();
		cachedHandler = handler;
		List<TMP_Dropdown.OptionData> returnedRegions = new List<TMP_Dropdown.OptionData>();
		foreach (Region item in cachedHandler.EnabledRegions)
		{
			returnedRegions.Add(new TMP_Dropdown.OptionData(item.Code));
		}
		dropdown.AddOptions(returnedRegions);
		dropdown.onValueChanged.RemoveListener(ChooseRegion);
		dropdown.onValueChanged.AddListener(ChooseRegion);
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		Debug.Log("[Photon Region Handler] :: Connected to master");
		foreach (TMP_Dropdown.OptionData item in dropdown.options)
		{
			if (PhotonNetwork.CloudRegion == item.text)
			{
				dropdown.SetValueWithoutNotify(dropdown.options.IndexOf(item));
			}
		}
	}
}
