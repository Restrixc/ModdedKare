using System.Collections;
using UnityEngine;

public class SlotMachineRow : MonoBehaviour
{
	private float timeInterval;

	public bool rowStopped;

	public string stoppedSlot;

	public GameObject strip;

	public SlotMachineDragonsHoard myMachine;

	public AudioClip tickNoise;

	public AudioClip slotLocked;

	private AudioSource myAud;

	private float stripLength = 30.625f;

	private float movementLength = 0.64f;

	private float slotOffset = 0f;

	private float randomVal = 0f;

	private void Start()
	{
		rowStopped = true;
		myMachine.HandlePulled += StartRotating;
		myAud = base.gameObject.GetComponent<AudioSource>();
	}

	private void StartRotating()
	{
		stoppedSlot = "";
		StartCoroutine("Rotate");
	}

	private void Click()
	{
		myAud.PlayOneShot(tickNoise);
	}

	private void Clunk()
	{
		myAud.PlayOneShot(slotLocked);
	}

	private IEnumerator Rotate()
	{
		rowStopped = false;
		timeInterval = 0.025f;
		Random.InitState(Random.Range(0, 9999));
		randomVal = movementLength * (float)Mathf.RoundToInt(Random.Range(80, 114));
		for (int i = 0; (float)i < randomVal; i++)
		{
			if (strip.transform.localPosition.y <= 0f - stripLength / 2f)
			{
				strip.transform.localPosition = new Vector3(0f, stripLength / 2f, 0f);
			}
			strip.transform.localPosition = new Vector3(0f, slotOffset + (strip.transform.localPosition.y - movementLength), 0f);
			if (i > Mathf.RoundToInt(randomVal * 0.5f))
			{
				timeInterval = 0.05f;
			}
			if (i > Mathf.RoundToInt(randomVal * 0.7f))
			{
				timeInterval = 0.08f;
			}
			if (i > Mathf.RoundToInt(randomVal * 0.95f))
			{
				timeInterval = 0.16f;
			}
			if (i > Mathf.RoundToInt(randomVal * 0.98f))
			{
				timeInterval = 0.24f;
			}
			Click();
			yield return new WaitForSeconds(timeInterval);
		}
		float currentYPos = strip.transform.localPosition.y;
		stoppedSlot = "Empty";
		if (FloatCompare(currentYPos, -15.40751f))
		{
			stoppedSlot = "Gold Bar";
		}
		if (FloatCompare(currentYPos, -12.8475f))
		{
			stoppedSlot = "Cup";
		}
		if (FloatCompare(currentYPos, -10.2875f))
		{
			stoppedSlot = "Silver Coin";
		}
		if (FloatCompare(currentYPos, -7.727501f))
		{
			stoppedSlot = "Hoard";
		}
		if (FloatCompare(currentYPos, -5.167502f))
		{
			stoppedSlot = "Gold Bar";
		}
		if (FloatCompare(currentYPos, -2.607502f))
		{
			stoppedSlot = "Blue Stone";
		}
		if (FloatCompare(currentYPos, 0.0475f))
		{
			stoppedSlot = "Silver Coin";
		}
		if (FloatCompare(currentYPos, 2.512497f))
		{
			stoppedSlot = "Ring";
		}
		if (FloatCompare(currentYPos, 5.0725f))
		{
			stoppedSlot = "Cup";
		}
		if (FloatCompare(currentYPos, 7.632496f))
		{
			stoppedSlot = "Scroll";
		}
		if (FloatCompare(currentYPos, 10.1925f))
		{
			stoppedSlot = "Treasure Chest";
		}
		if (FloatCompare(currentYPos, 12.7525f))
		{
			stoppedSlot = "Ring";
		}
		if (FloatCompare(currentYPos, 15.3125f))
		{
			stoppedSlot = "Gold Bar";
		}
		rowStopped = true;
		Clunk();
		myMachine.CheckForUpdate();
		yield return new WaitForSeconds(1f);
	}

	private void OnDestroy()
	{
		myMachine.HandlePulled -= StartRotating;
	}

	private bool FloatCompare(float a, float b)
	{
		if (Mathf.Approximately(a, b))
		{
			return true;
		}
		return false;
	}
}
