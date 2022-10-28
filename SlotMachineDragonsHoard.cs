using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineDragonsHoard : MonoBehaviour
{
	[SerializeField]
	private Text prizeText;

	[SerializeField]
	private SlotMachineRow[] rows;

	[SerializeField]
	private Transform handle;

	[SerializeField]
	public ScriptableFloat money;

	private int prizeValue;

	private float timeLastPlayed;

	private float continuousSessionTimeout;

	private float attractModeTimeout;

	private bool resultsChecked = false;

	public AudioClip won;

	public AudioClip bigwin;

	public AudioClip failed;

	public AudioClip started;

	public AudioClip startedShort;

	public AudioClip[] attract;

	public AudioSource gameAud;

	public AudioSource attractAud;

	public GameObject LEDLights;

	private Material LEDLightsMat;

	public event Action HandlePulled = delegate
	{
	};

	private void Awake()
	{
		LEDLightsMat = LEDLights.GetComponent<MeshRenderer>().material;
		attractModeTimeout = 20 + UnityEngine.Random.Range(5, 12);
		continuousSessionTimeout = 8f;
		StartCoroutine(AttractSubsystem());
	}

	public void CheckForUpdate()
	{
		if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped && !resultsChecked)
		{
			CheckResults();
		}
	}

	public void RunMachine()
	{
		if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped)
		{
			Started();
			StartCoroutine("PullHandle");
		}
	}

	private IEnumerator PullHandle()
	{
		this.HandlePulled();
		yield return new WaitForSeconds(0.2f);
	}

	private void CheckResults()
	{
		if (rows[0].stoppedSlot == "Silver Coin" && rows[1].stoppedSlot == "Silver Coin" && rows[2].stoppedSlot == "Silver Coin")
		{
			prizeValue = 20;
			Win();
		}
		else if (rows[0].stoppedSlot == "Gold Bar" && rows[1].stoppedSlot == "Gold Bar" && rows[2].stoppedSlot == "Gold Bar")
		{
			prizeValue = 40;
			Win();
		}
		else if (rows[0].stoppedSlot == "Cup" && rows[1].stoppedSlot == "Cup" && rows[2].stoppedSlot == "Cup")
		{
			prizeValue = 100;
			Win();
		}
		else if (rows[0].stoppedSlot == "Ring" && rows[1].stoppedSlot == "Ring" && rows[2].stoppedSlot == "Ring")
		{
			prizeValue = 300;
			Win();
		}
		else if (rows[0].stoppedSlot == "Hoard" && rows[1].stoppedSlot == "Hoard" && rows[2].stoppedSlot == "Hoard")
		{
			BigWin();
			prizeValue = 500;
		}
		else if (rows[0].stoppedSlot == "Blue Stone" && rows[1].stoppedSlot == "Blue Stone" && rows[2].stoppedSlot == "Blue Stone")
		{
			BigWin();
			prizeValue = 1000;
		}
		else if (rows[0].stoppedSlot == "Scroll" && rows[1].stoppedSlot == "Scroll" && rows[2].stoppedSlot == "Scroll")
		{
			BigWin();
			prizeValue = 5000;
		}
		else if (rows[0].stoppedSlot == "Treasure Chest" && rows[1].stoppedSlot == "Treasure Chest" && rows[2].stoppedSlot == "Treasure Chest")
		{
			BigWin();
			prizeValue = 10000;
		}
		else if (rows[0].stoppedSlot == "Ring" || rows[1].stoppedSlot == "Ring" || rows[2].stoppedSlot == "Ring")
		{
			prizeValue += 5;
			Win();
		}
		else if ((rows[0].stoppedSlot == "Ring" && rows[1].stoppedSlot == "Ring") || (rows[1].stoppedSlot == "Ring" && rows[2].stoppedSlot == "Ring"))
		{
			prizeValue += 25;
			Win();
		}
		else if (rows[0].stoppedSlot == "Silver Coin" || rows[0].stoppedSlot == "Gold Bar" || rows[0].stoppedSlot == "Hoard")
		{
			if ((rows[1].stoppedSlot == "Silver Coin" || rows[1].stoppedSlot == "Gold Bar" || rows[1].stoppedSlot == "Hoard") && (rows[2].stoppedSlot == "Silver Coin" || rows[2].stoppedSlot == "Gold Bar" || rows[2].stoppedSlot == "Hoard"))
			{
				prizeValue += 50;
				Win();
			}
		}
		else
		{
			Failed();
		}
		money.give(prizeValue);
		resultsChecked = true;
	}

	private void Win()
	{
		prizeText.enabled = true;
		prizeText.text = " You Won: $" + prizeValue;
		gameAud.PlayOneShot(won);
	}

	private void BigWin()
	{
		prizeText.enabled = true;
		prizeText.text = " BIG WINNER! $" + prizeValue;
		gameAud.PlayOneShot(bigwin);
	}

	private void Failed()
	{
		prizeText.enabled = true;
		prizeText.text = "Try your luck again!";
		gameAud.PlayOneShot(failed);
	}

	private void Started()
	{
		attractAud.Stop();
		if (Time.realtimeSinceStartup > continuousSessionTimeout + timeLastPlayed)
		{
			gameAud.PlayOneShot(started);
		}
		else
		{
			gameAud.PlayOneShot(startedShort);
		}
		timeLastPlayed = Time.realtimeSinceStartup;
		prizeValue = 0;
		prizeText.enabled = false;
		resultsChecked = false;
		LEDLightsMat.SetVector("_EmissionColor", Color.yellow * 5f);
	}

	private IEnumerator AttractSubsystem()
	{
		while (true)
		{
			yield return new WaitForSeconds(attractModeTimeout);
			if (timeLastPlayed + attractModeTimeout < Time.realtimeSinceStartup)
			{
				attractAud.Stop();
				attractAud.clip = attract[UnityEngine.Random.Range(0, attract.Length - 1)];
				attractAud.Play();
				LEDLightsMat.SetVector("_EmissionColor", Color.yellow * ((float)UnityEngine.Random.Range(-1, 1) * 5f));
				prizeText.enabled = true;
				prizeText.text = "Only $1 to Play!";
			}
		}
	}
}
