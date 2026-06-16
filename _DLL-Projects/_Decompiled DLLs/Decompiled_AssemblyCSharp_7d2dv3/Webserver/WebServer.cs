using Webserver.UrlHandlers;

namespace Webserver;

public static class WebServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Web webInstance;

	public static void Init()
	{
		ModEvents.GameStartDone.RegisterHandler(GameStartDone);
		ModEvents.WorldShuttingDown.RegisterHandler(WorldShuttingDown);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GameStartDone(ref ModEvents.SGameStartDoneData _data)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			webInstance = new Web();
			LogBuffer.Init();
			if (ItemIconHandler.Instance != null)
			{
				ThreadManager.StartCoroutine(ItemIconHandler.Instance.LoadIcons());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WorldShuttingDown(ref ModEvents.SWorldShuttingDownData _data)
	{
		webInstance?.Disconnect();
	}
}
