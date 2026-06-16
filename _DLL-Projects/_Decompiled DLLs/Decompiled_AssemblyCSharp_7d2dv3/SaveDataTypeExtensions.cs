using System;

public static class SaveDataTypeExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathUser = new SaveDataManagedPath(SaveDataType.User.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathSaves = new SaveDataManagedPath(SaveDataType.Saves.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathSavesLocal = new SaveDataManagedPath(SaveDataType.SavesLocal.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathGeneratedWorlds = new SaveDataManagedPath(SaveDataType.GeneratedWorlds.GetPathRaw());

	public static bool IsRoot(this SaveDataType saveDataType)
	{
		return saveDataType == SaveDataType.User;
	}

	public static int GetSlotPathDepth(this SaveDataType saveDataType)
	{
		switch (saveDataType)
		{
		case SaveDataType.User:
			return 0;
		case SaveDataType.Saves:
			return 2;
		case SaveDataType.SavesLocal:
			return 1;
		case SaveDataType.GeneratedWorlds:
			return 1;
		default:
			Log.Error(string.Format("{0}.{1} does not have a slot path length, defaulting to '0'.", "SaveDataType", saveDataType));
			return 0;
		}
	}

	public static string GetPathRaw(this SaveDataType saveDataType)
	{
		return saveDataType switch
		{
			SaveDataType.User => string.Empty, 
			SaveDataType.Saves => "Saves", 
			SaveDataType.SavesLocal => "SavesLocal", 
			SaveDataType.GeneratedWorlds => "GeneratedWorlds", 
			_ => throw new ArgumentOutOfRangeException("saveDataType", saveDataType, $"No path specified for {saveDataType}."), 
		};
	}

	public static SaveDataManagedPath GetPath(this SaveDataType saveDataType)
	{
		return saveDataType switch
		{
			SaveDataType.User => s_rootPathUser, 
			SaveDataType.Saves => s_rootPathSaves, 
			SaveDataType.SavesLocal => s_rootPathSavesLocal, 
			SaveDataType.GeneratedWorlds => s_rootPathGeneratedWorlds, 
			_ => throw new ArgumentOutOfRangeException("saveDataType", saveDataType, $"No relative path specified for {saveDataType}."), 
		};
	}
}
