using System;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController;

[DefaultExecutionOrder(-100)]
public class KinematicCharacterSystem : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static KinematicCharacterSystem _instance;

	public static List<KinematicCharacterMotor> CharacterMotors = new List<KinematicCharacterMotor>(100);

	public static List<PhysicsMover> PhysicsMovers = new List<PhysicsMover>(100);

	public static bool AutoSimulation = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float _lastCustomInterpolationStartTime = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float _lastCustomInterpolationDeltaTime = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int CharacterMotorsBaseCapacity = 100;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int PhysicsMoversBaseCapacity = 100;

	public static bool Interpolate = true;

	public static void EnsureCreation()
	{
		if (_instance == null)
		{
			GameObject obj = new GameObject("KinematicCharacterSystem");
			_instance = obj.AddComponent<KinematicCharacterSystem>();
			obj.hideFlags = HideFlags.NotEditable;
			_instance.hideFlags = HideFlags.NotEditable;
		}
	}

	public static KinematicCharacterSystem GetInstance()
	{
		return _instance;
	}

	public static void SetCharacterMotorsCapacity(int capacity)
	{
		if (capacity < CharacterMotors.Count)
		{
			capacity = CharacterMotors.Count;
		}
		CharacterMotors.Capacity = capacity;
	}

	public static void RegisterCharacterMotor(KinematicCharacterMotor motor)
	{
		CharacterMotors.Add(motor);
	}

	public static void UnregisterCharacterMotor(KinematicCharacterMotor motor)
	{
		CharacterMotors.Remove(motor);
	}

	public static void SetPhysicsMoversCapacity(int capacity)
	{
		if (capacity < PhysicsMovers.Count)
		{
			capacity = PhysicsMovers.Count;
		}
		PhysicsMovers.Capacity = capacity;
	}

	public static void RegisterPhysicsMover(PhysicsMover mover)
	{
		PhysicsMovers.Add(mover);
		mover.Rigidbody.interpolation = RigidbodyInterpolation.None;
	}

	public static void UnregisterPhysicsMover(PhysicsMover mover)
	{
		PhysicsMovers.Remove(mover);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		_instance = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (AutoSimulation)
		{
			float deltaTime = Time.deltaTime;
			if (Interpolate)
			{
				PreSimulationInterpolationUpdate(deltaTime);
			}
			Simulate(deltaTime, CharacterMotors, CharacterMotors.Count, PhysicsMovers, PhysicsMovers.Count);
			if (Interpolate)
			{
				PostSimulationInterpolationUpdate(deltaTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Interpolate)
		{
			CustomInterpolationUpdate();
		}
	}

	public static void PreSimulationInterpolationUpdate(float deltaTime)
	{
		for (int i = 0; i < CharacterMotors.Count; i++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = CharacterMotors[i];
			kinematicCharacterMotor.InitialTickPosition = kinematicCharacterMotor.TransientPosition;
			kinematicCharacterMotor.InitialTickRotation = kinematicCharacterMotor.TransientRotation;
			kinematicCharacterMotor.Transform.SetPositionAndRotation(kinematicCharacterMotor.TransientPosition, kinematicCharacterMotor.TransientRotation);
		}
		for (int j = 0; j < PhysicsMovers.Count; j++)
		{
			PhysicsMover physicsMover = PhysicsMovers[j];
			physicsMover.InitialTickPosition = physicsMover.TransientPosition;
			physicsMover.InitialTickRotation = physicsMover.TransientRotation;
			physicsMover.Transform.SetPositionAndRotation(physicsMover.TransientPosition, physicsMover.TransientRotation);
			physicsMover.Rigidbody.position = physicsMover.TransientPosition;
			physicsMover.Rigidbody.rotation = physicsMover.TransientRotation;
		}
	}

	public static void Simulate(float deltaTime, List<KinematicCharacterMotor> motors, int characterMotorsCount, List<PhysicsMover> movers, int physicsMoversCount)
	{
		for (int i = 0; i < physicsMoversCount; i++)
		{
			movers[i].VelocityUpdate(deltaTime);
		}
		for (int j = 0; j < characterMotorsCount; j++)
		{
			motors[j].UpdatePhase1(deltaTime);
		}
		for (int k = 0; k < physicsMoversCount; k++)
		{
			PhysicsMover physicsMover = movers[k];
			physicsMover.Transform.SetPositionAndRotation(physicsMover.TransientPosition, physicsMover.TransientRotation);
			physicsMover.Rigidbody.position = physicsMover.TransientPosition;
			physicsMover.Rigidbody.rotation = physicsMover.TransientRotation;
		}
		for (int l = 0; l < characterMotorsCount; l++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = motors[l];
			kinematicCharacterMotor.UpdatePhase2(deltaTime);
			kinematicCharacterMotor.Transform.SetPositionAndRotation(kinematicCharacterMotor.TransientPosition, kinematicCharacterMotor.TransientRotation);
		}
		Physics.SyncTransforms();
	}

	public static void PostSimulationInterpolationUpdate(float deltaTime)
	{
		_lastCustomInterpolationStartTime = Time.time;
		_lastCustomInterpolationDeltaTime = deltaTime;
		for (int i = 0; i < CharacterMotors.Count; i++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = CharacterMotors[i];
			kinematicCharacterMotor.Transform.SetPositionAndRotation(kinematicCharacterMotor.InitialTickPosition, kinematicCharacterMotor.InitialTickRotation);
		}
		for (int j = 0; j < PhysicsMovers.Count; j++)
		{
			PhysicsMover physicsMover = PhysicsMovers[j];
			physicsMover.Rigidbody.position = physicsMover.InitialTickPosition;
			physicsMover.Rigidbody.rotation = physicsMover.InitialTickRotation;
			physicsMover.Rigidbody.MovePosition(physicsMover.TransientPosition);
			physicsMover.Rigidbody.MoveRotation(physicsMover.TransientRotation);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CustomInterpolationUpdate()
	{
		float t = Mathf.Clamp01((Time.time - _lastCustomInterpolationStartTime) / _lastCustomInterpolationDeltaTime);
		for (int i = 0; i < CharacterMotors.Count; i++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = CharacterMotors[i];
			kinematicCharacterMotor.Transform.SetPositionAndRotation(Vector3.Lerp(kinematicCharacterMotor.InitialTickPosition, kinematicCharacterMotor.TransientPosition, t), Quaternion.Slerp(kinematicCharacterMotor.InitialTickRotation, kinematicCharacterMotor.TransientRotation, t));
		}
		for (int j = 0; j < PhysicsMovers.Count; j++)
		{
			PhysicsMover physicsMover = PhysicsMovers[j];
			physicsMover.Transform.SetPositionAndRotation(Vector3.Lerp(physicsMover.InitialTickPosition, physicsMover.TransientPosition, t), Quaternion.Slerp(physicsMover.InitialTickRotation, physicsMover.TransientRotation, t));
		}
	}
}
