using System;
using System.Collections;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DragonMailObjective : ISavable, IPunObservable
{
	public delegate void ObjectiveAction(DragonMailObjective obj);

	public bool autoAdvance;

	[SerializeField]
	protected LocalizedString title;

	[SerializeField]
	protected PhotonGameObjectReference starExplosion;

	public event ObjectiveAction completed;

	public event ObjectiveAction updated;

	public virtual string GetTitle()
	{
		return title.GetLocalizedString();
	}

	protected void TriggerComplete()
	{
		TriggerUpdate();
		this.completed?.Invoke(this);
	}

	protected void TriggerUpdate()
	{
		this.updated?.Invoke(this);
	}

	protected virtual void Advance(Vector3 position)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			GameObject obj = PhotonNetwork.Instantiate(starExplosion.photonName, position, Quaternion.identity, 0);
			obj.GetPhotonView().StartCoroutine(DestroyAfterTime(obj));
		}
	}

	private IEnumerator DestroyAfterTime(GameObject obj)
	{
		yield return new WaitForSeconds(5f);
		PhotonNetwork.Destroy(obj);
	}

	public virtual void Register()
	{
	}

	public virtual void Unregister()
	{
	}

	public virtual string GetTextBody()
	{
		return "";
	}

	public virtual void Save(BinaryWriter writer)
	{
	}

	public virtual void Load(BinaryReader reader)
	{
	}

	public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public virtual void OnValidate()
	{
		starExplosion.OnValidate();
	}
}
