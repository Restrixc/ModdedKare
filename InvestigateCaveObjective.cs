using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class InvestigateCaveObjective : DragonMailObjective
{
	private class KoboldDetector : MonoBehaviour
	{
		public delegate void KoboldEnteredZoneAction(Kobold k);

		public static KoboldEnteredZoneAction entered;

		private void OnTriggerEnter(Collider other)
		{
			if (other.GetComponentInParent<Kobold>() != null)
			{
				entered?.Invoke(other.GetComponentInParent<Kobold>());
			}
		}
	}

	[SerializeField]
	private Collider caveArea;

	private KoboldDetector detector;

	[SerializeField]
	private LocalizedString description;

	public override void Register()
	{
		base.Register();
		caveArea.isTrigger = true;
		if ((object)detector == null)
		{
			detector = caveArea.gameObject.AddComponent<KoboldDetector>();
		}
		KoboldDetector.entered = (KoboldDetector.KoboldEnteredZoneAction)Delegate.Combine(KoboldDetector.entered, new KoboldDetector.KoboldEnteredZoneAction(OnKoboldEnterZone));
	}

	public override void Unregister()
	{
		base.Unregister();
		KoboldDetector.entered = (KoboldDetector.KoboldEnteredZoneAction)Delegate.Remove(KoboldDetector.entered, new KoboldDetector.KoboldEnteredZoneAction(OnKoboldEnterZone));
		if (detector != null)
		{
			UnityEngine.Object.Destroy(detector);
		}
	}

	private void OnKoboldEnterZone(Kobold k)
	{
		TriggerComplete();
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
