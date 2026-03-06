using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGazeController : MonoBehaviour
{
	public static List<CharacterGazeController> instances = new List<CharacterGazeController>();

	public Transform leftEyeTransform;

	public Transform rightEyeTransform;

	public Vector3 leftEyeLocalPosition;

	public Vector3 rightEyeLocalPosition;

	public Transform rootTransform;

	public Transform neckTransform;

	public Transform headTransform;

	[Range(0f, 100f)]
	public float eyeRotationSpeed = 30f;

	[Range(0f, 100f)]
	public float headRotationSpeed = 7f;

	[Range(0f, 50f)]
	public float twitchSpeed = 25f;

	public float eyeLookAtTargetAngle = 35f;

	public float headLookAtTargetAngle = 75f;

	[Range(0f, 20f)]
	public float maxLookAtDistance = 5f;

	public SkinnedMeshRenderer eyeSkinnedMeshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material eyeMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform lookAtTransformOverride;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lookAtCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform lookAtTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom random;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> lookatOffsets = new List<Vector3>
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(-2f, 0f, 0f),
		new Vector3(2f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 0f, -2f),
		new Vector3(0f, 0f, 2f)
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityAlive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion currentLeftEyeRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion currentRightEyeRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion currentHeadRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float gazeTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lookatOffsetIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldSnapHeadNextUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldSnapEyesNextUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDead;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lookAtTargetDefaultPosition = new Vector3(0f, 1.7f, 10f);

	public Transform LookAtTarget
	{
		get
		{
			if (lookAtTarget == null)
			{
				lookAtTarget = new GameObject("LookAtTarget").transform;
				lookAtTarget.parent = rootTransform;
				lookAtTarget.localPosition = lookAtTargetDefaultPosition;
				return lookAtTarget;
			}
			return lookAtTarget;
		}
	}

	public Transform LookAtTransformOverride
	{
		get
		{
			return lookAtTransformOverride;
		}
		set
		{
			lookAtTransformOverride = value;
		}
	}

	public GameRandom Random
	{
		get
		{
			if (random != null)
			{
				return random;
			}
			return random = GameRandomManager.Instance.CreateGameRandom();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		entityAlive = GetComponentInParent<EntityAlive>();
		eyeMaterial = new Material(eyeSkinnedMeshRenderer.material);
		if (eyeMaterial == null || eyeMaterial.shader.name != "Game/SDCS/Eye")
		{
			Debug.LogError("Eye Material is not valid");
			base.enabled = false;
		}
		else
		{
			eyeSkinnedMeshRenderer.material = eyeMaterial;
			gazeTimer = Time.realtimeSinceStartup + Random.RandomRange(0.25f, 2f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		instances.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (instances.Contains(this))
		{
			instances.Remove(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (instances.Contains(this))
		{
			instances.Remove(this);
		}
		if (lookAtTarget != null)
		{
			UnityEngine.Object.Destroy(lookAtTarget.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (entityAlive != null)
		{
			if (entityAlive.emodel.IsRagdollActive)
			{
				return;
			}
			isDead = entityAlive.IsDead();
		}
		UpdateLookAtTarget();
		UpdateHeadRotation();
		UpdateEyeGaze();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookAtTarget()
	{
		if (lookAtTransformOverride != null)
		{
			lookAtCamera = true;
			LookAtTarget.position = lookAtTransformOverride.position;
			return;
		}
		CharacterGazeController characterGazeController = null;
		float num = float.PositiveInfinity;
		for (int num2 = instances.Count - 1; num2 >= 0; num2--)
		{
			CharacterGazeController characterGazeController2 = instances[num2];
			if (characterGazeController2 == null)
			{
				instances.RemoveAt(num2);
			}
			else if (characterGazeController2.enabled && characterGazeController2 != this)
			{
				Vector3 to = characterGazeController2.headTransform.position - headTransform.position;
				float num3 = Vector3.Angle(neckTransform.forward, to);
				float magnitude = to.magnitude;
				if (num3 < eyeLookAtTargetAngle && magnitude <= maxLookAtDistance && num3 < num)
				{
					characterGazeController = characterGazeController2;
					num = num3;
				}
			}
		}
		if (characterGazeController != null)
		{
			lookAtCamera = true;
			LookAtTarget.position = characterGazeController.headTransform.position;
		}
		else
		{
			bool flag = entityAlive is EntityPlayerLocal;
			EntityPlayerLocal entityPlayerLocal = ((!(GameManager.Instance != null) || GameManager.Instance.World == null) ? null : GameManager.Instance.World.GetPrimaryPlayer());
			Vector3 vector = rootTransform.TransformPoint(lookAtTargetDefaultPosition);
			if (entityPlayerLocal != null && entityPlayerLocal.cameraTransform != null)
			{
				vector = (flag ? (entityPlayerLocal.bFirstPersonView ? entityPlayerLocal.cameraTransform.position : ((!entityPlayerLocal.IsCameraFacingCharacter()) ? (entityPlayerLocal.cameraTransform.position + entityPlayerLocal.cameraTransform.forward * 10f) : entityPlayerLocal.cameraTransform.position)) : ((!entityPlayerLocal.bFirstPersonView) ? entityPlayerLocal.getHeadPosition() : entityPlayerLocal.cameraTransform.position));
			}
			else if (Camera.main != null)
			{
				vector = Camera.main.transform.position;
			}
			Vector3 to2 = vector - headTransform.position;
			float num4 = Vector3.Angle(neckTransform.forward, to2);
			float magnitude2 = to2.magnitude;
			lookAtCamera = false;
			if (num4 < headLookAtTargetAngle && magnitude2 <= maxLookAtDistance)
			{
				lookAtCamera = true;
			}
			else if (entityPlayerLocal != null && flag && !entityPlayerLocal.bFirstPersonView && !entityPlayerLocal.inventory.holdingItem.IsActionRunning(entityPlayerLocal.inventory.holdingItemData))
			{
				lookAtCamera = true;
			}
			if (lookAtCamera)
			{
				LookAtTarget.position = vector;
			}
			else
			{
				LookAtTarget.localPosition = lookAtTargetDefaultPosition;
			}
		}
		if (Time.realtimeSinceStartup > gazeTimer)
		{
			lookatOffsetIndex = Random.RandomRange(0, lookatOffsets.Count);
			gazeTimer = Time.realtimeSinceStartup + Random.RandomRange(0.25f, 2f);
		}
	}

	public void SnapNextUpdate()
	{
		shouldSnapHeadNextUpdate = true;
		shouldSnapEyesNextUpdate = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadRotation()
	{
		Vector3 normalized = (LookAtTarget.position - headTransform.position).normalized;
		Vector3 normalized2 = (normalized + neckTransform.forward).normalized;
		if (Vector3.Angle(neckTransform.forward, normalized) < headLookAtTargetAngle && !isDead)
		{
			if (shouldSnapHeadNextUpdate)
			{
				currentHeadRotation = Quaternion.FromToRotation(neckTransform.forward, normalized2);
				shouldSnapHeadNextUpdate = false;
			}
			else
			{
				currentHeadRotation = Quaternion.Slerp(currentHeadRotation, Quaternion.FromToRotation(neckTransform.forward, normalized2), headRotationSpeed * Time.deltaTime);
			}
		}
		else if (shouldSnapHeadNextUpdate)
		{
			currentHeadRotation = Quaternion.identity;
			shouldSnapHeadNextUpdate = false;
		}
		else
		{
			currentHeadRotation = Quaternion.Slerp(currentHeadRotation, Quaternion.identity, headRotationSpeed * Time.deltaTime);
		}
		headTransform.rotation = currentHeadRotation * neckTransform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEyeGaze()
	{
		Vector3 localPosition = LookAtTarget.localPosition;
		if (!lookAtCamera)
		{
			LookAtTarget.position += lookatOffsets[lookatOffsetIndex];
		}
		Vector3 toDirection = LookAtTarget.position - headTransform.TransformPoint(leftEyeLocalPosition);
		Vector3 toDirection2 = LookAtTarget.position - headTransform.TransformPoint(rightEyeLocalPosition);
		Quaternion b = Quaternion.FromToRotation(leftEyeTransform.forward, toDirection);
		Quaternion b2 = Quaternion.FromToRotation(rightEyeTransform.forward, toDirection2);
		float num = Mathf.Sin(Time.time * twitchSpeed) * 0.5f + 0.5f;
		if (isDead)
		{
			b = Quaternion.identity;
			b2 = Quaternion.identity;
		}
		if (shouldSnapEyesNextUpdate)
		{
			currentLeftEyeRotation = b;
			currentRightEyeRotation = b2;
			shouldSnapEyesNextUpdate = false;
		}
		else
		{
			currentLeftEyeRotation = Quaternion.Slerp(currentLeftEyeRotation, b, eyeRotationSpeed * num * Time.deltaTime);
			currentRightEyeRotation = Quaternion.Slerp(currentRightEyeRotation, b2, eyeRotationSpeed * num * Time.deltaTime);
		}
		currentLeftEyeRotation.x = Utils.FastClamp(currentLeftEyeRotation.x, -0.2f, 0.2f);
		currentLeftEyeRotation.y = Utils.FastClamp(currentLeftEyeRotation.y, -0.4f, 0.4f);
		currentRightEyeRotation.x = Utils.FastClamp(currentRightEyeRotation.x, -0.2f, 0.2f);
		currentRightEyeRotation.y = Utils.FastClamp(currentRightEyeRotation.y, -0.4f, 0.4f);
		eyeMaterial.SetVector("_LeftEyeRotation", new Vector4(0f - currentLeftEyeRotation.x, currentLeftEyeRotation.y, currentLeftEyeRotation.z, currentLeftEyeRotation.w));
		eyeMaterial.SetVector("_RightEyeRotation", new Vector4(0f - currentRightEyeRotation.x, currentRightEyeRotation.y, currentRightEyeRotation.z, currentRightEyeRotation.w));
		eyeMaterial.SetVector("_LeftEyePosition", leftEyeLocalPosition);
		eyeMaterial.SetVector("_RightEyePosition", rightEyeLocalPosition);
		LookAtTarget.localPosition = localPosition;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void Cleanup()
	{
		instances.Clear();
	}
}
