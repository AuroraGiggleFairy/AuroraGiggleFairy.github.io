using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAITerritorial : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 movePos;

	public EAITerritorial()
	{
		MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		if (theEntity.isWithinHomeDistanceCurrentPosition())
		{
			return false;
		}
		ChunkCoordinates homePosition = theEntity.getHomePosition();
		Vector3 vector = RandomPositionGenerator.CalcTowards(theEntity, 5, 15, 7, homePosition.position.ToVector3());
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		movePos = vector;
		return true;
	}

	public override bool Continue()
	{
		return !theEntity.getNavigator().noPathAndNotPlanningOne();
	}

	public override void Start()
	{
		theEntity.FindPath(movePos, theEntity.GetMoveSpeed(), canBreak: false, this);
	}
}
