using PenetrationTech;
using UnityEngine;

public class KoboldJawOpenListener : PenetrableListener
{
	[SerializeField]
	private Transform jawTransform;

	[SerializeField]
	private Vector3 jawOpenDirection;

	[SerializeField]
	private Animator koboldAnimator;

	[SerializeField]
	private CharacterControllerAnimator controllerAnimator;

	private static readonly int LookUp = Animator.StringToHash("LookUp");

	private Vector3 startingLocalPosition;

	private bool lastWork;

	private float jawVelocity;

	private float jawMoveAmount;

	private float girthRadiusMemory;

	private bool zeroOutNow;

	protected override void OnPenetrationDepthChange(float newDepth)
	{
		base.OnPenetrationDepthChange(newDepth);
		bool work = newDepth > 0f;
		if (work != lastWork)
		{
			koboldAnimator.SetBool(LookUp, work);
			controllerAnimator.SetLookEnabled(!work);
			lastWork = work;
		}
	}

	public override void Update()
	{
		base.Update();
		jawMoveAmount = Mathf.SmoothDamp(jawMoveAmount, girthRadiusMemory, ref jawVelocity, 0.25f);
		if (zeroOutNow && jawMoveAmount != 0f)
		{
			jawTransform.localPosition = startingLocalPosition + jawTransform.parent.InverseTransformDirection(jawTransform.TransformDirection(jawOpenDirection)) * jawMoveAmount * 0.5f;
		}
	}

	protected override void OnPenetrationGirthRadiusChange(float newGirthRadius)
	{
		base.OnPenetrationGirthRadiusChange(newGirthRadius);
		girthRadiusMemory = newGirthRadius;
		if (newGirthRadius != 0f)
		{
			zeroOutNow = false;
			jawTransform.localPosition = startingLocalPosition + jawTransform.parent.InverseTransformDirection(jawTransform.TransformDirection(jawOpenDirection)) * jawMoveAmount * 0.5f;
		}
		else
		{
			zeroOutNow = true;
		}
	}

	public override void OnEnable(Penetrable p)
	{
		base.OnEnable(p);
		startingLocalPosition = jawTransform.localPosition;
	}

	public override void AssertValid()
	{
		base.AssertValid();
		if (jawTransform == null)
		{
			throw new PenetrableListenerValidationException($"jawTransform is null on {this}");
		}
		if (koboldAnimator == null)
		{
			throw new PenetrableListenerValidationException($"KoboldAnimator is null on {this}");
		}
		if (controllerAnimator == null)
		{
			throw new PenetrableListenerValidationException($"controllerAnimator is null on {this}");
		}
	}

	public override void OnDrawGizmosSelected(Penetrable p)
	{
		base.OnDrawGizmosSelected(p);
	}

	public override void NotifyPenetration(Penetrable penetrable, Penetrator penetrator, float worldSpaceDistanceToPenisRoot, Penetrable.SetClipDistanceAction clipAction)
	{
		NotifyPenetrationGDO(penetrable, penetrator, worldSpaceDistanceToPenisRoot, clipAction, PenData.Girth | PenData.Depth);
	}
}
