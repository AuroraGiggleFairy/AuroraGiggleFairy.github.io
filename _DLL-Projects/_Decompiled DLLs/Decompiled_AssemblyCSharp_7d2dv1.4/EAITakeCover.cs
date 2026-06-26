using System.Collections.Generic;
using ExtUtilsForEnt;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAITakeCover : EAIBase
{
	public enum State
	{
		Idle,
		FindPath,
		PreProcessPath,
		ProcessPath,
		Empty
	}

	[Preserve]
	public class CoverNode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public List<CoverNode> neighbors = new List<CoverNode>();

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Vector3i BlockPos { get; }

		public CoverNode(Vector3 _pos)
		{
			BlockPos = new Vector3i(_pos);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class PosData
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public Vector3 Dir
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public float Dist
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public float Cover
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public PosData(Vector3 _dir, float _dist, float _cover = 0.5f)
		{
			Dir = _dir;
			Dist = _dist;
			Cover = _cover;
		}
	}

	[Preserve]
	public class CoverCastInfo
	{
		public Vector3 Pos;

		public Vector3 Dir;

		public Vector3 HitPoint;

		public float ThreatDistance;

		public CoverCastInfo(Vector3 _pos, Vector3 _dir, Vector3 _hitPoint, float _threatDist)
		{
			Set(_pos, _dir, _hitPoint, _threatDist);
		}

		public void Set(Vector3 _pos, Vector3 _dir, Vector3 _hitPoint, float _threatDist)
		{
			Pos = _pos;
			Dir = _dir;
			HitPoint = _hitPoint;
			ThreatDistance = _threatDist;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int timeoutTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int coverTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeDistance = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 halfBlockOffset = Vector3.one * 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathFinderThread pathFinder;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathInfo pathInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPathing;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> currentPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pathEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager ecm;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool targetViewBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive threatTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float retryPathTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public State state;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCoverDist = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool findingPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopSeekingCover;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] mainBlockAxis = new Vector3[8]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, 0.5f)
	};

	public EAITakeCover()
	{
		MutexBits = 1;
		World world = GameManager.Instance.World;
		if (world != null)
		{
			this.world = world;
		}
		PathFinderThread instance = PathFinderThread.Instance;
		if (instance != null)
		{
			pathFinder = instance;
		}
		ecm = EntityCoverManager.Instance;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		if (!EntityCoverManager.DebugModeEnabled)
		{
			return false;
		}
		if (theEntity.sleepingOrWakingUp || theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (theEntity.Jumping && !theEntity.isSwimming))
		{
			return false;
		}
		EntityAlive attackTarget = theEntity.GetAttackTarget();
		if ((bool)attackTarget)
		{
			threatTarget = attackTarget;
		}
		if (threatTarget == null)
		{
			return false;
		}
		if (stopSeekingCover)
		{
			return false;
		}
		if (theEntity.Health < theEntity.GetMaxHealth() && Vector3.Distance(theEntity.position, threatTarget.position) > 5f)
		{
			return true;
		}
		return false;
	}

	public override void Start()
	{
		timeoutTicks = 800;
		retryPathTicks = 60f;
		fleeTicks = 0;
		PathFinderThread.Instance.RemovePathsFor(theEntity.entityId);
		stopSeekingCover = false;
	}

	public override bool Continue()
	{
		if (theEntity.Health < theEntity.GetMaxHealth() && Vector3.Distance(theEntity.position, threatTarget.position) < 3f)
		{
			return false;
		}
		if (stopSeekingCover)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setState(State _state)
	{
		state = _state;
	}

	public override void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || world == null)
		{
			return;
		}
		EntityAlive attackTarget = theEntity.GetAttackTarget();
		if ((bool)attackTarget)
		{
			threatTarget = attackTarget;
		}
		if (threatTarget == null || updateCover())
		{
			return;
		}
		switch (state)
		{
		case State.Idle:
			if (!findingPath)
			{
				findingPath = true;
				setState(State.FindPath);
			}
			break;
		case State.FindPath:
			if (!pathFinder.IsCalculatingPath(theEntity.entityId))
			{
				Vector3 vector = findCoverDir();
				pathFinder.RemovePathsFor(theEntity.entityId);
				pathFinder.FindPath(theEntity, threatTarget.getHipPosition() + vector * 10f * 2f, theEntity.moveSpeedAggro, _canBreak: false, this);
				setState(State.PreProcessPath);
			}
			break;
		case State.PreProcessPath:
		{
			pathInfo = pathFinder.GetPath(theEntity.entityId);
			if (pathFinder.IsCalculatingPath(theEntity.entityId) || pathInfo?.path == null)
			{
				break;
			}
			currentPath.Clear();
			bool flag = false;
			int num = 0;
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < pathInfo.path.points.Length; i++)
			{
				Vector3 projectedLocation = pathInfo.path.points[i].projectedLocation;
				currentPath.Add(projectedLocation);
				Vector3 vector2 = matchHipHeight(projectedLocation);
				List<CoverCastInfo> bestCoverDirection = getBestCoverDirection(vector2, threatTarget.getHipPosition(), 10f);
				Vector3 dir = Vector3.zero;
				Vector3 v = Vector3.zero;
				if (bestCoverDirection.Count > 0)
				{
					dir = bestCoverDirection[0].Dir;
					v = bestCoverDirection[0].HitPoint;
				}
				Vector3 vector3 = new Vector3i(v).ToVector3CenterXZ();
				if (!EUtils.isPositionBlocked(vector2, threatTarget.getChestPosition(), 65536) || !(vector3 != pathEnd))
				{
					continue;
				}
				list.Add(vector3);
				if (num > 3 || i >= pathInfo.path.points.Length - 1)
				{
					int index = 0;
					float num2 = float.MaxValue;
					for (int j = 0; j < list.Count; j++)
					{
						EUtils.DrawBounds(new Vector3i(list[j]), Color.red * Color.yellow * 0.5f, 10f);
						float num3 = Vector3.Distance(list[j], theEntity.position);
						if (num3 < num2 && EUtils.isPositionBlocked(list[j], threatTarget.getChestPosition(), 65536) && ecm.IsFree(list[j]))
						{
							index = j;
							num2 = num3;
						}
					}
					Vector3 vector4 = list[index];
					pathEnd = new Vector3i(vector4).ToVector3CenterXZ();
					ecm.AddCover(pathEnd, dir);
					ecm.MarkReserved(theEntity.entityId, pathEnd);
					EUtils.DrawLine(vector2, vector4, Color.red, 10f);
					EUtils.DrawBounds(new Vector3i(vector4), Color.green, 10f);
					pathFinder.FindPath(theEntity, theEntity.position, pathEnd, theEntity.moveSpeedAggro, _canBreak: false, this);
					flag = true;
					break;
				}
				num++;
			}
			if (flag && currentPath.Count > 0)
			{
				EUtils.DrawPath(currentPath, Color.white, Color.yellow);
				setState(State.ProcessPath);
			}
			else
			{
				freeCover();
				retryPathTicks = 60f;
				setState(State.FindPath);
			}
			break;
		}
		case State.ProcessPath:
			if (retryPathTicks > 0f)
			{
				retryPathTicks -= 1f;
				if (retryPathTicks <= 0f)
				{
					freeCover();
					retryPathTicks = 60f;
					setState(State.FindPath);
					break;
				}
			}
			if (currentPath.Count > 0)
			{
				if (Vector3.Distance(theEntity.position, pathEnd) < 0.5f)
				{
					pathFinder.RemovePathsFor(theEntity.entityId);
					theEntity.SetLookPosition(threatTarget.getHeadPosition());
					ecm.UseCover(theEntity.entityId, pathEnd);
					theEntity.navigator.clearPath();
					theEntity.moveHelper.Stop();
					coverTicks = 20 * base.Random.RandomRange(4);
					findingPath = false;
					setState(State.Idle);
				}
			}
			else
			{
				freeCover();
				retryPathTicks = 60f;
				setState(State.FindPath);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateCover()
	{
		if (!ecm.HasCover(theEntity.entityId))
		{
			return false;
		}
		if (ecm.GetCoverPos(theEntity.entityId) == null)
		{
			return false;
		}
		if (coverTicks > 0)
		{
			coverTicks--;
			if (coverTicks <= 0)
			{
				if (base.Random.RandomRange(2) < 1)
				{
					freeCover();
					if (base.Random.RandomRange(2) < 1)
					{
						stopSeekingCover = true;
					}
				}
				else
				{
					coverTicks = 60;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeCover()
	{
		ecm.FreeCover(theEntity.entityId);
		coverTicks = 60;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addCover(Vector3 pos, Vector3 dir, bool debugDraw = false)
	{
		if (debugDraw)
		{
			EUtils.DrawBounds(new Vector3i(pos), Color.cyan, 10f);
		}
		ecm.AddCover(pos, dir);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 matchHipHeight(Vector3 point)
	{
		float y = theEntity.getHipPosition().y;
		Vector3 result = point;
		result.y = y;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCrouching(bool value)
	{
		theEntity.Crouching = value;
		theEntity.GetComponentInChildren<Animator>().SetBool("IsCrouching", value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToDir(Vector3 dir)
	{
		return Quaternion.Lerp(theEntity.transform.rotation, Quaternion.LookRotation(dir), (1f - Vector3.Angle(theEntity.transform.forward, dir) / 180f) * 7f * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findCoverDir(bool debugDraw = false)
	{
		RaycastHit hit;
		Vector3 vector = getBestCoverDirection(threatTarget.getHipPosition(), 10f, out hit, debugDraw);
		if (vector == Vector3.zero)
		{
			vector = (theEntity.position - threatTarget.position).normalized;
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 getBestCoverDirection(Vector3 point, float dist, out RaycastHit hit, bool debugDraw = false)
	{
		List<PosData> list = new List<PosData>();
		hit = default(RaycastHit);
		for (int i = 0; i < mainBlockAxis.Length; i++)
		{
			if (EUtils.isPositionBlocked(point, point + mainBlockAxis[i] * dist, out hit, 65536, debugDraw))
			{
				Vector3 vector = new Vector3(0f, 0.5f, 0f);
				RaycastHit hit2 = default(RaycastHit);
				float cover = 0.5f;
				if (EUtils.isPositionBlocked(point + vector, point + vector + mainBlockAxis[i] * dist, out hit2, 65536, debugDraw))
				{
					cover = 1f;
				}
				list.Add(new PosData(mainBlockAxis[i], hit.distance, cover));
			}
		}
		float num = float.MaxValue;
		int index = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Dist < num)
			{
				num = list[j].Dist;
				index = j;
			}
		}
		if (list.Count > 0)
		{
			if (debugDraw)
			{
				EUtils.DrawLine(point, point + list[0].Dir * dist, Color.green, 10f);
			}
			return list[index].Dir;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CoverCastInfo> getBestCoverDirection(Vector3 point, Vector3 target, float dist, bool debugDraw = false)
	{
		List<CoverCastInfo> list = new List<CoverCastInfo>();
		Vector3 vector = new Vector3i(point).ToVector3Center();
		vector.y += 0.15f;
		for (int i = 0; i < mainBlockAxis.Length; i++)
		{
			Vector3 vector2 = new Vector3i(mainBlockAxis[i] * dist) - halfBlockOffset;
			if (EUtils.isPositionBlocked(vector, vector + vector2, out var hit, 65536, debugDraw))
			{
				list.Add(new CoverCastInfo(vector, mainBlockAxis[i], hit.point + Origin.position + hit.normal * 0.1f, Vector3.Distance(hit.point + Origin.position, target)));
			}
		}
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (CoverCastInfo x, CoverCastInfo y) => x.ThreatDistance.CompareTo(y.ThreatDistance));
		return list;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, state {state}, coverTicks {coverTicks}";
	}
}
