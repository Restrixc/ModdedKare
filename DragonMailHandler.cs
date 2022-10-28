using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragonMailHandler : MonoBehaviour, IPunObservable
{
	public GameObject selectOnStart;

	public ScriptableFloat dragonMoneyGoal;

	public GameObject viewMain;

	public GameObject viewMoneyDonate;

	public GameObject viewMoneyConfirm;

	public GameObject viewKoboldSend;

	public GameObject viewKoboldRetrieve;

	public GameObject viewKoboldReceipt;

	public Slider mainviewMoneySlider;

	public Slider donationSlider;

	public TextMeshProUGUI moneyLabel;

	public TextMeshProUGUI moneyValue;

	public GameObject koboldBoxPrefab;

	public GameObject koboldBoxInstance;

	public Transform boxSpawnPoint;

	private PhotonView pView;

	public bool dmActive;

	public static DragonMailHandler inst;

	public Canvas dmMainCanvas;

	private void Start()
	{
		pView = GetComponent<PhotonView>();
		RefreshMoneyGoal();
		inst = this;
	}

	public void RefreshMoneyGoal()
	{
		moneyValue.text = string.Format("{0}/{1} {2}", dragonMoneyGoal.value.ToString(), dragonMoneyGoal.max.ToString(), (dragonMoneyGoal.value / dragonMoneyGoal.max).ToString("P"));
		mainviewMoneySlider.maxValue = dragonMoneyGoal.max;
		mainviewMoneySlider.value = dragonMoneyGoal.value;
	}

	public void SwitchToMain()
	{
		Debug.Log("switching to Main");
		EventSystem.current.SetSelectedGameObject(selectOnStart);
		TurnOn(viewMain);
		RefreshMoneyGoal();
		dmActive = true;
	}

	public void SwitchToMoneyDonate()
	{
		Debug.Log("switching to viewMoneyDonate");
		TurnOn(viewMoneyDonate);
	}

	public void SwitchToMoneyConfirm()
	{
		TurnOn(viewMoneyConfirm);
	}

	public void SwitchToKoboldDonate()
	{
		Debug.Log("switching to KoboldDonate");
		if (koboldBoxInstance == null)
		{
			TurnOn(viewKoboldSend);
		}
		else
		{
			TurnOn(viewKoboldRetrieve);
		}
	}

	public void SwitchToKoboldReceipt()
	{
		TurnOn(viewKoboldReceipt);
	}

	private void TurnOffAll()
	{
		Debug.Log("Turning off all");
		TurnOff(viewMain);
		TurnOff(viewMoneyDonate);
		TurnOff(viewMoneyConfirm);
		TurnOff(viewKoboldReceipt);
		TurnOff(viewKoboldRetrieve);
		TurnOff(viewKoboldSend);
	}

	private void TurnOff(GameObject go)
	{
		Debug.Log("turned off" + go.name + " with GUID " + go.GetInstanceID());
		go.GetComponent<Animator>().SetBool("Open", value: false);
		go.GetComponent<CanvasGroup>().interactable = false;
		go.GetComponent<CanvasGroup>().blocksRaycasts = false;
	}

	private void TurnOn(GameObject go)
	{
		TurnOffAll();
		Debug.Log("turning on" + go.name + " with GUID " + go.GetInstanceID());
		go.GetComponent<Animator>().SetBool("Open", value: true);
		go.GetComponent<CanvasGroup>().interactable = true;
		go.GetComponent<CanvasGroup>().blocksRaycasts = true;
	}

	public void AddMoneyToGoal()
	{
		dragonMoneyGoal.give(donationSlider.value);
		if (dragonMoneyGoal.value >= dragonMoneyGoal.max)
		{
		}
		RefreshMoneyGoal();
	}

	public void Close()
	{
		dmActive = false;
		dmMainCanvas.GetComponent<Animator>().SetBool("Open", dmActive);
		dmMainCanvas.GetComponent<CanvasGroup>().blocksRaycasts = dmActive;
		dmMainCanvas.GetComponent<CanvasGroup>().interactable = dmActive;
		Cursor.lockState = CursorLockMode.None;
		dmMainCanvas.enabled = dmActive;
	}

	public void Open()
	{
		dmActive = true;
		dmMainCanvas.GetComponent<Animator>().SetBool("Open", dmActive);
		dmMainCanvas.GetComponent<CanvasGroup>().blocksRaycasts = dmActive;
		dmMainCanvas.GetComponent<CanvasGroup>().interactable = dmActive;
		Cursor.lockState = CursorLockMode.None;
		dmMainCanvas.enabled = dmActive;
	}

	public void Toggle()
	{
		if (dmActive)
		{
			Close();
		}
		else if (!dmActive)
		{
			Open();
			SwitchToMain();
		}
	}

	public void SendDonationBox()
	{
		if (koboldBoxInstance != null)
		{
			PhotonNetwork.Destroy(koboldBoxInstance.GetComponent<PhotonView>());
		}
		koboldBoxInstance = PhotonNetwork.Instantiate("DonationBox", boxSpawnPoint.position, Quaternion.identity, 0);
	}

	public void RetrieveDonationBox()
	{
		if (koboldBoxInstance != null)
		{
			PhotonNetwork.Destroy(koboldBoxInstance);
		}
		else
		{
			Debug.LogWarning("[DragonMailHandler] :: Attempted to destroy SendBox which did not exist");
		}
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
}
