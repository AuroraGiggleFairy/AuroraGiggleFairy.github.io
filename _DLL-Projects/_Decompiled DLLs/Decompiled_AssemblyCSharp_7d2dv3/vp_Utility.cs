using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public static class vp_Utility
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<Type, string> m_TypeAliases = new Dictionary<Type, string>
	{
		{
			typeof(void),
			"void"
		},
		{
			typeof(byte),
			"byte"
		},
		{
			typeof(sbyte),
			"sbyte"
		},
		{
			typeof(short),
			"short"
		},
		{
			typeof(ushort),
			"ushort"
		},
		{
			typeof(int),
			"int"
		},
		{
			typeof(uint),
			"uint"
		},
		{
			typeof(long),
			"long"
		},
		{
			typeof(ulong),
			"ulong"
		},
		{
			typeof(float),
			"float"
		},
		{
			typeof(double),
			"double"
		},
		{
			typeof(decimal),
			"decimal"
		},
		{
			typeof(object),
			"object"
		},
		{
			typeof(bool),
			"bool"
		},
		{
			typeof(char),
			"char"
		},
		{
			typeof(string),
			"string"
		},
		{
			typeof(Vector2),
			"Vector2"
		},
		{
			typeof(Vector3),
			"Vector3"
		},
		{
			typeof(Vector4),
			"Vector4"
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, int> m_UniqueIDs = new Dictionary<int, int>();

	public static bool LockCursor
	{
		get
		{
			return Cursor.lockState == CursorLockMode.Locked;
		}
		set
		{
			Cursor.visible = !value;
			Cursor.lockState = (value ? CursorLockMode.Locked : SoftCursor.DefaultCursorLockState);
		}
	}

	public static int UniqueID
	{
		get
		{
			int num;
			while (true)
			{
				num = UnityEngine.Random.Range(0, 1000000000);
				if (!m_UniqueIDs.ContainsKey(num))
				{
					break;
				}
				if (m_UniqueIDs.Count >= 1000000000)
				{
					ClearUniqueIDs();
					UnityEngine.Debug.LogWarning("Warning (vp_Utility.UniqueID) More than 1 billion unique IDs have been generated. This seems like an awful lot for a game client. Clearing dictionary and starting over!");
				}
			}
			m_UniqueIDs.Add(num, 0);
			return num;
		}
	}

	[Obsolete("Please use 'vp_MathUtility.NaNSafeFloat' instead.")]
	public static float NaNSafeFloat(float value, float prevValue = 0f)
	{
		return vp_MathUtility.NaNSafeFloat(value, prevValue);
	}

	[Obsolete("Please use 'vp_MathUtility.NaNSafeVector2' instead.")]
	public static Vector2 NaNSafeVector2(Vector2 vector, Vector2 prevVector = default(Vector2))
	{
		return vp_MathUtility.NaNSafeVector2(vector, prevVector);
	}

	[Obsolete("Please use 'vp_MathUtility.NaNSafeVector3' instead.")]
	public static Vector3 NaNSafeVector3(Vector3 vector, Vector3 prevVector = default(Vector3))
	{
		return vp_MathUtility.NaNSafeVector3(vector, prevVector);
	}

	[Obsolete("Please use 'vp_MathUtility.NaNSafeQuaternion' instead.")]
	public static Quaternion NaNSafeQuaternion(Quaternion quaternion, Quaternion prevQuaternion = default(Quaternion))
	{
		return vp_MathUtility.NaNSafeQuaternion(quaternion, prevQuaternion);
	}

	[Obsolete("Please use 'vp_MathUtility.SnapToZero' instead.")]
	public static Vector3 SnapToZero(Vector3 value, float epsilon = 0.0001f)
	{
		return vp_MathUtility.SnapToZero(value, epsilon);
	}

	[Obsolete("Please use 'vp_MathUtility.SnapToZero' instead.")]
	public static float SnapToZero(float value, float epsilon = 0.0001f)
	{
		return vp_MathUtility.SnapToZero(value, epsilon);
	}

	[Obsolete("Please use 'vp_MathUtility.ReduceDecimals' instead.")]
	public static float ReduceDecimals(float value, float factor = 1000f)
	{
		return vp_MathUtility.ReduceDecimals(value, factor);
	}

	[Obsolete("Please use 'vp_3DUtility.HorizontalVector' instead.")]
	public static Vector3 HorizontalVector(Vector3 value)
	{
		return vp_3DUtility.HorizontalVector(value);
	}

	public static string GetErrorLocation(int level = 1, bool showOnlyLast = false)
	{
		StackTrace stackTrace = new StackTrace();
		string text = "";
		string text2 = "";
		for (int num = stackTrace.FrameCount - 1; num > level; num--)
		{
			if (num < stackTrace.FrameCount - 1)
			{
				text += " --> ";
			}
			StackFrame frame = stackTrace.GetFrame(num);
			if (frame.GetMethod().DeclaringType.ToString() == text2)
			{
				text = "";
			}
			text2 = frame.GetMethod().DeclaringType.ToString();
			text = text + text2 + ":" + frame.GetMethod().Name;
		}
		if (showOnlyLast)
		{
			try
			{
				text = text.Substring(text.LastIndexOf(" --> "));
				text = text.Replace(" --> ", "");
			}
			catch
			{
			}
		}
		return text;
	}

	public static string GetTypeAlias(Type type)
	{
		string value = "";
		if (!m_TypeAliases.TryGetValue(type, out value))
		{
			return type.ToString();
		}
		return value;
	}

	public static void Activate(GameObject obj, bool activate = true)
	{
		obj.SetActive(activate);
	}

	public static bool IsActive(GameObject obj)
	{
		return obj.activeSelf;
	}

	public static void RandomizeList<T>(this List<T> list)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			int index = UnityEngine.Random.Range(i, count);
			T value = list[i];
			list[i] = list[index];
			list[index] = value;
		}
	}

	public static T RandomObject<T>(this List<T> list)
	{
		List<T> list2 = new List<T>();
		list2.AddRange(list);
		list2.RandomizeList();
		return list2.FirstOrDefault();
	}

	public static List<T> ChildComponentsToList<T>(this Transform t) where T : Component
	{
		return t.GetComponentsInChildren<T>().ToList();
	}

	public static bool IsDescendant(Transform descendant, Transform potentialAncestor)
	{
		if (descendant == null)
		{
			return false;
		}
		if (potentialAncestor == null)
		{
			return false;
		}
		if (descendant.parent == descendant)
		{
			return false;
		}
		if (descendant.parent == potentialAncestor)
		{
			return true;
		}
		return IsDescendant(descendant.parent, potentialAncestor);
	}

	public static Component GetParent(Component target)
	{
		if (target == null)
		{
			return null;
		}
		if (target != target.transform)
		{
			return target.transform;
		}
		return target.transform.parent;
	}

	public static Transform GetTransformByNameInChildren(Transform trans, string name, bool includeInactive = false, bool subString = false)
	{
		name = name.ToLower();
		foreach (Transform tran in trans)
		{
			if (!subString)
			{
				if (tran.name.ToLower() == name && (includeInactive || tran.gameObject.activeInHierarchy))
				{
					return tran;
				}
			}
			else if (tran.name.ToLower().Contains(name) && (includeInactive || tran.gameObject.activeInHierarchy))
			{
				return tran;
			}
			Transform transformByNameInChildren = GetTransformByNameInChildren(tran, name, includeInactive, subString);
			if (transformByNameInChildren != null)
			{
				return transformByNameInChildren;
			}
		}
		return null;
	}

	public static Transform GetTransformByNameInAncestors(Transform trans, string name, bool includeInactive = false, bool subString = false)
	{
		if (trans.parent == null)
		{
			return null;
		}
		name = name.ToLower();
		if (!subString)
		{
			if (trans.parent.name.ToLower() == name && (includeInactive || trans.gameObject.activeInHierarchy))
			{
				return trans.parent;
			}
		}
		else if (trans.parent.name.ToLower().Contains(name) && (includeInactive || trans.gameObject.activeInHierarchy))
		{
			return trans.parent;
		}
		Transform transformByNameInAncestors = GetTransformByNameInAncestors(trans.parent, name, includeInactive, subString);
		if (transformByNameInAncestors != null)
		{
			return transformByNameInAncestors;
		}
		return null;
	}

	public static UnityEngine.Object Instantiate(UnityEngine.Object original)
	{
		return Instantiate(original, Vector3.zero, Quaternion.identity);
	}

	public static UnityEngine.Object Instantiate(UnityEngine.Object original, Vector3 position, Quaternion rotation)
	{
		if (vp_PoolManager.Instance == null || !vp_PoolManager.Instance.enabled)
		{
			return UnityEngine.Object.Instantiate(original, position, rotation);
		}
		return vp_GlobalEventReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>.Send("vp_PoolManager Instantiate", original, position, rotation);
	}

	public static void Destroy(UnityEngine.Object obj)
	{
		Destroy(obj, 0f);
	}

	public static void Destroy(UnityEngine.Object obj, float t)
	{
		if (vp_PoolManager.Instance == null || !vp_PoolManager.Instance.enabled)
		{
			UnityEngine.Object.Destroy(obj, t);
		}
		else
		{
			vp_GlobalEvent<UnityEngine.Object, float>.Send("vp_PoolManager Destroy", obj, t);
		}
	}

	public static void ClearUniqueIDs()
	{
		m_UniqueIDs.Clear();
	}

	public static int PositionToID(Vector3 position)
	{
		return (int)Mathf.Abs(position.x * 10000f + position.y * 1000f + position.z * 100f);
	}

	[Obsolete("Please use 'vp_AudioUtility.PlayRandomSound' instead.")]
	public static void PlayRandomSound(AudioSource audioSource, List<AudioClip> sounds, Vector2 pitchRange)
	{
		vp_AudioUtility.PlayRandomSound(audioSource, sounds, pitchRange);
	}

	[Obsolete("Please use 'vp_AudioUtility.PlayRandomSound' instead.")]
	public static void PlayRandomSound(AudioSource audioSource, List<AudioClip> sounds)
	{
		vp_AudioUtility.PlayRandomSound(audioSource, sounds);
	}
}
