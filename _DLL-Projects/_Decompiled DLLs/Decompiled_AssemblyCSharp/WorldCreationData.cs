using System;
using System.Xml.Linq;

public class WorldCreationData
{
	public const string PropProviderId = "ProviderId";

	public const string PropWorld_Class = "World.Class";

	public const string PropWorldEnvironment_Prefab = "WorldEnvironment.Prefab";

	public const string PropWorldEnvironment_Class = "WorldEnvironment.Class";

	public const string PropWorldBiomeProvider_Class = "WorldBiomeProvider.Class";

	public const string PropWorldTerrainGenerator_Class = "WorldTerrainGenerator.Class";

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
