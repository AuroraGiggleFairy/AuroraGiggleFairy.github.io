using System.Collections.Generic;

public class EntityGroupSpawnState
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct State(SEntityClassAndProb _src)
	{
		public readonly int entityClassId = _src.entityClassId;

		public readonly float prob = _src.prob;

		public int numSpawned = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<State> state = new List<State>();

	public EntityGroupSpawnState(string _sEntityGroupName)
	{
		List<SEntityClassAndProb> list = EntityGroups.list[_sEntityGroupName];
		for (int i = 0; i < list.Count; i++)
		{
			state.Add(new State(list[i]));
		}
	}

	public int GetRandomFromGroup()
	{
		float randomFloat = GameManager.Instance.World.GetGameRandom().RandomFloat;
		float num = 0f;
		for (int i = 0; i < this.state.Count; i++)
		{
			State state = this.state[i];
			num += state.prob;
			if (randomFloat <= num && state.prob > 0f)
			{
				return state.entityClassId;
			}
		}
		return -1;
	}

	public void DidSpawn(int _classId)
	{
		for (int i = 0; i < state.Count; i++)
		{
			State value = state[i];
			if (value.entityClassId == _classId)
			{
				value.numSpawned++;
			}
			state[i] = value;
		}
	}
}
