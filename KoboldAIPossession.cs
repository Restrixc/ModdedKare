using System.Collections;
using Photon.Pun;
using UnityEngine;

public class KoboldAIPossession : MonoBehaviourPun
{
	private WaitForSeconds waitForSeconds;

	private Transform focus;

	private bool focusing = false;

	private CharacterControllerAnimator characterControllerAnimator;

	private static readonly Collider[] colliders = new Collider[32];

	[SerializeField]
	private Transform headTransform;

	private Ragdoller ragdoller;

	private Vector3 lerpDir;

	private Rigidbody body;

	private LayerMask lookAtMask;

	private void Awake()
	{
		ragdoller = GetComponentInParent<Ragdoller>();
		lerpDir = Vector3.forward;
		waitForSeconds = new WaitForSeconds(2f);
		characterControllerAnimator = GetComponentInParent<CharacterControllerAnimator>();
		body = GetComponentInParent<Rigidbody>();
	}

	private void Start()
	{
		lookAtMask = (int)GameManager.instance.usableHitMask | LayerMask.GetMask("Player");
		StartCoroutine(Think());
	}

	private void LateUpdate()
	{
		if (base.photonView.IsMine)
		{
			if (ragdoller != null && ragdoller.ragdolled)
			{
				characterControllerAnimator.SetEyeDir(headTransform.forward);
			}
			else if (!(focus == null) && !(headTransform == null))
			{
				Vector3 wantedDir = (focusing ? Vector3.Lerp((focus.position - headTransform.position).normalized, body.transform.forward, 0.6f) : body.transform.forward);
				lerpDir = Vector3.RotateTowards(lerpDir, wantedDir, Time.deltaTime * 30f, 0f);
				characterControllerAnimator.SetEyeDir(lerpDir);
			}
		}
	}

	private IEnumerator Think()
	{
		while (true)
		{
			yield return waitForSeconds;
			if (!base.photonView.IsMine || Random.Range(0f, 1f) > 0.5f)
			{
				continue;
			}
			if (focusing)
			{
				focusing = false;
				continue;
			}
			int hits = Physics.OverlapSphereNonAlloc(base.transform.position, 2f, colliders, lookAtMask);
			if (hits <= 0)
			{
				continue;
			}
			for (int i = 0; i < 4; i++)
			{
				Collider collider1 = colliders[Random.Range(0, hits)];
				if (!collider1.transform.IsChildOf(body.transform))
				{
					focus = collider1.transform;
					focusing = true;
				}
			}
		}
	}
}
