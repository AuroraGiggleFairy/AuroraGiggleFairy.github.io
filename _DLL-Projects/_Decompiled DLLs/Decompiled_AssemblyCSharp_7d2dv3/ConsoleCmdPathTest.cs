using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePath;
using Pathfinding;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPathTest : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine executionsCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAIPathTest aiPathTest;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool recalculatePath = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canBreakBlocks = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canClimbLadders = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canClimbWalls = false;

	public override bool IsExecuteOnClient => false;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "pathtest" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Usage. Toggle a path test mode.");
		stringBuilder.AppendLine(" breakblocks - toggles path allowed to break blocks");
		stringBuilder.AppendLine(" climbladders - toggles path allowed to break blocks");
		stringBuilder.AppendLine(" climbwalls - toggles path allowed to break blocks");
		return stringBuilder.ToString();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (_params.Count > 0)
		{
			foreach (string _param in _params)
			{
				string text = _param.ToLower();
				bool on = text.StartsWith("+");
				bool off = text.StartsWith("-");
				if (on || off)
				{
					text = text.Substring(1);
				}
				Func<bool, bool> func = [PublicizedFrom(EAccessModifier.Internal)] (bool currentValue) =>
				{
					if (on)
					{
						return true;
					}
					return !off && !currentValue;
				};
				if (string.Equals(text, "breakblocks", StringComparison.OrdinalIgnoreCase))
				{
					canBreakBlocks = func(canBreakBlocks);
				}
				else if (string.Equals(text, "climbladders", StringComparison.OrdinalIgnoreCase))
				{
					canClimbLadders = func(canClimbLadders);
				}
				else if (string.Equals(text, "climbwalls", StringComparison.OrdinalIgnoreCase))
				{
					canClimbWalls = func(canClimbWalls);
				}
			}
		}
		if (executionsCoroutine != null)
		{
			aiPathTest?.CancelTargetMove();
			GameManager.Instance.StopCoroutine(executionsCoroutine);
			executionsCoroutine = null;
		}
		else
		{
			executionsCoroutine = GameManager.Instance.StartCoroutine(ExecutePathTestCoroutine());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ExecutePathTestCoroutine()
	{
		EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
		aiPathTest = null;
		string bodyPartName;
		EntityAlive entityAlive = ItemActionAttack.FindHitEntityNoTagCheck(player.HitInfo, out bodyPartName) as EntityAlive;
		if (entityAlive != null)
		{
			aiPathTest = entityAlive.aiManager?.GetTasks<EAIPathTest>()?.FirstOrDefault();
		}
		PathInfoSingleTarget pathInfo = new PathInfoSingleTarget(player, Vector3.zero, _canBreakBlocks: false, 1f, null);
		ASPPathFinder pathFinder = new ASPPathFinder(pathInfo, _bDrn: false, _canClimbLadders: false, _bCanClimbWalls: false);
		GraphNode[] pathNodes = null;
		pathInfo.state = PathInfo.State.Done;
		pathInfo.OnPathResult = [PublicizedFrom(EAccessModifier.Internal)] (Path p) =>
		{
			pathNodes = p.path?.ToArray();
		};
		while (true)
		{
			if (pathInfo.canBreakBlocks == canBreakBlocks && pathFinder.canClimbLadders == canClimbLadders)
			{
				_ = pathFinder.canClimbLadders != canClimbLadders;
			}
			pathInfo.canBreakBlocks = canBreakBlocks;
			pathFinder.canClimbLadders = canClimbLadders;
			pathFinder.canClimbWalls = canClimbWalls;
			WorldRayHitInfo hitInfo = player.HitInfo;
			if (hitInfo.bHitValid)
			{
				if (Input.GetMouseButton(2))
				{
					pathInfo.targetPos = World.blockToTransformPos(hitInfo.hit.blockPos);
					recalculatePath = true;
				}
				else if (Input.GetMouseButton(3))
				{
					pathInfo.hasStart = !pathInfo.hasStart;
					pathInfo.startPos = World.blockToTransformPos(hitInfo.hit.blockPos);
					recalculatePath = true;
				}
			}
			if (recalculatePath)
			{
				recalculatePath = false;
				if (aiPathTest != null)
				{
					pathNodes = null;
					aiPathTest.SetTargetMove(pathInfo.targetPos, player, canBreakBlocks);
				}
				else if (pathInfo.state == PathInfo.State.Done)
				{
					recalculatePath = false;
					pathFinder.Calculate(pathInfo.hasStart ? pathInfo.startPos : player.position);
				}
			}
			if (pathNodes != null)
			{
				for (int num = 1; num < pathNodes.Length; num++)
				{
					AstarVoxelGrid.VoxelNode obj = (AstarVoxelGrid.VoxelNode)pathNodes[num - 1];
					AstarVoxelGrid.VoxelNode voxelNode = (AstarVoxelGrid.VoxelNode)pathNodes[num];
					Vector3 start = (Vector3)obj.position;
					Vector3 end = (Vector3)voxelNode.position;
					uint num2 = Math.Max(obj.Tag, voxelNode.Tag);
					Color color = Color.white;
					switch (num2)
					{
					case 0u:
						color = Color.green;
						break;
					case 1u:
						color = Color.grey;
						break;
					case 2u:
						color = Color.yellow;
						break;
					case 3u:
						color = Color.blue;
						break;
					case 4u:
						color = Color.cyan;
						break;
					}
					Debug.DrawLine(start, end, color, 0.11f);
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enable a path testing utility mode";
	}
}
