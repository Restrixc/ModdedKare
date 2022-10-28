using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KoboldCharacterController : MonoBehaviourPun, IPunObservable, ISavable
{
	[Serializable]
	public class PID
	{
		public float P;

		public float D;

		private float error = 0f;

		private float errorDifference = 0f;

		private float lastError = 0f;

		private float timeStep = 0f;

		public PID(float initP, float initI, float initD)
		{
			P = initP;
			D = initD;
		}

		public void UpdatePID(float newError, float timeElapsed)
		{
			error = newError;
			errorDifference = error - lastError;
			lastError = error;
			timeStep = timeElapsed;
		}

		public float GetCorrection()
		{
			return error * P * timeStep * 100f + errorDifference * D * timeStep * 200f;
		}
	}

	[HideInInspector]
	public Rigidbody body;

	private Vector3 worldModelOffset = Vector3.zero;

	private Vector3 velocity = new Vector3(0f, 0f, 0f);

	[HideInInspector]
	public bool grounded = false;

	[HideInInspector]
	public bool jumped = false;

	[HideInInspector]
	public float jumpTimer = 0f;

	[HideInInspector]
	public float crouchAmount = 0f;

	[HideInInspector]
	public Vector3 groundVelocity;

	[HideInInspector]
	public Rigidbody groundRigidbody;

	public float frictionMultiplier = 1f;

	public float speedMultiplier = 1f;

	public List<AudioClip> footlands = new List<AudioClip>();

	private bool groundedMemory;

	private float distanceToGround;

	private float colliderFullHeight;

	private Vector3 colliderNormalCenter;

	public float airSpeedMultiplier = 1f;

	public Vector3 inputDir = new Vector3(0f, 0f, 0f);

	public bool inputJump = false;

	private float inputCrouched = 0f;

	private float targetCrouched;

	private float targetCrouchedVel;

	public bool inputWalking = false;

	[Tooltip("Gravity applied per second to the character, generally to make the gravity feel less floaty.")]
	public Vector3 gravityMod = new Vector3(0f, -0.25f, 0f);

	[Tooltip("Fixed impulse for how high the character jumps.")]
	public float jumpStrength = 8f;

	[Tooltip("The speed at which acceleration is no longer applied when the player is moving.")]
	public float speed = 19f;

	[Tooltip("The speed at which acceleration is no longer applied when the player is crouch-moving.")]
	public float crouchSpeed = 13f;

	[Tooltip("Speed multiplier for when inputWalking is true.")]
	public float walkSpeedMultiplier = 0.4f;

	[Tooltip("How quickly the player reaches max speed while walking.")]
	public float accel = 5f;

	[Tooltip("How quickly the player reaches max speed while crouch-walking.")]
	public float crouchAccel = 2f;

	[Tooltip("How quickly the player reaches max speed while in the air.")]
	public float airAccel = 6f;

	[SerializeField]
	[Tooltip("How high the character should float off the ground. (measured from capsule center to ground)")]
	public float stepHeight = 1.2f;

	[Tooltip("How high the character should float off the ground while crouched. (measured from capsule center to ground)")]
	public float stepHeightCrouched = 0.6f;

	[Tooltip("How far to raycast in order to suck the player to the floor, necessary for walking down slopes. (measured from capsule center to ground)")]
	public float stepHeightCheckDistance = 1.6f;

	[Tooltip("Basically the constraint settings for keeping the player a certain distance from the floor.")]
	public PID groundingPID = new PID(0.9f, 0f, 1.8f);

	[Tooltip("How fat the raycast is to check for walkable ground under the capsule collider.")]
	public float groundCheckRadius = 0.2f;

	[Tooltip("How sharp the player movement is, high friction means sharper movement.")]
	public float friction = 9f;

	[Tooltip("How sharp the player movement is while crouched, high friction means sharper movement.")]
	public float crouchFriction = 11f;

	[Tooltip("The collider of the player capsule.")]
	public CapsuleCollider collider;

	[Tooltip("How tall the collider should be during a full crouch.")]
	public float crouchHeight = 0.75f;

	[Tooltip("Reference to the player model so we can push it up and down based on when we crouch.")]
	public Transform worldModel;

	public float airCap = 0.6f;

	private Vector3 defaultWorldModelPosition = Vector3.zero;

	private RaycastHit[] hitArray = new RaycastHit[5];

	private float effectiveSpeed
	{
		get
		{
			float s = Mathf.Lerp(grounded ? speed : (speed * airSpeedMultiplier), crouchSpeed, crouchAmount);
			float f = Mathf.Lerp(s, s * walkSpeedMultiplier, (grounded && inputWalking) ? 1f : 0f);
			return f * speedMultiplier * Mathf.Lerp(base.transform.lossyScale.x, 1f, 0.5f);
		}
	}

	private float effectiveAccel
	{
		get
		{
			float a = Mathf.Lerp(accel, crouchAccel, crouchAmount);
			float f = Mathf.Lerp(a, a * walkSpeedMultiplier, (grounded && inputWalking) ? 1f : 0f);
			return grounded ? f : airAccel;
		}
	}

	private float effectiveStepHeight => Mathf.Lerp(stepHeight, stepHeightCrouched, crouchAmount) * base.transform.lossyScale.x + 0.06f;

	private float effectiveStepCheckDistance => (jumpTimer > 0f || !grounded) ? effectiveStepHeight : (stepHeightCheckDistance * base.transform.lossyScale.x);

	private float effectiveFriction => Mathf.Lerp(friction, crouchFriction, Mathf.Round(crouchAmount)) * frictionMultiplier;

	public void SetInputCrouched(float input)
	{
		targetCrouched = Mathf.Clamp01(input);
	}

	public float GetInputCrouched()
	{
		return targetCrouched;
	}

	public void SetSpeed(float speed)
	{
		this.speed = speed;
	}

	public void SetJumpHeight(float jumpStrength)
	{
		this.jumpStrength = jumpStrength;
	}

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		body.useGravity = base.photonView.IsMine;
		colliderFullHeight = collider.height;
		colliderNormalCenter = collider.center;
		defaultWorldModelPosition = worldModel.localPosition;
	}

	private bool Stuck()
	{
		Vector3 topPoint = collider.transform.TransformPoint(collider.center + new Vector3(0f, collider.height / 2f, 0f));
		Vector3 botPoint = collider.transform.TransformPoint(collider.center);
		Collider[] array = Physics.OverlapCapsule(topPoint, botPoint, collider.radius * collider.transform.lossyScale.x, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore);
		foreach (Collider c in array)
		{
			if (!(c.transform.root == base.transform.root))
			{
				return true;
			}
		}
		return false;
	}

	private void CheckCrouched()
	{
		float oldHeight = collider.height;
		collider.height = Mathf.MoveTowards(collider.height, Mathf.Lerp(colliderFullHeight, crouchHeight, inputCrouched), Time.deltaTime * 2f);
		float diff = (collider.height - oldHeight) / 2f;
		if (diff > 0f && Stuck())
		{
			collider.height = oldHeight;
		}
		else
		{
			float oldStepHeight = effectiveStepHeight;
			crouchAmount = collider.height.Remap(crouchHeight, colliderFullHeight, 1f, 0f);
			diff += effectiveStepHeight - oldStepHeight;
			worldModelOffset -= new Vector3(0f, diff * 0.5f, 0f);
			if (grounded)
			{
				body.MovePosition(body.position + new Vector3(0f, diff, 0f) / 2f);
			}
			else
			{
				body.MovePosition(body.position - new Vector3(0f, diff, 0f) / 2f);
			}
		}
		worldModel.localPosition = defaultWorldModelPosition + worldModelOffset / worldModel.parent.lossyScale.y;
	}

	private void Friction()
	{
		float speed = velocity.magnitude;
		if (speed < 0.1f)
		{
			velocity = Vector3.zero;
			return;
		}
		float stopSpeed = 1f;
		float control = ((speed < stopSpeed) ? stopSpeed : speed);
		float drop = 0f;
		drop += control * effectiveFriction * Time.fixedDeltaTime;
		float newspeed = speed - drop;
		if (newspeed < 0f)
		{
			newspeed = 0f;
		}
		newspeed /= speed;
		velocity *= newspeed;
	}

	private void GroundCalculate()
	{
		groundVelocity = Vector3.zero;
		distanceToGround = float.MaxValue;
		Vector3 floorNormal = Vector3.up;
		bool actuallyHit = false;
		float radius = groundCheckRadius * base.transform.lossyScale.x;
		for (float x = 0f - radius; x <= radius; x += radius)
		{
			for (float y = 0f - radius; y <= radius; y += radius)
			{
				if (!Physics.Raycast(collider.transform.TransformPoint(collider.center) + new Vector3(x, 0f, y), Vector3.down, out var hit, 5f * base.transform.lossyScale.x, GameManager.instance.walkableGroundMask))
				{
					continue;
				}
				floorNormal = hit.normal;
				distanceToGround = hit.distance;
				if (hit.normal.y >= 0.7f && hit.distance <= effectiveStepCheckDistance + 0.05f)
				{
					if ((bool)hit.rigidbody)
					{
						groundVelocity = hit.rigidbody.GetPointVelocity(hit.point);
					}
					groundRigidbody = hit.rigidbody;
					actuallyHit = true;
					break;
				}
			}
			if (actuallyHit)
			{
				break;
			}
		}
		if (!actuallyHit)
		{
			groundRigidbody = null;
		}
		if (distanceToGround <= effectiveStepCheckDistance + 0.05f && floorNormal.y >= 0.7f)
		{
			grounded = true;
			groundingPID.UpdatePID(0f - (distanceToGround - effectiveStepHeight), Time.fixedDeltaTime);
			if (jumpTimer <= 0f)
			{
				velocity += Vector3.up * Mathf.Min(groundingPID.GetCorrection(), jumpStrength);
			}
		}
		else
		{
			floorNormal = Vector3.up;
			grounded = false;
		}
	}

	private void AirBrake(Vector3 wishdir)
	{
		float d = Vector3.Dot(Vector3.Normalize(wishdir), Vector3.Normalize(velocity));
		if (d < -0.25f)
		{
			velocity = Vector3.ProjectOnPlane(velocity, wishdir);
		}
		else if (d < 0f)
		{
			float oldMag = velocity.magnitude;
			velocity = Vector3.ProjectOnPlane(velocity, wishdir);
			if (velocity.magnitude > 0f)
			{
				velocity *= oldMag / velocity.magnitude;
			}
		}
	}

	private void Update()
	{
		if (base.enabled)
		{
			inputCrouched = Mathf.SmoothDamp(inputCrouched, targetCrouched, ref targetCrouchedVel, 0.1f);
			CheckCrouched();
		}
	}

	private void CheckSounds()
	{
		if (grounded && grounded != groundedMemory)
		{
			GameManager.instance.SpawnAudioClipInWorld(footlands[UnityEngine.Random.Range(0, footlands.Count - 1)], base.transform.position + Vector3.down * distanceToGround, 0.85f);
		}
		groundedMemory = grounded;
	}

	private void FixedUpdate()
	{
		if (base.enabled)
		{
			velocity = body.velocity;
			jumped = false;
			velocity -= groundVelocity;
			groundVelocity = Vector3.zero;
			GroundCalculate();
			if (!grounded)
			{
				velocity += gravityMod * (base.transform.localScale.x * Time.deltaTime);
			}
			JumpCheck();
			if (grounded)
			{
				Friction();
			}
			body.useGravity = !grounded;
			if (inputDir.magnitude == 0f)
			{
				Accelerate(Vector3.forward, 0f, effectiveAccel);
			}
			else
			{
				Accelerate(inputDir, effectiveSpeed, effectiveAccel);
			}
			CheckSounds();
			velocity += groundVelocity;
			if (base.photonView.IsMine)
			{
				body.velocity = velocity;
			}
		}
	}

	private void JumpCheck()
	{
		jumpTimer -= Time.fixedDeltaTime;
		if (grounded && inputJump)
		{
			jumped = true;
			velocity.y = Mathf.Max(velocity.y, jumpStrength * Mathf.Lerp(base.transform.lossyScale.x, 1f, 0.5f));
			grounded = false;
			jumpTimer = 0.25f;
		}
	}

	private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
	{
		float wishspd = wishspeed;
		if (!grounded)
		{
			wishspd = Mathf.Min(wishspd, airCap);
		}
		float currentspeed = Vector3.Dot(velocity, wishdir);
		float addspeed = wishspd - currentspeed;
		if (!(addspeed <= 0f))
		{
			float accelspeed = accel * wishspeed * Time.deltaTime;
			accelspeed = Mathf.Min(accelspeed, addspeed);
			velocity += accelspeed * wishdir;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(inputJump);
			stream.SendNext(targetCrouched);
		}
		else
		{
			inputJump = (bool)stream.ReceiveNext();
			targetCrouched = (float)stream.ReceiveNext();
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(targetCrouched);
	}

	public void Load(BinaryReader reader)
	{
		targetCrouched = reader.ReadSingle();
	}
}
