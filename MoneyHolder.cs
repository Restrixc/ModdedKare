using System.IO;
using Photon.Pun;

public class MoneyHolder : MonoBehaviourPun, ISavable, IPunObservable, IValuedGood
{
	public delegate void MoneyChangedAction(float newMoney);

	public MoneyChangedAction moneyChanged;

	private float money = 5f;

	public float GetMoney()
	{
		return money;
	}

	public void SetMoney(float amount)
	{
		money = amount;
	}

	[PunRPC]
	public void AddMoney(float add)
	{
		if (!(add <= 0f))
		{
			money += add;
			moneyChanged(money);
		}
	}

	public bool ChargeMoney(float amount)
	{
		if (money < amount)
		{
			return false;
		}
		money -= amount;
		moneyChanged(money);
		return true;
	}

	public bool HasMoney(float amount)
	{
		return money >= amount;
	}

	public void Load(BinaryReader reader)
	{
		money = reader.ReadSingle();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(money);
		}
		else
		{
			money = (float)stream.ReceiveNext();
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(money);
	}

	public float GetWorth()
	{
		return money;
	}
}
