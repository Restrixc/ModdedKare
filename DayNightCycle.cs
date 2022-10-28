using System.Collections;
using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;

public class DayNightCycle : MonoBehaviourPun, IPunObservable, ISavable
{
	[SerializeField]
	private GameEventGeneric midnightEvent;

	[SerializeField]
	private GameEventFloat metabolizeEvent;

	private static DayNightCycle instance;

	private WaitForSeconds waitForTwoSeconds;

	private int daysPast = 0;

	public static void StaticSleep()
	{
		instance.Sleep();
	}

	private void Awake()
	{
		waitForTwoSeconds = new WaitForSeconds(2f);
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		StartCoroutine(MetabolizeOccassionally());
	}

	private IEnumerator MetabolizeOccassionally()
	{
		while (base.isActiveAndEnabled)
		{
			yield return waitForTwoSeconds;
			metabolizeEvent.Raise(2f);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(daysPast);
		}
		else
		{
			daysPast = (int)stream.ReceiveNext();
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(daysPast);
	}

	public void Load(BinaryReader reader)
	{
		daysPast = reader.ReadInt32();
	}

	private void Sleep()
	{
		midnightEvent.Raise(null);
		daysPast++;
	}
}
