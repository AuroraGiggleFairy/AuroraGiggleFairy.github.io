using System.Collections.Generic;

public struct LevelRequirement(int _level)
{
	public readonly int Level = _level;

	public List<IRequirement> Requirements = null;

	public void AddRequirement(IRequirement _req)
	{
		if (Requirements == null)
		{
			Requirements = new List<IRequirement>();
		}
		Requirements.Add(_req);
	}
}
