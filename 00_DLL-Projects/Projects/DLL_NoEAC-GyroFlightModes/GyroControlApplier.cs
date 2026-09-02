using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace GyroFlightModes
{
	/// <summary>
	/// Control-table swap only. Speed, torque, tank, seats, and storage stay on the loaded gyro.
	/// Heli motors/forces follow Bdubayah MD-500 (Vehicles AIO), mapped to vanilla gyro bones.
	/// </summary>
	internal static class GyroControlApplier
	{
		private static readonly FieldInfo MotorsField = AccessTools.Field(typeof(EntityVehicle), "motors");
		private static readonly FieldInfo ForcesField = AccessTools.Field(typeof(EntityVehicle), "forces");
		private static readonly Type MotorType = AccessTools.Inner(typeof(EntityVehicle), "Motor");
		private static readonly Type ForceType = AccessTools.Inner(typeof(EntityVehicle), "Force");

		private static MotorSnapshot[] originalMotors;
		private static ForceSnapshot[] originalForces;
		private static bool originalCaptured;

		private static object motorTriggerOn;
		private static object forceTriggerMotor0;
		private static object forceTriggerMotor1;
		private static object forceTriggerInputForward;
		private static object forceTriggerInputStrafe;
		private static object forceTriggerInputUp;
		private static object forceTriggerInputDown;
		private static object forceTypeRelative;
		private static object forceTypeRelativeTorque;

		internal static void CaptureOriginalFromXml(EntityVehicle vehicle)
		{
			Array motors = MotorsField?.GetValue(vehicle) as Array;
			Array forces = ForcesField?.GetValue(vehicle) as Array;
			if (motors == null || forces == null || motors.Length < 2 || forces.Length < 6)
			{
				Console.WriteLine("[GyroFlightModes] capture skipped motors=" + (motors == null ? "null" : motors.Length.ToString()) + " forces=" + (forces == null ? "null" : forces.Length.ToString()));
				return;
			}

			EnsureEnums();
			originalMotors = new MotorSnapshot[motors.Length];
			for (int i = 0; i < motors.Length; i++)
			{
				originalMotors[i] = MotorSnapshot.From(motors.GetValue(i));
			}

			originalForces = new ForceSnapshot[forces.Length];
			for (int i = 0; i < forces.Length; i++)
			{
				originalForces[i] = ForceSnapshot.From(forces.GetValue(i));
			}

			originalCaptured = true;
			Console.WriteLine("[GyroFlightModes] captured original control table motors=" + motors.Length + " forces=" + forces.Length);
		}

		internal static void Apply(EntityVehicle vehicle, GyroFlightMode mode)
		{
			if (!originalCaptured)
			{
				CaptureOriginalFromXml(vehicle);
			}

			if (!originalCaptured)
			{
				return;
			}

			if (mode == GyroFlightMode.Heli)
			{
				ApplyHeli(vehicle);
			}
			else
			{
				ApplySnapshot(vehicle, originalMotors, originalForces);
			}
		}

		private static void ApplyHeli(EntityVehicle vehicle)
		{
			EnsureEnums();
			if (motorTriggerOn == null || forceTriggerInputForward == null)
			{
				Console.WriteLine("[GyroFlightModes] heli apply aborted, enum resolve failed");
				return;
			}
			Array motors = MotorsField?.GetValue(vehicle) as Array;
			Array forces = ForcesField?.GetValue(vehicle) as Array;
			if (motors == null || forces == null)
			{
				return;
			}

			VPEngine engine = vehicle.GetVehicle()?.FindPart("engine") as VPEngine;

			if (motors.Length > 0)
			{
				object motor0 = motors.GetValue(0);
				SetMotor(motor0, engine, 0.02f, 1f, 0.01f, 0.2f, 8f, 0.993f, motorTriggerOn);
			}

			if (motors.Length > 1)
			{
				object motor1 = motors.GetValue(1);
				SetMotor(motor1, engine, 0f, 1.35f, 0.01f, 0.1f, 8f, 0.993f, motorTriggerOn);
			}

			if (forces.Length > 0)
			{
				SetForce(forces.GetValue(0), forceTriggerMotor0, forceTypeRelative, new Vector3(0f, 0.195f, 0f), new Vector2(280f, 2f));
			}

			if (forces.Length > 1)
			{
				SetForce(forces.GetValue(1), forceTriggerMotor1, forceTypeRelative, Vector3.zero, new Vector2(9999f, 2f));
			}

			if (forces.Length > 2)
			{
				SetForce(forces.GetValue(2), forceTriggerInputForward, forceTypeRelative, new Vector3(0f, 0.03f, 0f), new Vector2(9999f, 2f));
			}

			if (forces.Length > 3)
			{
				SetForce(forces.GetValue(3), forceTriggerInputStrafe, forceTypeRelativeTorque, new Vector3(0f, 0.02f, 0f), new Vector2(9999f, 2f));
			}

			if (forces.Length > 4)
			{
				SetForce(forces.GetValue(4), forceTriggerInputUp, forceTypeRelativeTorque, new Vector3(-0.01f, 0f, 0f), new Vector2(9999f, 2f));
			}

			if (forces.Length > 5)
			{
				SetForce(forces.GetValue(5), forceTriggerInputDown, forceTypeRelativeTorque, new Vector3(0.01f, 0f, 0f), new Vector2(9999f, 2f));
			}

			Console.WriteLine("[GyroFlightModes] applied heli control table");
		}

		private static void ApplySnapshot(EntityVehicle vehicle, MotorSnapshot[] motorsSnap, ForceSnapshot[] forcesSnap)
		{
			Array motors = MotorsField?.GetValue(vehicle) as Array;
			Array forces = ForcesField?.GetValue(vehicle) as Array;
			if (motors == null || forces == null)
			{
				return;
			}

			VPEngine engine = vehicle.GetVehicle()?.FindPart("engine") as VPEngine;
			int motorCount = Math.Min(motors.Length, motorsSnap.Length);
			for (int i = 0; i < motorCount; i++)
			{
				motorsSnap[i].ApplyTo(motors.GetValue(i), engine);
			}

			int forceCount = Math.Min(forces.Length, forcesSnap.Length);
			for (int i = 0; i < forceCount; i++)
			{
				forcesSnap[i].ApplyTo(forces.GetValue(i));
			}
		}

		private static void SetMotor(object motor, VPEngine engine, float engineOffPer, float turbo, float rpmAccelMin, float rpmAccelMax, float rpmMax, float rpmDrag, object trigger)
		{
			if (motor == null)
			{
				return;
			}

			SetField(motor, "engine", engine);
			SetField(motor, "engineOffPer", engineOffPer);
			SetField(motor, "turbo", turbo);
			SetField(motor, "rpmAccelMin", rpmAccelMin);
			SetField(motor, "rpmAccelMax", rpmAccelMax);
			SetField(motor, "rpmMax", rpmMax <= 0f ? 0.001f : rpmMax);
			SetField(motor, "rpm", rpmMax <= 0f ? 0.001f : rpmMax);
			SetField(motor, "rpmDrag", rpmDrag);
			SetField(motor, "trigger", trigger);
		}

		private static void SetForce(object force, object trigger, object type, Vector3 forceVec, Vector2 ceiling)
		{
			if (force == null)
			{
				return;
			}

			SetField(force, "trigger", trigger);
			SetField(force, "type", type);
			SetField(force, "force", forceVec);
			SetField(force, "ceiling", ceiling);
		}

		private static void SetField(object target, string name, object value)
		{
			FieldInfo field = AccessTools.Field(target.GetType(), name);
			field?.SetValue(target, value);
		}

		private static object GetField(object target, string name)
		{
			return AccessTools.Field(target.GetType(), name)?.GetValue(target);
		}

		private static void EnsureEnums()
		{
			if (motorTriggerOn != null)
			{
				return;
			}

			Type motorTrigger = GetNestedEnum(MotorType, "Trigger");
			Type forceTrigger = GetNestedEnum(ForceType, "Trigger");
			Type forceKind = GetNestedEnum(ForceType, "Type");

			motorTriggerOn = ParseEnum(motorTrigger, "On");
			forceTriggerMotor0 = ParseEnum(forceTrigger, "Motor0");
			forceTriggerMotor1 = ParseEnum(forceTrigger, "Motor1");
			forceTriggerInputForward = ParseEnum(forceTrigger, "InputForward");
			forceTriggerInputStrafe = ParseEnum(forceTrigger, "InputStrafe");
			forceTriggerInputUp = ParseEnum(forceTrigger, "InputUp");
			forceTriggerInputDown = ParseEnum(forceTrigger, "InputDown");
			forceTypeRelative = ParseEnum(forceKind, "Relative");
			forceTypeRelativeTorque = ParseEnum(forceKind, "RelativeTorque");
		}

		private static Type GetNestedEnum(Type parent, string name)
		{
			if (parent == null)
			{
				return null;
			}

			return parent.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic);
		}

		private static object ParseEnum(Type enumType, string name)
		{
			if (enumType == null)
			{
				return null;
			}

			try
			{
				return Enum.Parse(enumType, name, ignoreCase: true);
			}
			catch
			{
				return null;
			}
		}

		private struct MotorSnapshot
		{
			public bool BindEngine;
			public float EngineOffPer;
			public float Turbo;
			public float RpmAccelMin;
			public float RpmAccelMax;
			public float RpmMax;
			public float RpmDrag;
			public object Trigger;

			public static MotorSnapshot From(object motor)
			{
				MotorSnapshot snap = default;
				if (motor == null)
				{
					return snap;
				}

				snap.BindEngine = GetField(motor, "engine") != null;
				snap.EngineOffPer = GetFloat(motor, "engineOffPer");
				snap.Turbo = GetFloat(motor, "turbo");
				snap.RpmAccelMin = GetFloat(motor, "rpmAccelMin");
				snap.RpmAccelMax = GetFloat(motor, "rpmAccelMax");
				snap.RpmMax = GetFloat(motor, "rpmMax");
				snap.RpmDrag = GetFloat(motor, "rpmDrag");
				snap.Trigger = GetField(motor, "trigger");
				return snap;
			}

			public void ApplyTo(object motor, VPEngine engine)
			{
				SetMotor(motor, BindEngine ? engine : null, EngineOffPer, Turbo, RpmAccelMin, RpmAccelMax, RpmMax, RpmDrag, Trigger);
			}

			private static float GetFloat(object motor, string name)
			{
				object value = GetField(motor, name);
				return value is float f ? f : 0f;
			}
		}

		private struct ForceSnapshot
		{
			public object Trigger;
			public object Type;
			public Vector3 Force;
			public Vector2 Ceiling;

			public static ForceSnapshot From(object force)
			{
				ForceSnapshot snap = default;
				if (force == null)
				{
					return snap;
				}

				snap.Trigger = GetField(force, "trigger");
				snap.Type = GetField(force, "type");
				object forceVal = GetField(force, "force");
				snap.Force = forceVal is Vector3 v ? v : Vector3.zero;
				object ceilingVal = GetField(force, "ceiling");
				snap.Ceiling = ceilingVal is Vector2 c ? c : new Vector2(9999f, 2f);
				return snap;
			}

			public void ApplyTo(object force)
			{
				SetForce(force, Trigger, Type, Force, Ceiling);
			}
		}
	}
}
