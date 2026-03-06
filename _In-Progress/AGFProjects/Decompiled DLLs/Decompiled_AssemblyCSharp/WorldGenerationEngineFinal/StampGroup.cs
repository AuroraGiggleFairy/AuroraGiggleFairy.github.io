using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public class StampGroup
{
	public string Name;

	public List<Stamp> Stamps;

	public StampGroup(string _name)
	{
		Name = _name;
		Stamps = new List<Stamp>();
	}
}
