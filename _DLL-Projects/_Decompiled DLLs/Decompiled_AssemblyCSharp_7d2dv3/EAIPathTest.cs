using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIPathTest : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityBandit banditEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetWorldPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive targetYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool newTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canBreakBlocks;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 1;
		banditEntity = _theEntity as EntityBandit;
	}

	public void SetTargetMove(Vector3 _targetWorld, EntityAlive _targetYaw, bool _canBreakBlocks)
	{
		if (banditEntity != null)
		{
			targetWorldPosition = _targetWorld;
			targetYaw = _targetYaw;
			newTarget = true;
			cancel = false;
			canBreakBlocks = _canBreakBlocks;
		}
	}

	public void CancelTargetMove()
	{
		targetWorldPosition = Vector3.zero;
		newTarget = false;
		cancel = true;
	}

	public override bool CanExecute()
	{
		return newTarget;
	}

	public override bool Continue()
	{
		return !cancel;
	}

	public override void Update()
	{
		base.Update();
		if (newTarget)
		{
			newTarget = false;
			PathInfoSingleTarget pathInfo = new PathInfoSingleTarget(theEntity, targetWorldPosition, canBreakBlocks, theEntity.GetMoveSpeed(), null);
			PathFinderThread.Instance.FindPath(pathInfo);
			if (banditEntity != null)
			{
				banditEntity.focusBody.SetFocus(FocusPriority.Highest, new AIFocusBody(targetYaw)
				{
					ConditionDistance = new AIFocusConditionDistance(5f, targetWorldPosition)
				});
			}
		}
		Vector3 vector = targetWorldPosition - Origin.position;
		Debug.DrawLine(vector, vector + Vector3.up * 2f, Color.blue);
		PathEntity path = theEntity.navigator.getPath();
		if (path == null)
		{
			return;
		}
		for (int i = 0; i < path.getCurrentPathLength() - 1; i++)
		{
			Vector3 startPos = path.getPathPointFromIndex(i).projectedLocation - Origin.position;
			Color color = new Color(1f, 1f, 0.1f);
			if (i < path.getCurrentPathIndex())
			{
				color = new Color(0.3f, 0.3f, 0f);
			}
			Utils.DrawLine(startPos, path.getPathPointFromIndex(i + 1).projectedLocation - Origin.position, color, color * 0.5f, 3, 0.1f);
		}
	}
}
