using Webserver;
using Webserver.UrlHandlers;

namespace MapRendering;

public static class MapRendering
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string mapTilesBaseUrl = "/map/";

	public static void Init()
	{
		ModEvents.GameStartDone.RegisterHandler(GameStartDone);
		ModEvents.GameShutdown.RegisterHandler(GameShutdown);
		ModEvents.CalcChunkColorsDone.RegisterHandler(CalcChunkColorsDone);
		Web.ServerInitialized += [PublicizedFrom(EAccessModifier.Internal)] (Web _web) =>
		{
			if (MapRenderer.Enabled)
			{
				_web.RegisterPathHandler("/map/", new StaticHandler(GameIO.GetSaveGameDir() + "/map", MapRenderer.GetTileCache(), _logMissingFiles: false, "web.map"));
				_web.OpenApiHelpers.RegisterCustomSpec(typeof(MapRendering).Assembly, "MapTileHandler", "/map/");
			}
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GameStartDone(ref ModEvents.SGameStartDoneData _data)
	{
		if (MapRenderer.Enabled)
		{
			_ = MapRenderer.Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GameShutdown(ref ModEvents.SGameShutdownData _data)
	{
		MapRenderer.Shutdown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalcChunkColorsDone(ref ModEvents.SCalcChunkColorsDoneData _data)
	{
		MapRenderer.RenderSingleChunk(_data.Chunk);
	}
}
