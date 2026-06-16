using System;
using System.Xml.Linq;

public class WorldCreationData
{
	public const string PropProviderId = "ProviderId";

	public const string PropWorldEnvironment = "WorldEnvironment";

	public const string PropWorldEnvironment_Prefab = "Prefab";

	public const string PropWorldEnvironment_Class = "Class";

	public DynamicProperties Properties = new DynamicProperties();

	public WorldCreationData(string _levelDir)
	{
		try
		{
			XDocument xDocument = SdXDocument.Load(_levelDir + "/world.xml");
			if (xDocument.Root == null)
			{
				throw new Exception("No root node in world.xml!");
			}
			foreach (XElement item in xDocument.Root.Elements("property"))
			{
				Properties.Add(item);
			}
		}
		catch (Exception)
		{
		}
	}

	public void Apply(World _world, WorldState _worldState)
	{
		if (Properties.Values.ContainsKey("ProviderId"))
		{
			_worldState.providerId = (EnumChunkProviderId)int.Parse(Properties.Values["ProviderId"]);
		}
	}
}
