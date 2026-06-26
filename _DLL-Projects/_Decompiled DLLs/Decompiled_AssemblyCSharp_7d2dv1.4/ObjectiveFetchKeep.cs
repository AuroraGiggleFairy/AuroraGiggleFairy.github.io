using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchKeep : ObjectiveFetch
{
	public ObjectiveFetchKeep()
	{
		KeepItems = true;
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchKeep objectiveFetchKeep = new ObjectiveFetchKeep();
		CopyValues(objectiveFetchKeep);
		objectiveFetchKeep.KeepItems = KeepItems;
		return objectiveFetchKeep;
	}
}
