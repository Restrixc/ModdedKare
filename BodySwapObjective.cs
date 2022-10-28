using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Localization;

public class BodySwapObjective : ObjectiveWithSpaceBeam
{
	[SerializeField]
	private LocalizedString description;

	public override void Register()
	{
		base.Register();
		BrainSwapperMachine.bodySwapped += OnBodySwap;
	}

	public override void Unregister()
	{
		base.Unregister();
		BrainSwapperMachine.bodySwapped -= OnBodySwap;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnBodySwap(Kobold a, Kobold b)
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (!((Kobold)player.TagObject != a) || !((Kobold)player.TagObject != b))
			{
				if (a != null)
				{
					Advance(a.transform.position);
				}
				else if (b != null)
				{
					Advance(b.transform.position);
				}
			}
		}
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}

	public override string GetTitle()
	{
		return title.GetLocalizedString() + " 0/1";
	}
}
