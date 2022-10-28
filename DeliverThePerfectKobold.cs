using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class DeliverThePerfectKobold : ObjectiveWithSpaceBeam
{
	[SerializeField]
	private LocalizedString description;

	[SerializeField]
	private GameEventPhotonView soldGameObjectEvent;

	public override void Register()
	{
		base.Register();
		soldGameObjectEvent.AddListener(OnSoldObject);
	}

	public override void Unregister()
	{
		base.Unregister();
		soldGameObjectEvent.RemoveListener(OnSoldObject);
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnSoldObject(PhotonView view)
	{
		Kobold i = view.GetComponent<Kobold>();
		if (i == null)
		{
			return;
		}
		KoboldGenes genes = i.GetGenes();
		if (genes != null)
		{
			float sum = 0f;
			sum += genes.baseSize;
			sum += genes.fatSize;
			sum += genes.ballSize;
			sum += genes.bellySize;
			sum += genes.dickSize;
			sum += genes.dickThickness;
			sum += genes.maxEnergy;
			sum += genes.fatSize;
			if (sum > 210f)
			{
				Advance(spaceBeamTarget.position);
			}
		}
	}

	public override string GetTitle()
	{
		return title.GetLocalizedString() + " 0/1";
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
