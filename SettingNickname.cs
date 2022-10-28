using Photon.Pun;
using UnityEngine;
using UnityScriptableSettings;


public class SettingNickname : SettingString
{
	public override void SetValue(string value)
	{
		PhotonNetwork.NickName = value;
		base.SetValue(value);
	}
}
