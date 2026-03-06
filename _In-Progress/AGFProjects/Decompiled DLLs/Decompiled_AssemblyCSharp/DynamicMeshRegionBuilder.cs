using System;
using System.Threading;

public class DynamicMeshRegionBuilder
{
	public string Error;

	public bool StopRequested;

	public DynamicMeshBuilderStatus Status;

	public ExportMeshResult Result = ExportMeshResult.Missing;

	public DynamicMeshRegion Region;

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread thread;

	public static World world => GameManager.Instance.World;

	public bool AddNewItem(DynamicMeshRegion region)
	{
		if (Status != DynamicMeshBuilderStatus.Ready)
		{
			Log.Warning("Builder thread tried to start when not ready. Current Status: " + Status);
			return false;
		}
		Region = region;
		Status = DynamicMeshBuilderStatus.StartingExport;
		return true;
	}

	public void RequestStop(bool forceStop = false)
	{
		StopRequested = true;
		if (forceStop)
		{
			try
			{
				thread?.Abort();
			}
			catch (Exception)
			{
			}
			Status = DynamicMeshBuilderStatus.Stopped;
		}
	}

	public void StartThread()
	{
		thread = new Thread([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			try
			{
				while (!StopRequested)
				{
					if (GameManager.Instance == null || GameManager.Instance.World == null)
					{
						return;
					}
					if (Status != DynamicMeshBuilderStatus.Ready && Status != DynamicMeshBuilderStatus.Complete)
					{
						if (Status != DynamicMeshBuilderStatus.StartingExport)
						{
							Log.Error("Builder thread and wrong state: " + Status);
							Status = DynamicMeshBuilderStatus.Error;
							return;
						}
						throw new NotImplementedException("No build method");
					}
					Thread.Sleep(100);
				}
			}
			catch (Exception ex)
			{
				Error = "Builder error: " + ex.Message;
				Log.Error(Error);
			}
			Status = DynamicMeshBuilderStatus.Stopped;
		});
		thread.Start();
	}
}
