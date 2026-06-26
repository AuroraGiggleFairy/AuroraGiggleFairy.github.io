using System;
using UnityEngine;

namespace KinematicCharacterController;

[RequireComponent(typeof(CapsuleCollider))]
public class KinematicCharacterMotor : MonoBehaviour
{
	[Header("Components")]
	[ReadOnly]
	public CapsuleCollider Capsule;

	[Header("Capsule Settings")]
	[SerializeField]
	[Tooltip("Radius of the Character Capsule")]
	[PublicizedFrom(EAccessModifier.Private)]
	public float CapsuleRadius = 0.5f;

	[SerializeField]
	[Tooltip("Height of the Character Capsule")]
	[PublicizedFrom(EAccessModifier.Private)]
	public float CapsuleHeight = 2f;

	[SerializeField]
	[Tooltip("Height of the Character Capsule")]
	[PublicizedFrom(EAccessModifier.Private)]
	public float CapsuleYOffset = 1f;

	[SerializeField]
	[Tooltip("Physics material of the Character Capsule (Does not affect character movement. Only affects things colliding with it)")]
	[PublicizedFrom(EAccessModifier.Private)]
	public PhysicMaterial CapsulePhysicsMaterial;

	[Header("Misc settings")]
	[Tooltip("Increases the range of ground detection, to allow snapping to ground at very high speeds")]
	public float GroundDetectionExtraDistance;

	[Range(0f, 89f)]
	[Tooltip("Maximum slope angle on which the character can be stable")]
	public float MaxStableSlopeAngle = 60f;

	[Tooltip("Which layers can the character be considered stable on")]
	public LayerMask StableGroundLayers = -1;

	[Tooltip("Notifies the Character Controller when discrete collisions are detected")]
	public bool DiscreteCollisionEvents;

	[Header("Step settings")]
	[Tooltip("Handles properly detecting grounding status on steps, but has a performance cost.")]
	public StepHandlingMethod StepHandling = StepHandlingMethod.Standard;

	[Tooltip("Maximum height of a step which the character can climb")]
	public float MaxStepHeight = 0.5f;

	[Tooltip("Can the character step up obstacles even if it is not currently stable?")]
	public bool AllowSteppingWithoutStableGrounding;

	[Tooltip("Minimum length of a step that the character can step on (used in Extra stepping method). Use this to let the character step on steps that are smaller that its radius")]
	public float MinRequiredStepDepth = 0.1f;

	[Header("Ledge settings")]
	[Tooltip("Handles properly detecting ledge information and grounding status, but has a performance cost.")]
	public bool LedgeAndDenivelationHandling = true;

	[Tooltip("The distance from the capsule central axis at which the character can stand on a ledge and still be stable")]
	public float MaxStableDistanceFromLedge = 0.5f;

	[Tooltip("Prevents snapping to ground on ledges beyond a certain velocity")]
	public float MaxVelocityForLedgeSnap;

	[Tooltip("The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground")]
	[Range(1f, 180f)]
	public float MaxStableDenivelationAngle = 180f;

	[Header("Rigidbody interaction settings")]
	[Tooltip("Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies")]
	public bool InteractiveRigidbodyHandling = true;

	[Tooltip("How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.")]
	public RigidbodyInteractionType RigidbodyInteractionType;

	[Tooltip("Determines if the character preserves moving platform velocities when de-grounding from them")]
	public bool PreserveAttachedRigidbodyMomentum = true;

	[Header("Constraints settings")]
	[Tooltip("Determines if the character's movement uses the planar constraint")]
	public bool HasPlanarConstraint;

	[Tooltip("Defines the plane that the character's movement is constrained on, if HasMovementConstraintPlane is active")]
	public Vector3 PlanarConstraintAxis = Vector3.forward;

	[NonSerialized]
	public CharacterGroundingReport GroundingStatus;

	[NonSerialized]
	public CharacterTransientGroundingReport LastGroundingStatus;

	[NonSerialized]
	public LayerMask CollidableLayers = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform _transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _transientPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterUp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterForward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterRight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _initialSimulationPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion _initialSimulationRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody _attachedRigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterTransformToCapsuleCenter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterTransformToCapsuleBottom;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterTransformToCapsuleTop;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterTransformToCapsuleBottomHemi;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _characterTransformToCapsuleTopHemi;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _attachedRigidbodyVelocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _overlapsCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public OverlapResult[] _overlaps = new OverlapResult[16];

	[NonSerialized]
	public ICharacterController CharacterController;

	[NonSerialized]
	public bool LastMovementIterationFoundAnyGround;

	[NonSerialized]
	public int IndexInCharacterSystem;

	[NonSerialized]
	public Vector3 InitialTickPosition;

	[NonSerialized]
	public Quaternion InitialTickRotation;

	[NonSerialized]
	public Rigidbody AttachedRigidbodyOverride;

	[NonSerialized]
	public Vector3 BaseVelocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastHit[] _internalCharacterHits = new RaycastHit[16];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Collider[] _internalProbedColliders = new Collider[16];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody[] _rigidbodiesPushedThisMove = new Rigidbody[16];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RigidbodyProjectionHit[] _internalRigidbodyProjectionHits = new RigidbodyProjectionHit[6];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody _lastAttachedRigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _solveMovementCollisions = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _solveGrounding = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _movePositionDirty;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _movePositionTarget = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _moveRotationDirty;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion _moveRotationTarget = Quaternion.identity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _lastSolvedOverlapNormalDirty;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _lastSolvedOverlapNormal = Vector3.forward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _rigidbodiesPushedCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _rigidbodyProjectionHitCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _internalResultingMovementMagnitude;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _internalResultingMovementDirection = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _isMovingFromAttachedRigidbody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _mustUnground;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _mustUngroundTimeCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _cachedWorldUp = Vector3.up;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _cachedWorldForward = Vector3.forward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _cachedWorldRight = Vector3.right;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _cachedZeroVector = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion _transientRotation;

	public const int MaxHitsBudget = 16;

	public const int MaxCollisionBudget = 16;

	public const int MaxGroundingSweepIterations = 2;

	public const int MaxMovementSweepIterations = 6;

	public const int MaxSteppingSweepIterations = 3;

	public const int MaxRigidbodyOverlapsCount = 16;

	public const int MaxDiscreteCollisionIterations = 3;

	public const float CollisionOffset = 0.001f;

	public const float GroundProbeReboundDistance = 0.02f;

	public const float MinimumGroundProbingDistance = 0.005f;

	public const float GroundProbingBackstepDistance = 0.1f;

	public const float SweepProbingBackstepDistance = 0.002f;

	public const float SecondaryProbesVertical = 0.02f;

	public const float SecondaryProbesHorizontal = 0.001f;

	public const float MinVelocityMagnitude = 0.01f;

	public const float SteppingForwardDistance = 0.03f;

	public const float MinDistanceForLedge = 0.05f;

	public const float CorrelationForVerticalObstruction = 0.01f;

	public const float ExtraSteppingForwardDistance = 0.01f;

	public const float ExtraStepHeightPadding = 0.01f;

	public Transform Transform => _transform;

	public Vector3 TransientPosition => _transientPosition;

	public Vector3 CharacterUp => _characterUp;

	public Vector3 CharacterForward => _characterForward;

	public Vector3 CharacterRight => _characterRight;

	public Vector3 InitialSimulationPosition => _initialSimulationPosition;

	public Quaternion InitialSimulationRotation => _initialSimulationRotation;

	public Rigidbody AttachedRigidbody => _attachedRigidbody;

	public Vector3 CharacterTransformToCapsuleCenter => _characterTransformToCapsuleCenter;

	public Vector3 CharacterTransformToCapsuleBottom => _characterTransformToCapsuleBottom;

	public Vector3 CharacterTransformToCapsuleTop => _characterTransformToCapsuleTop;

	public Vector3 CharacterTransformToCapsuleBottomHemi => _characterTransformToCapsuleBottomHemi;

	public Vector3 CharacterTransformToCapsuleTopHemi => _characterTransformToCapsuleTopHemi;

	public Vector3 AttachedRigidbodyVelocity => _attachedRigidbodyVelocity;

	public int OverlapsCount => _overlapsCount;

	public OverlapResult[] Overlaps => _overlaps;

	public Quaternion TransientRotation
	{
		get
		{
			return _transientRotation;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_transientRotation = value;
			_characterUp = _transientRotation * _cachedWorldUp;
			_characterForward = _transientRotation * _cachedWorldForward;
			_characterRight = _transientRotation * _cachedWorldRight;
		}
	}

	public Vector3 Velocity => BaseVelocity + _attachedRigidbodyVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		KinematicCharacterSystem.EnsureCreation();
		KinematicCharacterSystem.RegisterCharacterMotor(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		KinematicCharacterSystem.UnregisterCharacterMotor(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		ValidateData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValidate()
	{
		ValidateData();
	}

	[ContextMenu("Remove Component")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRemoveComponent()
	{
		CapsuleCollider component = base.gameObject.GetComponent<CapsuleCollider>();
		UnityEngine.Object.DestroyImmediate(this);
		UnityEngine.Object.DestroyImmediate(component);
	}

	public void ValidateData()
	{
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().hideFlags = HideFlags.None;
		}
		Capsule = GetComponent<CapsuleCollider>();
		CapsuleRadius = Mathf.Clamp(CapsuleRadius, 0f, CapsuleHeight * 0.5f);
		Capsule.isTrigger = false;
		Capsule.direction = 1;
		Capsule.sharedMaterial = CapsulePhysicsMaterial;
		SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);
		MaxStepHeight = Mathf.Clamp(MaxStepHeight, 0f, float.PositiveInfinity);
		MinRequiredStepDepth = Mathf.Clamp(MinRequiredStepDepth, 0f, CapsuleRadius);
		MaxStableDistanceFromLedge = Mathf.Clamp(MaxStableDistanceFromLedge, 0f, CapsuleRadius);
		base.transform.localScale = Vector3.one;
	}

	public void SetCapsuleCollisionsActivation(bool collisionsActive)
	{
		Capsule.isTrigger = !collisionsActive;
	}

	public void SetMovementCollisionsSolvingActivation(bool movementCollisionsSolvingActive)
	{
		_solveMovementCollisions = movementCollisionsSolvingActive;
	}

	public void SetGroundSolvingActivation(bool stabilitySolvingActive)
	{
		_solveGrounding = stabilitySolvingActive;
	}

	public void SetPosition(Vector3 position, bool bypassInterpolation = true)
	{
		_transform.position = position;
		_initialSimulationPosition = position;
		_transientPosition = position;
		if (bypassInterpolation)
		{
			InitialTickPosition = position;
		}
	}

	public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
	{
		_transform.rotation = rotation;
		_initialSimulationRotation = rotation;
		TransientRotation = rotation;
		if (bypassInterpolation)
		{
			InitialTickRotation = rotation;
		}
	}

	public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
	{
		_transform.SetPositionAndRotation(position, rotation);
		_initialSimulationPosition = position;
		_initialSimulationRotation = rotation;
		_transientPosition = position;
		TransientRotation = rotation;
		if (bypassInterpolation)
		{
			InitialTickPosition = position;
			InitialTickRotation = rotation;
		}
	}

	public void MoveCharacter(Vector3 toPosition)
	{
		_movePositionDirty = true;
		_movePositionTarget = toPosition;
	}

	public void RotateCharacter(Quaternion toRotation)
	{
		_moveRotationDirty = true;
		_moveRotationTarget = toRotation;
	}

	public KinematicCharacterMotorState GetState()
	{
		KinematicCharacterMotorState result = default(KinematicCharacterMotorState);
		result.Position = _transientPosition;
		result.Rotation = _transientRotation;
		result.BaseVelocity = BaseVelocity;
		result.AttachedRigidbodyVelocity = _attachedRigidbodyVelocity;
		result.MustUnground = _mustUnground;
		result.MustUngroundTime = _mustUngroundTimeCounter;
		result.LastMovementIterationFoundAnyGround = LastMovementIterationFoundAnyGround;
		result.GroundingStatus.CopyFrom(GroundingStatus);
		result.AttachedRigidbody = _attachedRigidbody;
		return result;
	}

	public void ApplyState(KinematicCharacterMotorState state, bool bypassInterpolation = true)
	{
		SetPositionAndRotation(state.Position, state.Rotation, bypassInterpolation);
		BaseVelocity = state.BaseVelocity;
		_attachedRigidbodyVelocity = state.AttachedRigidbodyVelocity;
		_mustUnground = state.MustUnground;
		_mustUngroundTimeCounter = state.MustUngroundTime;
		LastMovementIterationFoundAnyGround = state.LastMovementIterationFoundAnyGround;
		GroundingStatus.CopyFrom(state.GroundingStatus);
		_attachedRigidbody = state.AttachedRigidbody;
	}

	public void SetCapsuleDimensions(float radius, float height, float yOffset)
	{
		CapsuleRadius = radius;
		CapsuleHeight = height;
		CapsuleYOffset = yOffset;
		Capsule.radius = CapsuleRadius;
		Capsule.height = Mathf.Clamp(CapsuleHeight, CapsuleRadius * 2f, CapsuleHeight);
		Capsule.center = new Vector3(0f, CapsuleYOffset, 0f);
		_characterTransformToCapsuleCenter = Capsule.center;
		_characterTransformToCapsuleBottom = Capsule.center + -_cachedWorldUp * (Capsule.height * 0.5f);
		_characterTransformToCapsuleTop = Capsule.center + _cachedWorldUp * (Capsule.height * 0.5f);
		_characterTransformToCapsuleBottomHemi = Capsule.center + -_cachedWorldUp * (Capsule.height * 0.5f) + _cachedWorldUp * Capsule.radius;
		_characterTransformToCapsuleTopHemi = Capsule.center + _cachedWorldUp * (Capsule.height * 0.5f) + -_cachedWorldUp * Capsule.radius;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		_transform = base.transform;
		ValidateData();
		_transientPosition = _transform.position;
		TransientRotation = _transform.rotation;
		CollidableLayers = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(base.gameObject.layer, i))
			{
				CollidableLayers = (int)CollidableLayers | (1 << i);
			}
		}
		SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);
	}

	public void UpdatePhase1(float deltaTime)
	{
		if (float.IsNaN(BaseVelocity.x) || float.IsNaN(BaseVelocity.y) || float.IsNaN(BaseVelocity.z))
		{
			BaseVelocity = Vector3.zero;
		}
		if (float.IsNaN(_attachedRigidbodyVelocity.x) || float.IsNaN(_attachedRigidbodyVelocity.y) || float.IsNaN(_attachedRigidbodyVelocity.z))
		{
			_attachedRigidbodyVelocity = Vector3.zero;
		}
		CharacterController.BeforeCharacterUpdate(deltaTime);
		_transientPosition = _transform.position;
		TransientRotation = _transform.rotation;
		_initialSimulationPosition = _transientPosition;
		_initialSimulationRotation = _transientRotation;
		_rigidbodyProjectionHitCount = 0;
		_overlapsCount = 0;
		_lastSolvedOverlapNormalDirty = false;
		if (_movePositionDirty)
		{
			if (_solveMovementCollisions)
			{
				if (InternalCharacterMove(_movePositionTarget - _transientPosition, deltaTime, out _internalResultingMovementMagnitude, out _internalResultingMovementDirection) && InteractiveRigidbodyHandling)
				{
					Vector3 processedVelocity = Vector3.zero;
					ProcessVelocityForRigidbodyHits(ref processedVelocity, deltaTime);
				}
			}
			else
			{
				_transientPosition = _movePositionTarget;
			}
			_movePositionDirty = false;
		}
		LastGroundingStatus.CopyFrom(GroundingStatus);
		GroundingStatus = default(CharacterGroundingReport);
		GroundingStatus.GroundNormal = _characterUp;
		if (_solveMovementCollisions)
		{
			Vector3 direction = _cachedWorldUp;
			float distance = 0f;
			int i = 0;
			bool flag = false;
			for (; i < 3; i++)
			{
				if (flag)
				{
					break;
				}
				int num = CharacterCollisionsOverlap(_transientPosition, _transientRotation, _internalProbedColliders);
				if (num > 0)
				{
					if (!CharacterController.OnCollisionOverlap(num, _internalProbedColliders))
					{
						break;
					}
					for (int j = 0; j < num; j++)
					{
						Transform component = _internalProbedColliders[j].GetComponent<Transform>();
						if (Physics.ComputePenetration(Capsule, _transientPosition, _transientRotation, _internalProbedColliders[j], component.position, component.rotation, out direction, out distance))
						{
							direction = GetObstructionNormal(hitStabilityReport: new HitStabilityReport
							{
								IsStable = IsStableOnNormal(direction)
							}, hitNormal: direction);
							distance *= CharacterController.GetCollisionOverlapScale(component);
							Vector3 vector = direction * (distance + 0.001f);
							_transientPosition += vector;
							if (_overlapsCount < _overlaps.Length)
							{
								_overlaps[_overlapsCount] = new OverlapResult(direction, _internalProbedColliders[j]);
								_overlapsCount++;
							}
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
			}
		}
		if (_solveGrounding)
		{
			if (MustUnground())
			{
				_transientPosition += _characterUp * 0.0075f;
			}
			else
			{
				float probingDistance = 0.005f;
				if (!LastGroundingStatus.SnappingPrevented && (LastGroundingStatus.IsStableOnGround || LastMovementIterationFoundAnyGround))
				{
					probingDistance = ((StepHandling == StepHandlingMethod.None) ? CapsuleRadius : Mathf.Max(CapsuleRadius, MaxStepHeight));
					probingDistance += GroundDetectionExtraDistance;
				}
				ProbeGround(ref _transientPosition, _transientRotation, probingDistance, ref GroundingStatus);
			}
		}
		LastMovementIterationFoundAnyGround = false;
		if (_mustUngroundTimeCounter > 0f)
		{
			_mustUngroundTimeCounter -= deltaTime;
		}
		_mustUnground = false;
		if (_solveGrounding)
		{
			CharacterController.PostGroundingUpdate(deltaTime);
		}
		if (!InteractiveRigidbodyHandling)
		{
			return;
		}
		_lastAttachedRigidbody = _attachedRigidbody;
		if ((bool)AttachedRigidbodyOverride)
		{
			_attachedRigidbody = AttachedRigidbodyOverride;
		}
		else if (GroundingStatus.IsStableOnGround && (bool)GroundingStatus.GroundCollider.attachedRigidbody)
		{
			Rigidbody interactiveRigidbody = GetInteractiveRigidbody(GroundingStatus.GroundCollider);
			if ((bool)interactiveRigidbody)
			{
				_attachedRigidbody = interactiveRigidbody;
			}
		}
		else
		{
			_attachedRigidbody = null;
		}
		Vector3 vector2 = Vector3.zero;
		if ((bool)_attachedRigidbody)
		{
			vector2 = GetVelocityFromRigidbodyMovement(_attachedRigidbody, _transientPosition, deltaTime);
		}
		if (PreserveAttachedRigidbodyMomentum && _lastAttachedRigidbody != null && _attachedRigidbody != _lastAttachedRigidbody)
		{
			BaseVelocity += _attachedRigidbodyVelocity;
			BaseVelocity -= vector2;
		}
		_attachedRigidbodyVelocity = _cachedZeroVector;
		if ((bool)_attachedRigidbody)
		{
			_attachedRigidbodyVelocity = vector2;
			Vector3 normalized = Vector3.ProjectOnPlane(Quaternion.Euler(57.29578f * _attachedRigidbody.angularVelocity * deltaTime) * _characterForward, _characterUp).normalized;
			TransientRotation = Quaternion.LookRotation(normalized, _characterUp);
		}
		if ((bool)GroundingStatus.GroundCollider && (bool)GroundingStatus.GroundCollider.attachedRigidbody && GroundingStatus.GroundCollider.attachedRigidbody == _attachedRigidbody && _attachedRigidbody != null && _lastAttachedRigidbody == null)
		{
			BaseVelocity -= Vector3.ProjectOnPlane(_attachedRigidbodyVelocity, _characterUp);
		}
		if (!(_attachedRigidbodyVelocity.sqrMagnitude > 0f))
		{
			return;
		}
		_isMovingFromAttachedRigidbody = true;
		if (_solveMovementCollisions)
		{
			if (InternalCharacterMove(_attachedRigidbodyVelocity * deltaTime, deltaTime, out _internalResultingMovementMagnitude, out _internalResultingMovementDirection))
			{
				_attachedRigidbodyVelocity = _internalResultingMovementDirection * _internalResultingMovementMagnitude / deltaTime;
			}
			else
			{
				_attachedRigidbodyVelocity = Vector3.zero;
			}
		}
		else
		{
			_transientPosition += _attachedRigidbodyVelocity * deltaTime;
		}
		_isMovingFromAttachedRigidbody = false;
	}

	public void UpdatePhase2(float deltaTime)
	{
		CharacterController.UpdateRotation(ref _transientRotation, deltaTime);
		TransientRotation = _transientRotation;
		if (_moveRotationDirty)
		{
			TransientRotation = _moveRotationTarget;
			_moveRotationDirty = false;
		}
		if (_solveMovementCollisions && InteractiveRigidbodyHandling)
		{
			if (InteractiveRigidbodyHandling && (bool)_attachedRigidbody)
			{
				float radius = Capsule.radius;
				if (CharacterGroundSweep(_transientPosition + _characterUp * radius, _transientRotation, -_characterUp, radius, out var closestHit) && closestHit.collider.attachedRigidbody == _attachedRigidbody && IsStableOnNormal(closestHit.normal))
				{
					float num = radius - closestHit.distance;
					_transientPosition = _transientPosition + _characterUp * num + _characterUp * 0.001f;
				}
			}
			if (InteractiveRigidbodyHandling)
			{
				Vector3 direction = _cachedWorldUp;
				float distance = 0f;
				int i = 0;
				bool flag = false;
				for (; i < 3; i++)
				{
					if (flag)
					{
						break;
					}
					int num2 = CharacterCollisionsOverlap(_transientPosition, _transientRotation, _internalProbedColliders);
					if (num2 > 0)
					{
						for (int j = 0; j < num2; j++)
						{
							Transform component = _internalProbedColliders[j].GetComponent<Transform>();
							if (!Physics.ComputePenetration(Capsule, _transientPosition, _transientRotation, _internalProbedColliders[j], component.position, component.rotation, out direction, out distance))
							{
								continue;
							}
							direction = GetObstructionNormal(hitStabilityReport: new HitStabilityReport
							{
								IsStable = IsStableOnNormal(direction)
							}, hitNormal: direction);
							Vector3 vector = direction * (distance + 0.001f);
							_transientPosition += vector;
							if (InteractiveRigidbodyHandling)
							{
								Rigidbody interactiveRigidbody = GetInteractiveRigidbody(_internalProbedColliders[j]);
								if (interactiveRigidbody != null)
								{
									HitStabilityReport hitStabilityReport = new HitStabilityReport
									{
										IsStable = IsStableOnNormal(direction)
									};
									if (hitStabilityReport.IsStable)
									{
										LastMovementIterationFoundAnyGround = hitStabilityReport.IsStable;
									}
									if (interactiveRigidbody != _attachedRigidbody)
									{
										Vector3 point = _transientPosition + _transientRotation * _characterTransformToCapsuleCenter;
										Vector3 transientPosition = _transientPosition;
										MeshCollider meshCollider = _internalProbedColliders[j] as MeshCollider;
										if (!meshCollider || meshCollider.convex)
										{
											Physics.ClosestPoint(point, _internalProbedColliders[j], component.position, component.rotation);
										}
										StoreRigidbodyHit(interactiveRigidbody, Velocity, transientPosition, direction, hitStabilityReport);
									}
								}
							}
							if (_overlapsCount < _overlaps.Length)
							{
								_overlaps[_overlapsCount] = new OverlapResult(direction, _internalProbedColliders[j]);
								_overlapsCount++;
							}
							break;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
		}
		CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
		if (BaseVelocity.magnitude < 0.01f)
		{
			BaseVelocity = Vector3.zero;
		}
		if (BaseVelocity.sqrMagnitude > 0f)
		{
			if (_solveMovementCollisions)
			{
				if (InternalCharacterMove(BaseVelocity * deltaTime, deltaTime, out _internalResultingMovementMagnitude, out _internalResultingMovementDirection))
				{
					BaseVelocity = _internalResultingMovementDirection * _internalResultingMovementMagnitude / deltaTime;
				}
				else
				{
					BaseVelocity = Vector3.zero;
				}
			}
			else
			{
				_transientPosition += BaseVelocity * deltaTime;
			}
		}
		if (InteractiveRigidbodyHandling)
		{
			ProcessVelocityForRigidbodyHits(ref BaseVelocity, deltaTime);
		}
		if (HasPlanarConstraint)
		{
			_transientPosition = _initialSimulationPosition + Vector3.ProjectOnPlane(_transientPosition - _initialSimulationPosition, PlanarConstraintAxis.normalized);
		}
		if (DiscreteCollisionEvents)
		{
			int num3 = CharacterCollisionsOverlap(_transientPosition, _transientRotation, _internalProbedColliders, 0.002f);
			for (int k = 0; k < num3; k++)
			{
				CharacterController.OnDiscreteCollisionDetected(_internalProbedColliders[k]);
			}
		}
		CharacterController.AfterCharacterUpdate(deltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsStableOnNormal(Vector3 normal)
	{
		return Vector3.Angle(_characterUp, normal) <= MaxStableSlopeAngle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsStableWithSpecialCases(ref HitStabilityReport stabilityReport, Vector3 velocity)
	{
		if (LedgeAndDenivelationHandling)
		{
			if (stabilityReport.LedgeDetected && stabilityReport.IsMovingTowardsEmptySideOfLedge)
			{
				if (velocity.magnitude >= MaxVelocityForLedgeSnap)
				{
					return false;
				}
				if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
				{
					return false;
				}
			}
			if (LastGroundingStatus.FoundAnyGround && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
			{
				if (Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal) > MaxStableDenivelationAngle)
				{
					return false;
				}
				if (Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal) > MaxStableDenivelationAngle)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref CharacterGroundingReport groundingReport)
	{
		if (probingDistance < 0.005f)
		{
			probingDistance = 0.005f;
		}
		int num = 0;
		RaycastHit closestHit = default(RaycastHit);
		bool flag = false;
		Vector3 vector = probingPosition;
		Vector3 vector2 = atRotation * -_cachedWorldUp;
		float num2 = probingDistance;
		while (num2 > 0f && num <= 2 && !flag)
		{
			if (CharacterGroundSweep(vector, atRotation, vector2, num2, out closestHit))
			{
				Vector3 vector3 = vector + vector2 * closestHit.distance;
				HitStabilityReport stabilityReport = default(HitStabilityReport);
				EvaluateHitStability(closestHit.collider, closestHit.normal, closestHit.point, vector3, _transientRotation, BaseVelocity, ref stabilityReport);
				groundingReport.FoundAnyGround = true;
				groundingReport.GroundNormal = closestHit.normal;
				groundingReport.InnerGroundNormal = stabilityReport.InnerNormal;
				groundingReport.OuterGroundNormal = stabilityReport.OuterNormal;
				groundingReport.GroundCollider = closestHit.collider;
				groundingReport.GroundPoint = closestHit.point;
				groundingReport.SnappingPrevented = false;
				if (stabilityReport.IsStable)
				{
					groundingReport.SnappingPrevented = !IsStableWithSpecialCases(ref stabilityReport, BaseVelocity);
					groundingReport.IsStableOnGround = true;
					if (!groundingReport.SnappingPrevented)
					{
						vector3 += -vector2 * 0.001f;
						probingPosition = vector3;
					}
					CharacterController.OnGroundHit(closestHit.collider, closestHit.normal, closestHit.point, ref stabilityReport);
					flag = true;
				}
				else
				{
					Vector3 vector4 = vector2 * closestHit.distance + atRotation * _cachedWorldUp * Mathf.Max(0.001f, closestHit.distance);
					vector += vector4;
					num2 = Mathf.Min(0.02f, Mathf.Max(num2 - vector4.magnitude, 0f));
					vector2 = Vector3.ProjectOnPlane(vector2, closestHit.normal).normalized;
				}
			}
			else
			{
				flag = true;
			}
			num++;
		}
	}

	public void ForceUnground(float time = 0.1f)
	{
		_mustUnground = true;
		_mustUngroundTimeCounter = time;
	}

	public bool MustUnground()
	{
		if (!_mustUnground)
		{
			return _mustUngroundTimeCounter > 0f;
		}
		return true;
	}

	public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
	{
		Vector3 rhs = Vector3.Cross(direction, _characterUp);
		return Vector3.Cross(surfaceNormal, rhs).normalized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InternalCharacterMove(Vector3 movement, float deltaTime, out float resultingMovementMagnitude, out Vector3 resultingMovementDirection)
	{
		_rigidbodiesPushedCount = 0;
		bool result = true;
		float remainingMovementMagnitude = movement.magnitude;
		Vector3 remainingMovementDirection = (resultingMovementDirection = movement.normalized);
		resultingMovementMagnitude = remainingMovementMagnitude;
		int num = 0;
		bool flag = true;
		Vector3 vector = _transientPosition;
		Vector3 originalMoveDirection = remainingMovementDirection;
		Vector3 previousObstructionNormal = _cachedZeroVector;
		MovementSweepState sweepState = MovementSweepState.Initial;
		for (int i = 0; i < _overlapsCount; i++)
		{
			Vector3 normal = _overlaps[i].Normal;
			if (Vector3.Dot(remainingMovementDirection, normal) < 0f)
			{
				InternalHandleMovementProjection(IsStableOnNormal(normal) && !MustUnground(), normal, normal, originalMoveDirection, ref sweepState, ref previousObstructionNormal, ref resultingMovementMagnitude, ref remainingMovementDirection, ref remainingMovementMagnitude);
			}
		}
		while (remainingMovementMagnitude > 0f && num <= 6 && flag)
		{
			if (CharacterCollisionsSweep(vector, _transientRotation, remainingMovementDirection, remainingMovementMagnitude + 0.001f, out var closestHit, _internalCharacterHits) > 0)
			{
				Vector3 vector2 = vector + remainingMovementDirection * closestHit.distance + closestHit.normal * 0.001f;
				Vector3 vector3 = vector2 - vector;
				float magnitude = vector3.magnitude;
				Vector3 withCharacterVelocity = Vector3.zero;
				if (deltaTime > 0f)
				{
					withCharacterVelocity = vector3 / deltaTime;
				}
				HitStabilityReport stabilityReport = default(HitStabilityReport);
				EvaluateHitStability(closestHit.collider, closestHit.normal, closestHit.point, vector2, _transientRotation, withCharacterVelocity, ref stabilityReport);
				bool flag2 = false;
				if (_solveGrounding && StepHandling != StepHandlingMethod.None && stabilityReport.ValidStepDetected && Mathf.Abs(Vector3.Dot(closestHit.normal, _characterUp)) <= 0.01f)
				{
					Vector3 normalized = Vector3.ProjectOnPlane(-closestHit.normal, _characterUp).normalized;
					Vector3 vector4 = vector2 + normalized * 0.03f + _characterUp * MaxStepHeight;
					RaycastHit closestHit2;
					int num2 = CharacterCollisionsSweep(vector4, _transientRotation, -_characterUp, MaxStepHeight, out closestHit2, _internalCharacterHits, 0f, acceptOnlyStableGroundLayer: true);
					for (int j = 0; j < num2; j++)
					{
						if (_internalCharacterHits[j].collider == stabilityReport.SteppedCollider)
						{
							vector = vector4 + -_characterUp * (_internalCharacterHits[j].distance - 0.001f);
							flag2 = true;
							remainingMovementMagnitude = Mathf.Max(remainingMovementMagnitude - magnitude, 0f);
							Vector3 vector5 = remainingMovementMagnitude * remainingMovementDirection;
							vector5 = Vector3.ProjectOnPlane(vector5, CharacterUp);
							remainingMovementMagnitude = vector5.magnitude;
							remainingMovementDirection = vector5.normalized;
							resultingMovementMagnitude = remainingMovementMagnitude;
							break;
						}
					}
				}
				if (!flag2)
				{
					Collider collider = closestHit.collider;
					Vector3 point = closestHit.point;
					Vector3 normal2 = closestHit.normal;
					vector = vector2;
					remainingMovementMagnitude = Mathf.Max(remainingMovementMagnitude - magnitude, 0f);
					CharacterController.OnMovementHit(collider, normal2, point, ref stabilityReport);
					Vector3 obstructionNormal = GetObstructionNormal(normal2, stabilityReport);
					if (InteractiveRigidbodyHandling && (bool)collider.attachedRigidbody)
					{
						StoreRigidbodyHit(collider.attachedRigidbody, remainingMovementDirection * resultingMovementMagnitude / deltaTime, point, obstructionNormal, stabilityReport);
					}
					InternalHandleMovementProjection(stabilityReport.IsStable && !MustUnground(), normal2, obstructionNormal, originalMoveDirection, ref sweepState, ref previousObstructionNormal, ref resultingMovementMagnitude, ref remainingMovementDirection, ref remainingMovementMagnitude);
				}
			}
			else
			{
				flag = false;
			}
			num++;
			if (num > 6)
			{
				remainingMovementMagnitude = 0f;
				result = false;
			}
		}
		_transientPosition = vector + remainingMovementDirection * remainingMovementMagnitude;
		resultingMovementDirection = remainingMovementDirection;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetObstructionNormal(Vector3 hitNormal, HitStabilityReport hitStabilityReport)
	{
		Vector3 vector = hitNormal;
		if (GroundingStatus.IsStableOnGround && !MustUnground() && !hitStabilityReport.IsStable)
		{
			vector = Vector3.Cross(Vector3.Cross(GroundingStatus.GroundNormal, vector).normalized, _characterUp).normalized;
		}
		if (vector.sqrMagnitude == 0f)
		{
			vector = hitNormal;
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal, HitStabilityReport hitStabilityReport)
	{
		if (_rigidbodyProjectionHitCount < _internalRigidbodyProjectionHits.Length && !hitRigidbody.GetComponent<KinematicCharacterMotor>())
		{
			RigidbodyProjectionHit rigidbodyProjectionHit = new RigidbodyProjectionHit
			{
				Rigidbody = hitRigidbody,
				HitPoint = hitPoint,
				EffectiveHitNormal = obstructionNormal,
				HitVelocity = hitVelocity,
				StableOnHit = hitStabilityReport.IsStable
			};
			_internalRigidbodyProjectionHits[_rigidbodyProjectionHitCount] = rigidbodyProjectionHit;
			_rigidbodyProjectionHitCount++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalHandleMovementProjection(bool stableOnHit, Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalMoveDirection, ref MovementSweepState sweepState, ref Vector3 previousObstructionNormal, ref float resultingMovementMagnitude, ref Vector3 remainingMovementDirection, ref float remainingMovementMagnitude)
	{
		if (remainingMovementMagnitude <= 0f)
		{
			return;
		}
		Vector3 movement = originalMoveDirection * remainingMovementMagnitude;
		float num = remainingMovementMagnitude;
		if (stableOnHit)
		{
			LastMovementIterationFoundAnyGround = true;
		}
		if (sweepState == MovementSweepState.FoundBlockingCrease)
		{
			remainingMovementMagnitude = 0f;
			resultingMovementMagnitude = 0f;
			sweepState = MovementSweepState.FoundBlockingCorner;
		}
		else
		{
			HandleMovementProjection(ref movement, obstructionNormal, stableOnHit);
			remainingMovementMagnitude = movement.magnitude;
			remainingMovementDirection = movement.normalized;
			resultingMovementMagnitude = remainingMovementMagnitude / num * resultingMovementMagnitude;
			if (sweepState == MovementSweepState.Initial)
			{
				sweepState = MovementSweepState.AfterFirstHit;
			}
			else if (sweepState == MovementSweepState.AfterFirstHit && Vector3.Dot(previousObstructionNormal, remainingMovementDirection) < 0f)
			{
				Vector3 normalized = Vector3.Cross(previousObstructionNormal, obstructionNormal).normalized;
				movement = Vector3.Project(movement, normalized);
				remainingMovementMagnitude = movement.magnitude;
				remainingMovementDirection = movement.normalized;
				resultingMovementMagnitude = remainingMovementMagnitude / num * resultingMovementMagnitude;
				sweepState = MovementSweepState.FoundBlockingCrease;
			}
		}
		previousObstructionNormal = obstructionNormal;
	}

	public virtual void HandleMovementProjection(ref Vector3 movement, Vector3 obstructionNormal, bool stableOnHit)
	{
		if (GroundingStatus.IsStableOnGround && !MustUnground())
		{
			if (stableOnHit)
			{
				movement = GetDirectionTangentToSurface(movement, obstructionNormal) * movement.magnitude;
				return;
			}
			Vector3 normalized = Vector3.Cross(Vector3.Cross(obstructionNormal, GroundingStatus.GroundNormal).normalized, obstructionNormal).normalized;
			movement = GetDirectionTangentToSurface(movement, normalized) * movement.magnitude;
			movement = Vector3.ProjectOnPlane(movement, obstructionNormal);
		}
		else if (stableOnHit)
		{
			movement = Vector3.ProjectOnPlane(movement, CharacterUp);
			movement = GetDirectionTangentToSurface(movement, obstructionNormal) * movement.magnitude;
		}
		else
		{
			movement = Vector3.ProjectOnPlane(movement, obstructionNormal);
		}
	}

	public virtual void HandleSimulatedRigidbodyInteraction(ref Vector3 processedVelocity, RigidbodyProjectionHit hit, float deltaTime)
	{
		float num = 0.2f;
		if (num > 0f && !hit.StableOnHit && !hit.Rigidbody.isKinematic)
		{
			float num2 = num / hit.Rigidbody.mass;
			Vector3 velocityFromRigidbodyMovement = GetVelocityFromRigidbodyMovement(hit.Rigidbody, hit.HitPoint, deltaTime);
			Vector3 vector = Vector3.Project(hit.HitVelocity, hit.EffectiveHitNormal) - velocityFromRigidbodyMovement;
			hit.Rigidbody.AddForceAtPosition(num2 * vector, hit.HitPoint, ForceMode.VelocityChange);
		}
		if (!hit.StableOnHit)
		{
			Vector3 vector2 = Vector3.Project(GetVelocityFromRigidbodyMovement(hit.Rigidbody, hit.HitPoint, deltaTime), hit.EffectiveHitNormal);
			Vector3 vector3 = Vector3.Project(processedVelocity, hit.EffectiveHitNormal);
			processedVelocity += vector2 - vector3;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
	{
		for (int i = 0; i < _rigidbodyProjectionHitCount; i++)
		{
			if (!_internalRigidbodyProjectionHits[i].Rigidbody)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < _rigidbodiesPushedCount; j++)
			{
				if (_rigidbodiesPushedThisMove[j] == _internalRigidbodyProjectionHits[j].Rigidbody)
				{
					flag = true;
					break;
				}
			}
			if (!flag && _internalRigidbodyProjectionHits[i].Rigidbody != _attachedRigidbody && _rigidbodiesPushedCount < _rigidbodiesPushedThisMove.Length)
			{
				_rigidbodiesPushedThisMove[_rigidbodiesPushedCount] = _internalRigidbodyProjectionHits[i].Rigidbody;
				_rigidbodiesPushedCount++;
				if (RigidbodyInteractionType == RigidbodyInteractionType.SimulatedDynamic)
				{
					HandleSimulatedRigidbodyInteraction(ref processedVelocity, _internalRigidbodyProjectionHits[i], deltaTime);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckIfColliderValidForCollisions(Collider coll)
	{
		if (coll == Capsule)
		{
			return false;
		}
		if (!InternalIsColliderValidForCollisions(coll))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InternalIsColliderValidForCollisions(Collider coll)
	{
		Rigidbody attachedRigidbody = coll.attachedRigidbody;
		if ((bool)attachedRigidbody)
		{
			bool isKinematic = attachedRigidbody.isKinematic;
			if (_isMovingFromAttachedRigidbody && (!isKinematic || attachedRigidbody == _attachedRigidbody))
			{
				return false;
			}
			if (RigidbodyInteractionType == RigidbodyInteractionType.Kinematic && !isKinematic)
			{
				return false;
			}
		}
		if (!CharacterController.IsColliderValidForCollisions(coll))
		{
			return false;
		}
		return true;
	}

	public void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, Vector3 withCharacterVelocity, ref HitStabilityReport stabilityReport)
	{
		if (!_solveGrounding)
		{
			stabilityReport.IsStable = false;
			return;
		}
		Vector3 vector = atCharacterRotation * _cachedWorldUp;
		Vector3 normalized = Vector3.ProjectOnPlane(hitNormal, vector).normalized;
		stabilityReport.IsStable = IsStableOnNormal(hitNormal);
		stabilityReport.InnerNormal = hitNormal;
		stabilityReport.OuterNormal = hitNormal;
		if (LedgeAndDenivelationHandling)
		{
			float num = 0.05f;
			if (StepHandling != StepHandlingMethod.None)
			{
				num = MaxStepHeight;
			}
			bool flag = false;
			bool flag2 = false;
			if (CharacterCollisionsRaycast(hitPoint + vector * 0.02f + normalized * 0.001f, -vector, num + 0.02f, out var closestHit, _internalCharacterHits) > 0)
			{
				flag = IsStableOnNormal(stabilityReport.InnerNormal = closestHit.normal);
			}
			if (CharacterCollisionsRaycast(hitPoint + vector * 0.02f + -normalized * 0.001f, -vector, num + 0.02f, out var closestHit2, _internalCharacterHits) > 0)
			{
				flag2 = IsStableOnNormal(stabilityReport.OuterNormal = closestHit2.normal);
			}
			stabilityReport.LedgeDetected = flag != flag2;
			if (stabilityReport.LedgeDetected)
			{
				stabilityReport.IsOnEmptySideOfLedge = flag2 && !flag;
				stabilityReport.LedgeGroundNormal = (flag2 ? stabilityReport.OuterNormal : stabilityReport.InnerNormal);
				stabilityReport.LedgeRightDirection = Vector3.Cross(hitNormal, stabilityReport.OuterNormal).normalized;
				stabilityReport.LedgeFacingDirection = Vector3.Cross(stabilityReport.LedgeGroundNormal, stabilityReport.LedgeRightDirection).normalized;
				stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane(hitPoint - (atCharacterPosition + atCharacterRotation * _characterTransformToCapsuleBottom), vector).magnitude;
				stabilityReport.IsMovingTowardsEmptySideOfLedge = Vector3.Dot(withCharacterVelocity, Vector3.ProjectOnPlane(stabilityReport.LedgeFacingDirection, CharacterUp)) > 0f;
			}
			if (stabilityReport.IsStable)
			{
				stabilityReport.IsStable = IsStableWithSpecialCases(ref stabilityReport, withCharacterVelocity);
			}
		}
		if (StepHandling != StepHandlingMethod.None && !stabilityReport.IsStable)
		{
			Rigidbody attachedRigidbody = hitCollider.attachedRigidbody;
			if (!attachedRigidbody || attachedRigidbody.isKinematic)
			{
				DetectSteps(atCharacterPosition, atCharacterRotation, hitPoint, normalized, ref stabilityReport);
				if (stabilityReport.ValidStepDetected)
				{
					stabilityReport.IsStable = true;
				}
			}
		}
		CharacterController.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref stabilityReport);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DetectSteps(Vector3 characterPosition, Quaternion characterRotation, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStabilityReport stabilityReport)
	{
		int num = 0;
		Vector3 vector = characterRotation * _cachedWorldUp;
		Vector3 vector2 = Vector3.Project(hitPoint - characterPosition, vector);
		Vector3 vector3 = hitPoint - vector2 + vector * MaxStepHeight;
		num = CharacterCollisionsSweep(vector3, characterRotation, -vector, MaxStepHeight + 0.001f, out var closestHit, _internalCharacterHits, 0f, acceptOnlyStableGroundLayer: true);
		if (CheckStepValidity(num, characterPosition, characterRotation, innerHitDirection, vector3, out var hitCollider))
		{
			stabilityReport.ValidStepDetected = true;
			stabilityReport.SteppedCollider = hitCollider;
		}
		if (StepHandling == StepHandlingMethod.Extra && !stabilityReport.ValidStepDetected)
		{
			vector3 = characterPosition + vector * MaxStepHeight + -innerHitDirection * MinRequiredStepDepth;
			num = CharacterCollisionsSweep(vector3, characterRotation, -vector, MaxStepHeight - 0.001f, out closestHit, _internalCharacterHits, 0f, acceptOnlyStableGroundLayer: true);
			if (CheckStepValidity(num, characterPosition, characterRotation, innerHitDirection, vector3, out hitCollider))
			{
				stabilityReport.ValidStepDetected = true;
				stabilityReport.SteppedCollider = hitCollider;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Quaternion characterRotation, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider)
	{
		hitCollider = null;
		Vector3 vector = characterRotation * Vector3.up;
		bool flag = false;
		while (nbStepHits > 0 && !flag)
		{
			RaycastHit raycastHit = default(RaycastHit);
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < nbStepHits; i++)
			{
				float distance = _internalCharacterHits[i].distance;
				if (distance > num)
				{
					num = distance;
					raycastHit = _internalCharacterHits[i];
					num2 = i;
				}
			}
			Vector3 vector2 = characterPosition + characterRotation * _characterTransformToCapsuleBottom;
			_ = Vector3.Project(raycastHit.point - vector2, vector).sqrMagnitude;
			Vector3 vector3 = stepCheckStartPos + -vector * (raycastHit.distance - 0.001f);
			if (CharacterCollisionsOverlap(vector3, characterRotation, _internalProbedColliders) <= 0 && CharacterCollisionsRaycast(raycastHit.point + vector * 0.02f + -innerHitDirection * 0.001f, -vector, MaxStepHeight + 0.02f, out var closestHit, _internalCharacterHits, acceptOnlyStableGroundLayer: true) > 0 && IsStableOnNormal(closestHit.normal) && CharacterCollisionsSweep(characterPosition, characterRotation, vector, MaxStepHeight - raycastHit.distance, out closestHit, _internalCharacterHits) <= 0)
			{
				bool flag2 = false;
				RaycastHit closestHit2;
				if (AllowSteppingWithoutStableGrounding)
				{
					flag2 = true;
				}
				else if (CharacterCollisionsRaycast(characterPosition + Vector3.Project(vector3 - characterPosition, vector), -vector, MaxStepHeight, out closestHit2, _internalCharacterHits, acceptOnlyStableGroundLayer: true) > 0 && IsStableOnNormal(closestHit2.normal))
				{
					flag2 = true;
				}
				if (!flag2 && CharacterCollisionsRaycast(raycastHit.point + innerHitDirection * 0.001f, -vector, MaxStepHeight, out closestHit2, _internalCharacterHits, acceptOnlyStableGroundLayer: true) > 0 && IsStableOnNormal(closestHit2.normal))
				{
					flag2 = true;
				}
				if (flag2)
				{
					hitCollider = raycastHit.collider;
					flag = true;
					return true;
				}
			}
			if (!flag)
			{
				nbStepHits--;
				if (num2 < nbStepHits)
				{
					_internalCharacterHits[num2] = _internalCharacterHits[nbStepHits];
				}
			}
		}
		return false;
	}

	public Vector3 GetVelocityFromRigidbodyMovement(Rigidbody interactiveRigidbody, Vector3 atPoint, float deltaTime)
	{
		if (deltaTime > 0f)
		{
			Vector3 velocity = interactiveRigidbody.velocity;
			if (interactiveRigidbody.angularVelocity != Vector3.zero)
			{
				Vector3 vector = interactiveRigidbody.position + interactiveRigidbody.centerOfMass;
				Vector3 vector2 = atPoint - vector;
				Quaternion quaternion = Quaternion.Euler(57.29578f * interactiveRigidbody.angularVelocity * deltaTime);
				Vector3 vector3 = vector + quaternion * vector2;
				velocity += (vector3 - atPoint) / deltaTime;
			}
			return velocity;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody GetInteractiveRigidbody(Collider onCollider)
	{
		Rigidbody attachedRigidbody = onCollider.attachedRigidbody;
		if ((bool)attachedRigidbody)
		{
			if ((bool)attachedRigidbody.gameObject.GetComponent<PhysicsMover>())
			{
				return attachedRigidbody;
			}
			if (!attachedRigidbody.isKinematic)
			{
				return attachedRigidbody;
			}
		}
		return null;
	}

	public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
	{
		if (deltaTime > 0f)
		{
			return (toPosition - fromPosition) / deltaTime;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RestrictVectorToPlane(ref Vector3 vector, Vector3 toPlane)
	{
		if (vector.x > 0f != toPlane.x > 0f)
		{
			vector.x = 0f;
		}
		if (vector.y > 0f != toPlane.y > 0f)
		{
			vector.y = 0f;
		}
		if (vector.z > 0f != toPlane.z > 0f)
		{
			vector.z = 0f;
		}
	}

	public int CharacterCollisionsOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
	{
		int layerMask = CollidableLayers;
		if (acceptOnlyStableGroundLayer)
		{
			layerMask = (int)CollidableLayers & (int)StableGroundLayers;
		}
		Vector3 point = position + rotation * _characterTransformToCapsuleBottomHemi;
		Vector3 point2 = position + rotation * _characterTransformToCapsuleTopHemi;
		if (inflate != 0f)
		{
			point += rotation * Vector3.down * inflate;
			point2 += rotation * Vector3.up * inflate;
		}
		int num = 0;
		for (int num2 = (num = Physics.OverlapCapsuleNonAlloc(point, point2, Capsule.radius + inflate, overlappedColliders, layerMask, QueryTriggerInteraction.Ignore)) - 1; num2 >= 0; num2--)
		{
			if (!CheckIfColliderValidForCollisions(overlappedColliders[num2]))
			{
				num--;
				if (num2 < num)
				{
					overlappedColliders[num2] = overlappedColliders[num];
				}
			}
		}
		return num;
	}

	public int CharacterOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, LayerMask layers, QueryTriggerInteraction triggerInteraction, float inflate = 0f)
	{
		Vector3 point = position + rotation * _characterTransformToCapsuleBottomHemi;
		Vector3 point2 = position + rotation * _characterTransformToCapsuleTopHemi;
		if (inflate != 0f)
		{
			point += rotation * Vector3.down * inflate;
			point2 += rotation * Vector3.up * inflate;
		}
		int num = 0;
		for (int num2 = (num = Physics.OverlapCapsuleNonAlloc(point, point2, Capsule.radius + inflate, overlappedColliders, layers, triggerInteraction)) - 1; num2 >= 0; num2--)
		{
			if (overlappedColliders[num2] == Capsule)
			{
				num--;
				if (num2 < num)
				{
					overlappedColliders[num2] = overlappedColliders[num];
				}
			}
		}
		return num;
	}

	public int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
	{
		int layerMask = CollidableLayers;
		if (acceptOnlyStableGroundLayer)
		{
			layerMask = (int)CollidableLayers & (int)StableGroundLayers;
		}
		Vector3 point = position + rotation * _characterTransformToCapsuleBottomHemi - direction * 0.002f;
		Vector3 point2 = position + rotation * _characterTransformToCapsuleTopHemi - direction * 0.002f;
		if (inflate != 0f)
		{
			point += rotation * Vector3.down * inflate;
			point2 += rotation * Vector3.up * inflate;
		}
		int num = 0;
		int num2 = Physics.CapsuleCastNonAlloc(point, point2, Capsule.radius + inflate, direction, hits, distance + 0.002f, layerMask, QueryTriggerInteraction.Ignore);
		closestHit = default(RaycastHit);
		float num3 = float.PositiveInfinity;
		num = num2;
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			hits[num4].distance -= 0.002f;
			RaycastHit raycastHit = hits[num4];
			float distance2 = raycastHit.distance;
			if (distance2 <= 0f || !CheckIfColliderValidForCollisions(raycastHit.collider))
			{
				num--;
				if (num4 < num)
				{
					hits[num4] = hits[num];
				}
			}
			else if (distance2 < num3)
			{
				closestHit = raycastHit;
				num3 = distance2;
			}
		}
		return num;
	}

	public int CharacterSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, LayerMask layers, QueryTriggerInteraction triggerInteraction, float inflate = 0f)
	{
		closestHit = default(RaycastHit);
		Vector3 point = position + rotation * _characterTransformToCapsuleBottomHemi;
		Vector3 point2 = position + rotation * _characterTransformToCapsuleTopHemi;
		if (inflate != 0f)
		{
			point += rotation * Vector3.down * inflate;
			point2 += rotation * Vector3.up * inflate;
		}
		int num = 0;
		int num2 = Physics.CapsuleCastNonAlloc(point, point2, Capsule.radius + inflate, direction, hits, distance, layers, triggerInteraction);
		float num3 = float.PositiveInfinity;
		num = num2;
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			RaycastHit raycastHit = hits[num4];
			if (raycastHit.distance <= 0f || raycastHit.collider == Capsule)
			{
				num--;
				if (num4 < num)
				{
					hits[num4] = hits[num];
				}
			}
			else
			{
				float distance2 = raycastHit.distance;
				if (distance2 < num3)
				{
					closestHit = raycastHit;
					num3 = distance2;
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
	{
		closestHit = default(RaycastHit);
		int num = Physics.CapsuleCastNonAlloc(position + rotation * _characterTransformToCapsuleBottomHemi - direction * 0.1f, position + rotation * _characterTransformToCapsuleTopHemi - direction * 0.1f, Capsule.radius, direction, _internalCharacterHits, distance + 0.1f, (int)CollidableLayers & (int)StableGroundLayers, QueryTriggerInteraction.Ignore);
		bool result = false;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = _internalCharacterHits[i];
			float distance2 = raycastHit.distance;
			if (distance2 > 0f && CheckIfColliderValidForCollisions(raycastHit.collider) && distance2 < num2)
			{
				closestHit = raycastHit;
				closestHit.distance -= 0.1f;
				num2 = distance2;
				result = true;
			}
		}
		return result;
	}

	public int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, bool acceptOnlyStableGroundLayer = false)
	{
		int layerMask = CollidableLayers;
		if (acceptOnlyStableGroundLayer)
		{
			layerMask = (int)CollidableLayers & (int)StableGroundLayers;
		}
		int num = 0;
		int num2 = Physics.RaycastNonAlloc(position, direction, hits, distance, layerMask, QueryTriggerInteraction.Ignore);
		closestHit = default(RaycastHit);
		float num3 = float.PositiveInfinity;
		num = num2;
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			RaycastHit raycastHit = hits[num4];
			float distance2 = raycastHit.distance;
			if (distance2 <= 0f || !CheckIfColliderValidForCollisions(raycastHit.collider))
			{
				num--;
				if (num4 < num)
				{
					hits[num4] = hits[num];
				}
			}
			else if (distance2 < num3)
			{
				closestHit = raycastHit;
				num3 = distance2;
			}
		}
		return num;
	}
}
