using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[UsedByNativeCode]
public sealed class TreePrototype
{
	internal GameObject m_Prefab;

	internal float m_BendFactor;

	internal int m_NavMeshLod;

	public GameObject prefab
	{
		get
		{
			return m_Prefab;
		}
		set
		{
			m_Prefab = value;
		}
	}

	public float bendFactor
	{
		get
		{
			return m_BendFactor;
		}
		set
		{
			m_BendFactor = value;
		}
	}

	public int navMeshLod
	{
		get
		{
			return m_NavMeshLod;
		}
		set
		{
			m_NavMeshLod = value;
		}
	}

	public TreePrototype()
	{
	}

	public TreePrototype(TreePrototype other)
	{
		prefab = other.prefab;
		bendFactor = other.bendFactor;
		navMeshLod = other.navMeshLod;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TreePrototype);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private bool Equals(TreePrototype other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GetType() != other.GetType())
		{
			return false;
		}
		return prefab == other.prefab && bendFactor == other.bendFactor && navMeshLod == other.navMeshLod;
	}

	internal bool Validate(out string errorMessage)
	{
		return ValidateTreePrototype(this, out errorMessage);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::ValidateTreePrototype")]
	internal static extern bool ValidateTreePrototype([NotNull("ArgumentNullException")] TreePrototype prototype, out string errorMessage);
}
