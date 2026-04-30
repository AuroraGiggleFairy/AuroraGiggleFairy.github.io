using System.Collections.Generic;
using ExtUtilsForEnt;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityCoverManager
{
	[Preserve]
	public class CoverPos
	{
		public Vector3 BlockPos;

		public Vector3 CoverDir;

		public float TimeCreated;

		public bool Reserved;

		public bool InUse;

		public CoverPos(Vector3 _pos, Vector3 _coverDir, float _timeCreated)
		{
			BlockPos = _pos;
			CoverDir = _coverDir;
			TimeCreated = _timeCreated;
		}
	}

	public static bool DebugModeEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityCoverManager instance;

	public Dictionary<int, CoverPos> CoverDic = new Dictionary<int, CoverPos>();

	public List<CoverPos> CoverPoints = new List<CoverPos>();

	public static EntityCoverManager Instance => instance;

	public static void Init()
	{
		instance = new EntityCoverManager();
		instance.Load();
	}

	public void Clear()
	{
		CoverDic.Clear();
		CoverPoints.Clear();
	}

	public void Clear(EntityAlive entity, float dist)
	{
		foreach (KeyValuePair<int, CoverPos> item in CoverDic)
		{
			if (Vector3.Distance(item.Value.BlockPos, entity.position) > dist)
			{
				CoverDic.Remove(item.Key);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load()
	{
	}

	public void Update()
	{
		DrawCoverPoints();
	}

	public bool HasCover(int entityId)
	{
		CoverPos value = null;
		if (CoverDic.TryGetValue(entityId, out value) && value.InUse)
		{
			return true;
		}
		return false;
	}

	public bool HasCoverReserved(int entityId)
	{
		CoverPos value = null;
		if (CoverDic.TryGetValue(entityId, out value) && (value.Reserved || value.InUse))
		{
			return true;
		}
		return false;
	}

	public bool IsFree(Vector3 coverPos)
	{
		foreach (KeyValuePair<int, CoverPos> item in CoverDic)
		{
			if (item.Value.BlockPos == coverPos)
			{
				return false;
			}
		}
		return true;
	}

	public CoverPos AddCover(Vector3 pos, Vector3 dir)
	{
		if (CoverPoints.Find([PublicizedFrom(EAccessModifier.Internal)] (CoverPos c) => c.BlockPos == pos) == null)
		{
			CoverPos coverPos = new CoverPos(pos, dir, Time.time);
			CoverPoints.Add(coverPos);
			return coverPos;
		}
		return null;
	}

	public CoverPos GetCoverPos(int entityId)
	{
		CoverPos value = null;
		CoverDic.TryGetValue(entityId, out value);
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public CoverPos GetCover(Vector3 pos)
	{
		return CoverPoints.Find([PublicizedFrom(EAccessModifier.Internal)] (CoverPos c) => c.BlockPos == pos);
	}

	public bool MarkReserved(int entityId, Vector3 pos)
	{
		if (!CoverDic.ContainsKey(entityId))
		{
			CoverPos cover = GetCover(pos);
			if (cover != null)
			{
				cover.Reserved = true;
				CoverDic.Add(entityId, cover);
				return true;
			}
		}
		return false;
	}

	public bool UseCover(int entityId, Vector3 pos)
	{
		CoverPos value = GetCover(pos);
		if (!CoverDic.ContainsKey(entityId))
		{
			if (value != null)
			{
				value.InUse = true;
				CoverDic.Add(entityId, value);
				return true;
			}
		}
		else if (CoverDic.TryGetValue(entityId, out value))
		{
			value.InUse = true;
			return true;
		}
		return false;
	}

	public void FreeCover(int entityId)
	{
		CoverPos value = null;
		if (CoverDic.TryGetValue(entityId, out value))
		{
			CoverDic.Remove(entityId);
		}
	}

	public void DrawCoverPoints()
	{
		for (int i = 0; i < CoverPoints.Count; i++)
		{
			CoverPos coverPos = CoverPoints[i];
			EUtils.DrawBounds(new Vector3i(coverPos.BlockPos), Color.yellow, 1f);
			EUtils.DrawLine(coverPos.BlockPos, coverPos.BlockPos + coverPos.CoverDir, Color.blue);
		}
	}
}
