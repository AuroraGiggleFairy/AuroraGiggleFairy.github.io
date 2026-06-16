using System.Collections.Generic;
using DynamicMusic.Legacy.ObjectModel;

namespace DynamicMusic.Legacy;

public static class LayerReserve
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Queue<InstrumentID> toLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	public static InstrumentID CurrentLoading;

	public static void Tick()
	{
		Load();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Load()
	{
		if (CurrentLoading != null)
		{
			if (!CurrentLoading.IsLoaded)
			{
				CurrentLoading.Load();
			}
			else if (toLoad.Count > 0)
			{
				CurrentLoading = toLoad.Dequeue();
				CurrentLoading.Load();
			}
			else
			{
				CurrentLoading = null;
			}
		}
		else if (toLoad.Count > 0)
		{
			CurrentLoading = toLoad.Dequeue();
			CurrentLoading.Load();
		}
	}

	public static void AddLoad(InstrumentID _id)
	{
		if (toLoad == null)
		{
			toLoad = new Queue<InstrumentID>();
		}
		toLoad.Enqueue(_id);
	}
}
