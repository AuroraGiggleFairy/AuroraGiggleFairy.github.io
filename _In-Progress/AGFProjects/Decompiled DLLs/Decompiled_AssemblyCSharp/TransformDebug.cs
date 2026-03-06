using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class TransformDebug : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class TransformState
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Quaternion zeroQuaternion = new Quaternion(0f, 0f, 0f, 0f);

		public Vector3 Position = Vector3.zero;

		public Quaternion Rotation = Quaternion.identity;

		public Vector3 Scale = Vector3.one;

		public void Update(Transform t)
		{
			Vector3 localPosition = t.localPosition;
			Position = new Vector3(IsValid(localPosition.x) ? localPosition.x : Position.x, IsValid(localPosition.y) ? localPosition.y : Position.y, IsValid(localPosition.z) ? localPosition.z : Position.z);
			Quaternion localRotation = t.localRotation;
			Rotation = new Quaternion(IsValid(localRotation.x) ? localRotation.x : Rotation.x, IsValid(localRotation.y) ? localRotation.y : Rotation.y, IsValid(localRotation.z) ? localRotation.z : Rotation.z, IsValid(localRotation.w) ? localRotation.w : Rotation.w);
			if (Rotation == zeroQuaternion)
			{
				Rotation = Quaternion.identity;
			}
			Vector3 localScale = t.localScale;
			Scale = new Vector3(IsValid(localScale.x) ? localScale.x : Scale.x, IsValid(localScale.y) ? localScale.y : Scale.y, IsValid(localScale.z) ? localScale.z : Scale.z);
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_OPERATIONS_PER_FRAME = 256;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ConditionalWeakTable<Transform, TransformState> m_states = new ConditionalWeakTable<Transform, TransformState>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Stack<Transform> m_stack = new Stack<Transform>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Stack<string> m_describeStack = new Stack<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder m_describeBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		try
		{
			if (m_stack.Count <= 0)
			{
				GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
				for (int num = rootGameObjects.Length - 1; num >= 0; num--)
				{
					GameObject gameObject = rootGameObjects[num];
					if ((bool)gameObject)
					{
						PushTransform(gameObject.transform);
					}
				}
			}
			for (int i = 0; i < 256; i++)
			{
				if (m_stack.Count <= 0)
				{
					break;
				}
				DoOperation();
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PushTransform(Transform t)
	{
		if ((bool)t)
		{
			m_stack.Push(t);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoOperation()
	{
		Transform transform = m_stack.Pop();
		if (!transform)
		{
			return;
		}
		TransformState orCreateValue = m_states.GetOrCreateValue(transform);
		orCreateValue.Update(transform);
		string text = null;
		if (transform.localPosition != orCreateValue.Position)
		{
			text = DescribeTransformHierarchy(transform);
			Log.Error(string.Format("[{0}] Invalid local position {1} correcting to {2}: {3}", "TransformDebug", transform.localPosition, orCreateValue.Position, text));
			transform.localPosition = orCreateValue.Position;
		}
		if (transform.localRotation != orCreateValue.Rotation)
		{
			if (text == null)
			{
				text = DescribeTransformHierarchy(transform);
			}
			Log.Error(string.Format("[{0}] Invalid local rotation {1} correcting to {2}: {3}", "TransformDebug", transform.localRotation, orCreateValue.Rotation, text));
			transform.localRotation = orCreateValue.Rotation;
		}
		if (transform.localScale != orCreateValue.Scale)
		{
			if (text == null)
			{
				text = DescribeTransformHierarchy(transform);
			}
			Log.Error(string.Format("[{0}] Invalid local scale {1} correcting to {2}: {3}", "TransformDebug", transform.localScale, orCreateValue.Scale, text));
			transform.localScale = orCreateValue.Scale;
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			PushTransform(child);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DescribeTransformHierarchy(Transform t)
	{
		try
		{
			m_describeStack.Push(t.name);
			Transform transform = t;
			while ((bool)transform.parent)
			{
				transform = transform.parent;
				m_describeStack.Push(transform.name);
			}
			while (m_describeStack.Count > 0)
			{
				if (m_describeBuilder.Length > 0)
				{
					m_describeBuilder.Append('/');
				}
				m_describeBuilder.Append(m_describeStack.Pop());
			}
			return m_describeBuilder.ToString();
		}
		finally
		{
			m_describeStack.Clear();
			m_describeBuilder.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsValid(float f)
	{
		return float.IsFinite(f);
	}

	public static void Test()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		Stack<Transform> stack = new Stack<Transform>();
		GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			if (!gameObject || !gameObject.name.EqualsCaseInsensitive("Entities"))
			{
				continue;
			}
			foreach (Transform item3 in gameObject.transform)
			{
				stack.Push(item3);
			}
		}
		if (stack.Count == 0)
		{
			Log.Error("No entities found (try testing while in-game).");
		}
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			if (!transform)
			{
				continue;
			}
			transform.localPosition = new Vector3(MaybeCorruptFloat(transform.localPosition.x), MaybeCorruptFloat(transform.localPosition.y), MaybeCorruptFloat(transform.localPosition.z));
			transform.localRotation = new Quaternion(MaybeCorruptFloat(transform.localRotation.x), MaybeCorruptFloat(transform.localRotation.y), MaybeCorruptFloat(transform.localRotation.z), MaybeCorruptFloat(transform.localRotation.w));
			transform.localScale = new Vector3(MaybeCorruptFloat(transform.localScale.x), MaybeCorruptFloat(transform.localScale.y), MaybeCorruptFloat(transform.localScale.z));
			foreach (Transform item4 in transform)
			{
				stack.Push(item4);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static float MaybeCorruptFloat(float originalValue)
		{
			if (UnityEngine.Random.value >= 0.05f)
			{
				return originalValue;
			}
			float value = UnityEngine.Random.value;
			if (value < 1f / 3f)
			{
				return float.NegativeInfinity;
			}
			if (value < 2f / 3f)
			{
				return float.PositiveInfinity;
			}
			return float.NaN;
		}
	}
}
