using System.Collections;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;
using Vilar.IK;

public class CharacterControllerAnimator : MonoBehaviourPun, IPunObservable, ISavable
{
	private Kobold kobold;

	private ClassicIK solver;

	private IAnimationStationSet currentStationSet;

	private AnimationStation currentStation;

	private Animator playerModel;

	private KoboldCharacterController controller;

	private bool isInAir;

	[SerializeField]
	private Transform leftKneeHint;

	[SerializeField]
	private Transform rightKneeHint;

	private bool jumped;

	private Vector3 tempDir;

	[SerializeField]
	private VisualEffect jumpDust;

	[SerializeField]
	private VisualEffect walkDust;

	[SerializeField]
	private Transform headTransform;

	[SerializeField]
	private AudioPack footstepPack;

	private Vector2 eyeRot;

	private float speedLerp;

	private Vector2 networkedEyeRot;

	private float networkedAngle;

	private Vector2 hipVectorVelocity;

	private Vector2 hipVector;

	private Vector2 desiredHipVector;

	private bool lookEnabled = true;

	[SerializeField]
	private Rigidbody body;

	[SerializeField]
	private PlayerPossession playerPossession;

	[SerializeField]
	private float crouchedAnimationSpeedMultiplier = 1f;

	[SerializeField]
	private float walkingAnimationSpeedMultiplier = 1f;

	[SerializeField]
	private float standingAnimationSpeedMultiplier = 1f;

	private float crouchLerper;

	private LookAtHandler handler;

	private Vector3 lastPosition;

	private bool animating;

	private static readonly int PenetrationSize = Animator.StringToHash("PenetrationSize");

	private static readonly int SexFace = Animator.StringToHash("SexFace");

	private static readonly int Orgasm = Animator.StringToHash("Orgasm");

	private static readonly int MadHappy = Animator.StringToHash("MadHappy");

	private static readonly int MoveX = Animator.StringToHash("MoveX");

	private static readonly int MoveY = Animator.StringToHash("MoveY");

	private static readonly int ThrustX = Animator.StringToHash("ThrustX");

	private static readonly int ThrustY = Animator.StringToHash("ThrustY");

	private static readonly int Speed = Animator.StringToHash("Speed");

	private static readonly int Jump = Animator.StringToHash("Jump");

	private static readonly int Grounded = Animator.StringToHash("Grounded");

	private static readonly int CrouchAmount = Animator.StringToHash("CrouchAmount");

	private float randomSample => 1f + Mathf.SmoothStep(0f, 1f, Mathf.PerlinNoise(0f, Time.timeSinceLevelLoad * 0.08f)) * 2f;

	private Vector3 eyeDir => Quaternion.Euler(0f - eyeRot.y, eyeRot.x, 0f) * Vector3.forward;

	private Vector3 networkedEyeDir => Quaternion.Euler(0f - eyeRot.y, eyeRot.x, 0f) * Vector3.forward;

	public void SetLookEnabled(bool lookEnabled)
	{
		this.lookEnabled = lookEnabled;
	}

	private IEnumerator SetLookRoutine()
	{
		float startTime = Time.time;
		float duration = 1f;
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			handler.SetWeight(Mathf.Lerp(handler.GetWeight(), lookEnabled ? 0.7f : 0f, t));
			yield return null;
		}
	}

	public void SetEyeRot(Vector2 newEyeRot)
	{
		eyeRot = newEyeRot;
	}

	public void SetEyeDir(Vector3 newEyeDir)
	{
		Vector3 euler = Quaternion.LookRotation(newEyeDir).eulerAngles;
		eyeRot = new Vector2(euler.y, 0f - euler.x);
	}

	public bool TryGetAnimationStationSet(out IAnimationStationSet set)
	{
		if (!animating)
		{
			set = null;
			return false;
		}
		set = currentStationSet;
		return true;
	}

	private void Awake()
	{
		kobold = GetComponentInParent<Kobold>();
		solver = GetComponentInChildren<ClassicIK>();
		playerModel = GetComponentInChildren<Animator>();
		handler = playerModel.gameObject.AddComponent<LookAtHandler>();
		controller = GetComponentInParent<KoboldCharacterController>();
		playerModel.gameObject.AddComponent<AnimatorExtender>();
		FootIK ik = playerModel.gameObject.AddComponent<FootIK>();
		ik.leftKneeHint = leftKneeHint;
		ik.rightKneeHint = rightKneeHint;
		playerModel.gameObject.AddComponent<HandIK>();
		playerModel.gameObject.AddComponent<FootstepSoundManager>().SetFootstepPack(footstepPack);
		playerModel.GetBoneTransform(HumanBodyBones.LeftFoot).gameObject.AddComponent<FootInteractor>();
		playerModel.GetBoneTransform(HumanBodyBones.RightFoot).gameObject.AddComponent<FootInteractor>();
	}

	[PunRPC]
	public void BeginAnimationRPC(int photonViewID, int animatorID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(photonViewID);
		IAnimationStationSet set = view.GetComponentInChildren<IAnimationStationSet>();
		BeginAnimation(set, set.GetAnimationStations()[animatorID]);
	}

	private void BeginAnimation(IAnimationStationSet set, AnimationStation station)
	{
		StopAnimation();
		StopAllCoroutines();
		kobold.GetComponent<Ragdoller>().SetLocked(newLockState: true);
		currentStationSet = set;
		currentStation = station;
		if (station.info.user != null)
		{
			station.info.user.GetComponent<CharacterControllerAnimator>().StopAnimation();
		}
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(AnimationRoutine());
		}
		else
		{
			GameManager.instance.StartCoroutine(AnimationRoutine());
		}
	}

	private void Start()
	{
		tempDir = Vector3.forward;
		handler.SetWeight(0.7f);
		lookEnabled = true;
	}

	private IEnumerator AnimationRoutine()
	{
		animating = true;
		solver.enabled = true;
		controller.enabled = false;
		kobold.body.isKinematic = true;
		solver.Initialize();
		currentStation.SetProgress(0f);
		currentStation.OnStartAnimation(kobold);
		float startTime = Time.time;
		float blendDuration = 1f;
		Quaternion startRotation = kobold.body.rotation;
		while (Time.time < startTime + blendDuration)
		{
			float t = (Time.time - startTime) / blendDuration;
			solver.ForceBlend(t);
			kobold.body.rotation = Quaternion.Lerp(startRotation, currentStation.transform.rotation, t);
			yield return null;
		}
		solver.ForceBlend(1f);
		yield return new WaitForSeconds(3f);
		float transitionDuration = 3f;
		float startTransitionTime = Time.time;
		while (Time.time < startTransitionTime + transitionDuration)
		{
			float t2 = (Time.time - startTransitionTime) / transitionDuration;
			currentStation.SetProgress(Mathf.Lerp(currentStation.progress, randomSample, t2));
			yield return null;
		}
		while (animating)
		{
			currentStation.SetProgress(randomSample);
			yield return null;
		}
		StopAnimation();
	}

	public bool IsAnimating()
	{
		return animating;
	}

	public void SetHipVector(Vector2 newHipVector)
	{
		desiredHipVector = Vector2.ClampMagnitude(newHipVector, 1f);
	}

	public Vector2 GetHipVector()
	{
		return desiredHipVector;
	}

	private void Update()
	{
		if (!base.photonView.IsMine)
		{
			eyeRot = Vector2.MoveTowards(eyeRot, networkedEyeRot, networkedAngle * Time.deltaTime * (float)PhotonNetwork.SerializationRate);
		}
		hipVector = Vector2.SmoothDamp(hipVector, desiredHipVector, ref hipVectorVelocity, 0.05f);
		if (kobold != null)
		{
			playerModel.SetFloat(ThrustX, hipVector.x);
			playerModel.SetFloat(ThrustY, hipVector.y);
			float maxPen = 0f;
			playerModel.SetFloat(PenetrationSize, Mathf.Clamp01(maxPen * 4f));
			if (maxPen > 0f)
			{
				playerModel.SetFloat(SexFace, Mathf.Lerp(playerModel.GetFloat(SexFace), 1f, Time.deltaTime * 2f));
			}
			else
			{
				playerModel.SetFloat(SexFace, Mathf.Lerp(playerModel.GetFloat(SexFace), 0f, Time.deltaTime));
			}
			foreach (DickInfo.DickSet dickSet in kobold.activeDicks)
			{
				if (dickSet.dick.TryGetPenetrable(out var _))
				{
					playerModel.SetFloat(SexFace, 1f);
				}
			}
			playerModel.SetFloat(Orgasm, Mathf.Clamp01(Mathf.Abs(kobold.stimulation / kobold.stimulationMax)));
			playerModel.SetFloat(MadHappy, Mathf.Clamp01(Mathf.Abs(kobold.stimulation / kobold.stimulationMax)));
		}
		if (animating)
		{
			currentStation.SetCharacter(solver);
		}
		if (!controller.grounded)
		{
			isInAir = true;
		}
		else if (isInAir)
		{
			jumpDust.SendEvent("TriggerPoof");
			isInAir = false;
		}
		if (jumped != controller.jumped && controller.jumped)
		{
			jumped = controller.jumped;
			jumpDust.SendEvent("TriggerPoof");
		}
		if (jumped != controller.jumped && !controller.jumped)
		{
			jumped = controller.jumped;
		}
		Vector3 velocity = (base.transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 1E-06f);
		lastPosition = base.transform.position;
		Vector3 dir = Vector3.Normalize(velocity);
		Vector3 original = playerModel.transform.InverseTransformDirection(dir);
		float? y = 0f;
		dir = original.With(null, y).normalized;
		Vector3 original2 = velocity;
		y = 0f;
		float speedTarget = original2.With(null, y).magnitude;
		speedTarget *= Mathf.Lerp(standingAnimationSpeedMultiplier, crouchedAnimationSpeedMultiplier, controller.GetInputCrouched());
		if (controller.inputWalking)
		{
			speedTarget *= walkingAnimationSpeedMultiplier;
		}
		speedLerp = Mathf.MoveTowards(speedLerp, speedTarget, Time.deltaTime * 10f);
		float speed = speedLerp;
		tempDir = Vector3.RotateTowards(tempDir, dir, Time.deltaTime * 10f, 0f);
		playerModel.SetFloat(MoveX, tempDir.x);
		playerModel.SetFloat(MoveY, tempDir.z);
		speed /= Mathf.Lerp(base.transform.lossyScale.x, 1f, 0.5f);
		playerModel.SetFloat(Speed, speed);
		if (walkDust.HasFloat("Speed"))
		{
			if (controller.enabled)
			{
				walkDust.SetFloat("Speed", velocity.magnitude * (controller.grounded ? 1f : 0f));
			}
			else
			{
				walkDust.SetFloat("Speed", 0f);
			}
		}
		playerModel.SetBool(Jump, controller.jumped);
		playerModel.SetBool(Grounded, controller.grounded);
		crouchLerper = Mathf.MoveTowards(crouchLerper, controller.crouchAmount, 3f * Time.deltaTime);
		playerModel.SetFloat(CrouchAmount, crouchLerper);
		Vector3 lookPos = headTransform.position + eyeDir;
		handler.SetWeight(Mathf.MoveTowards(handler.GetWeight(), lookEnabled ? 0.7f : 0.4f, Time.deltaTime));
		if (animating)
		{
			currentStation.SetLookAtPosition(lookPos);
			currentStation.SetHipOffset(hipVector);
			handler.SetLookAtWeight(handler.GetWeight(), 0f, 0.8f, 1f, 0.45f);
		}
		else
		{
			handler.SetLookAtWeight(handler.GetWeight(), 0.3f, 0.8f, 1f, 0.45f);
		}
		handler.SetLookAtPosition(lookPos);
	}

	[PunRPC]
	public void StopAnimationRPC()
	{
		StopAnimation();
	}

	private void StopAnimation()
	{
		if (animating)
		{
			StopAllCoroutines();
			StartCoroutine(StopAnimationRoutine());
			animating = false;
			if (currentStation != null && currentStation.info.user == kobold)
			{
				currentStation.info.user = null;
			}
			currentStation = null;
			currentStationSet = null;
		}
	}

	private IEnumerator StopAnimationRoutine()
	{
		float duration = 1f;
		float startTime = Time.time;
		Quaternion startRotation = kobold.body.rotation;
		Quaternion endRotation = Quaternion.Euler(0f, eyeRot.x, 0f);
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			solver.ForceBlend(1f - t);
			kobold.body.rotation = Quaternion.Lerp(startRotation, endRotation, t);
			yield return null;
		}
		solver.ForceBlend(0f);
		solver.enabled = false;
		controller.enabled = true;
		kobold.body.isKinematic = false;
		kobold.GetComponent<Ragdoller>().SetLocked(newLockState: false);
		solver.CleanUp();
	}

	private void FixedUpdate()
	{
		if (!(playerPossession == null))
		{
			Quaternion characterRot = Quaternion.Euler(0f, eyeRot.x, 0f);
			Vector3 fdir = characterRot * Vector3.forward;
			float deflectionForgivenessDegrees = 20f;
			Vector3 forward = body.transform.forward;
			Vector3 cross = Vector3.Cross(forward, fdir);
			float angleDiff = Mathf.Max(Vector3.Angle(forward, fdir) - deflectionForgivenessDegrees, 0f);
			body.AddTorque(cross * (angleDiff * 3f), ForceMode.Acceleration);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(eyeRot);
			stream.SendNext(hipVector);
			return;
		}
		networkedEyeRot = (Vector2)stream.ReceiveNext();
		if (networkedEyeRot.x > eyeRot.x + 180f)
		{
			networkedEyeRot.x -= 360f;
		}
		if (networkedEyeRot.x < eyeRot.x - 180f)
		{
			networkedEyeRot.x += 360f;
		}
		networkedAngle = Vector2.Distance(networkedEyeRot, eyeRot);
		desiredHipVector = (Vector2)stream.ReceiveNext();
	}

	public void Save(BinaryWriter writer)
	{
		if (animating)
		{
			writer.Write(currentStationSet.photonView.ViewID);
			writer.Write(currentStationSet.GetAnimationStations().IndexOf(currentStation));
		}
		else
		{
			writer.Write(-1);
			writer.Write(-1);
		}
	}

	public void Load(BinaryReader reader)
	{
		int photonViewID = reader.ReadInt32();
		int animationID = reader.ReadInt32();
		if (photonViewID != -1 && (currentStationSet == null || currentStationSet.photonView.ViewID != photonViewID || currentStation == null || currentStationSet.GetAnimationStations().IndexOf(currentStation) != animationID))
		{
			PhotonView view = PhotonNetwork.GetPhotonView(photonViewID);
			IAnimationStationSet set = view.GetComponentInChildren<IAnimationStationSet>();
			BeginAnimation(set, set.GetAnimationStations()[animationID]);
		}
	}
}
