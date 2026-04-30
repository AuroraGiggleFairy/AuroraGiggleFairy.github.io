using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
public struct PatchExtents
{
	internal float m_min;

	internal float m_max;

	public float min
	{
		get
		{
			return m_min;
		}
		set
		{
			m_min = value;
		}
	}

	public float max
	{
		get
		{
			return m_max;
		}
		set
		{
			m_max = value;
		}
	}
}
