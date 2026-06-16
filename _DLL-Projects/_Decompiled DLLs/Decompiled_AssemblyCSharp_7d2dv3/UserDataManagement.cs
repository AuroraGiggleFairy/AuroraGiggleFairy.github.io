using System;
using System.IO;
using Platform;
using WorldGenerationEngineFinal;

public static class UserDataManagement
{
	public enum Result
	{
		Success,
		TargetAlreadyExists,
		Exception,
		FailedToMoveSaves
	}

	public struct SaveMoveResultInfo
	{
		public Result Result;

		public string NewSaveDir;
	}

	public struct SaveCopyResultInfo
	{
		public Result Result;

		public string NewSaveDir;
	}

	public struct WorldMove
	{
		public readonly SaveInfoProvider.WorldEntryInfo worldInfo;

		public readonly UserDataStorageType moveToStorageType;

		public bool IsReady => worldInfo != null;

		public int CountOfAssociatedSavesInSameStorage
		{
			get
			{
				int num = 0;
				foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in SaveInfoProvider.Instance.SaveEntryInfos)
				{
					if (saveEntryInfo.WorldEntry == worldInfo && saveEntryInfo.StorageType == worldInfo.Location.StorageType)
					{
						num++;
					}
				}
				return num;
			}
		}

		public WorldMove(SaveInfoProvider.WorldEntryInfo worldInfo, UserDataStorageType moveToStorageType)
		{
			if (!worldInfo.Moveable)
			{
				throw new Exception("Cannot create world move op for non-moveable world");
			}
			this.worldInfo = worldInfo;
			this.moveToStorageType = moveToStorageType;
		}

		public Result PerformMove()
		{
			Result result = MoveWorld(worldInfo, moveToStorageType);
			if (result == Result.Success && !moveToStorageType.UsesDataLimit())
			{
				bool flag = true;
				foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in SaveInfoProvider.Instance.SaveEntryInfos)
				{
					if (saveEntryInfo.WorldEntry == worldInfo && saveEntryInfo.StorageType != moveToStorageType)
					{
						string saveGameDir = GameIO.GetSaveGameDir(worldInfo.Name, saveEntryInfo.Name, moveToStorageType);
						Result result2 = MoveSave(saveEntryInfo.SaveDir, saveGameDir);
						if (result2 == Result.Success)
						{
							SetSaveArchived(saveGameDir, shouldArchive: false);
						}
						flag = flag && result2 == Result.Success;
					}
				}
				if (!flag)
				{
					result = Result.FailedToMoveSaves;
				}
			}
			PathAbstractions.WorldsSearchPaths.InvalidateCache();
			SaveInfoProvider.Instance.SetDirty();
			return result;
		}
	}

	public struct GameSaveMove
	{
		public readonly SaveInfoProvider.SaveEntryInfo saveInfo;

		public readonly SaveInfoProvider.WorldEntryInfo worldInfo;

		public readonly string targetName;

		public readonly UserDataStorageType moveToStorageType;

		public bool IsReady => saveInfo != null;

		public bool WorldRequiresMoving => worldInfo.ShouldBeMovedWithSave(moveToStorageType);

		public GameSaveMove(SaveInfoProvider.SaveEntryInfo saveInfo, UserDataStorageType moveToStorageType)
			: this(saveInfo, saveInfo.Name, moveToStorageType)
		{
		}

		public GameSaveMove(SaveInfoProvider.SaveEntryInfo saveInfo, string targetName, UserDataStorageType moveToStorageType)
		{
			if (saveInfo.WorldEntry.Type == SaveInfoProvider.RemoteWorldsType && targetName != saveInfo.Name)
			{
				throw new ArgumentException("Cannot rename remote world saves");
			}
			this.saveInfo = saveInfo;
			worldInfo = saveInfo.WorldEntry;
			this.targetName = targetName;
			this.moveToStorageType = moveToStorageType;
		}

		public SaveMoveResultInfo PerformMove()
		{
			if (WorldRequiresMoving)
			{
				Result result = MoveWorld(worldInfo, moveToStorageType);
				if (result != Result.Success)
				{
					return new SaveMoveResultInfo
					{
						Result = result
					};
				}
				PathAbstractions.WorldsSearchPaths.InvalidateCache();
				SaveInfoProvider.Instance.SetDirty();
			}
			string text = ((targetName != saveInfo.Name) ? GameIO.GetSaveGameDir(worldInfo.Name, targetName, moveToStorageType) : GetSavePathTarget(saveInfo, moveToStorageType));
			Result result2 = MoveSave(saveInfo.SaveDir, text);
			SaveInfoProvider.Instance.SetDirty();
			return new SaveMoveResultInfo
			{
				Result = result2,
				NewSaveDir = ((result2 == Result.Success) ? text : null)
			};
		}
	}

	public struct GameSaveCopy
	{
		public readonly SaveInfoProvider.SaveEntryInfo saveInfo;

		public readonly SaveInfoProvider.WorldEntryInfo worldInfo;

		public readonly string targetName;

		public readonly UserDataStorageType storageTarget;

		public bool IsReady => saveInfo != null;

		public bool WorldRequiresMoving => worldInfo.ShouldBeMovedWithSave(storageTarget);

		public GameSaveCopy(SaveInfoProvider.SaveEntryInfo saveInfo, string targetName, UserDataStorageType moveToStorageType)
		{
			this.saveInfo = saveInfo;
			if (saveInfo.WorldEntry.Type == SaveInfoProvider.RemoteWorldsType)
			{
				throw new Exception("Copy not permitted for remote world saves");
			}
			worldInfo = saveInfo.WorldEntry;
			this.targetName = targetName;
			storageTarget = moveToStorageType;
		}

		public SaveCopyResultInfo PerformCopy()
		{
			if (WorldRequiresMoving)
			{
				Result result = MoveWorld(worldInfo, storageTarget);
				if (result != Result.Success)
				{
					return new SaveCopyResultInfo
					{
						Result = result
					};
				}
				PathAbstractions.WorldsSearchPaths.InvalidateCache();
				SaveInfoProvider.Instance.SetDirty();
			}
			string saveGameDir = GameIO.GetSaveGameDir(worldInfo.Name, targetName, storageTarget);
			Result result2 = CopySave(saveInfo.SaveDir, saveGameDir);
			SaveInfoProvider.Instance.SetDirty();
			return new SaveCopyResultInfo
			{
				Result = result2,
				NewSaveDir = ((result2 == Result.Success) ? saveGameDir : null)
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Result MoveWorld(SaveInfoProvider.WorldEntryInfo worldInfo, UserDataStorageType moveToStorageType)
	{
		string worldPath = WorldBuilder.GetWorldPath(moveToStorageType, worldInfo.Name);
		if (SdDirectory.Exists(worldPath))
		{
			Log.Error("Could not move world from " + worldInfo.Location.FullPath + " to " + worldPath + ". Directory already exists");
			return Result.TargetAlreadyExists;
		}
		Log.Out("Moving world " + worldInfo.Name + " from " + worldInfo.Location.FullPath + " to " + worldPath);
		try
		{
			GameIO.SafeDirectoryMove(worldInfo.Location.FullPath, worldPath);
		}
		catch (Exception ex)
		{
			Log.Error("Could not move world: " + ex.Message);
			Log.Exception(ex);
			return Result.Exception;
		}
		return Result.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Result MoveSave(string sourceDir, string targetDir)
	{
		if (SdDirectory.Exists(targetDir))
		{
			Log.Error("Could not move save from " + sourceDir + " to " + targetDir + ". Directory already exists");
			return Result.TargetAlreadyExists;
		}
		Log.Out("Moving save from " + sourceDir + " to " + targetDir);
		try
		{
			GameIO.SafeDirectoryMove(sourceDir, targetDir);
		}
		catch (Exception ex)
		{
			Log.Error("Could not move save: " + ex.Message);
			Log.Exception(ex);
			return Result.Exception;
		}
		return Result.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Result CopySave(string sourceDir, string targetDir)
	{
		if (SdDirectory.Exists(targetDir))
		{
			Log.Error("Could not copy save from " + sourceDir + " to " + targetDir + ". Directory already exists");
			return Result.TargetAlreadyExists;
		}
		Log.Out("Copying save from " + sourceDir + " to " + targetDir);
		try
		{
			GameIO.SafeDirectoryCopy(sourceDir, targetDir);
		}
		catch (Exception ex)
		{
			Log.Error("Could not copy save: " + ex.Message);
			Log.Exception(ex);
			return Result.Exception;
		}
		return Result.Success;
	}

	public static string GetSavePathTarget(SaveInfoProvider.SaveEntryInfo saveInfo, UserDataStorageType storageTarget)
	{
		SaveInfoProvider.WorldEntryInfo worldEntry = saveInfo.WorldEntry;
		if (worldEntry.Type == SaveInfoProvider.RemoteWorldsType)
		{
			return GameIO.GetSaveGameLocalDir(storageTarget, System.IO.Path.GetFileName(saveInfo.SaveDir));
		}
		return GameIO.GetSaveGameDir(worldEntry.Name, saveInfo.Name, storageTarget);
	}

	public static void SetSaveArchived(string saveDir, bool shouldArchive)
	{
		try
		{
			string text = System.IO.Path.Combine(saveDir, "archived.flag");
			if (SdFile.Exists(text))
			{
				if (!shouldArchive)
				{
					Log.Out("Unarchiving save by deleting: " + text);
					SdFile.Delete(text);
				}
			}
			else if (shouldArchive)
			{
				Log.Out("Archiving save by creating: " + text);
				using (SdFile.Create(text))
				{
					return;
				}
			}
		}
		catch (Exception e)
		{
			Log.Error($"Failed to set archived to {shouldArchive} for save at {saveDir}");
			Log.Exception(e);
		}
	}

	public static void SetSaveDataLimit(string saveDir, long reservedBytes)
	{
		try
		{
			string filename = System.IO.Path.Combine(saveDir, "main.ttw");
			WorldState worldState = new WorldState();
			if (!worldState.Load(filename, _warnOnDifferentVersion: false))
			{
				Log.Error($"Could not update save data limit for {saveDir} to {reservedBytes}B. WorldState failed to load");
			}
			else if (worldState.saveDataLimit != reservedBytes)
			{
				Log.Out($"Updating reserved bytes for {saveDir} to {reservedBytes}");
				worldState.saveDataLimit = reservedBytes;
				if (!worldState.Save(filename))
				{
					Log.Error($"Could not update save data limit for {saveDir} to {reservedBytes}B. WorldState failed to save");
				}
			}
		}
		catch (Exception e)
		{
			Log.Error($"Failed to set data limit to {reservedBytes} for save at {saveDir}");
			Log.Exception(e);
		}
	}
}
