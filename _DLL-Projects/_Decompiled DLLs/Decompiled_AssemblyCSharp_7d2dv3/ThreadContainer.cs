public class ThreadContainer
{
	public int DEBUG_TCId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantTerrain TerExt;

	public DistantChunk DChunk;

	public DistantChunkBasicMesh BMesh;

	public bool WasReset;

	public ThreadContainer(DistantTerrain _TerExt, DistantChunk _DChunk, DistantChunkBasicMesh _BMesh, bool _WasReset)
	{
		Init(_TerExt, _DChunk, _BMesh, _WasReset);
	}

	public ThreadContainer()
	{
		TerExt = null;
		DChunk = null;
		BMesh = null;
		WasReset = false;
	}

	public void Init(DistantTerrain _TerExt, DistantChunk _DChunk, DistantChunkBasicMesh _BMesh, bool _WasReset)
	{
		TerExt = _TerExt;
		DChunk = _DChunk;
		BMesh = _BMesh;
		WasReset = _WasReset;
	}

	public void ThreadExtraWork()
	{
		TerExt.ThreadExtraWork(DChunk, BMesh, WasReset);
	}

	public void MainExtraWork()
	{
		TerExt.MainExtraWork(DChunk, BMesh);
	}

	public void Clear(bool IsClearItem)
	{
		if (IsClearItem)
		{
			TerExt = null;
			DChunk = null;
			BMesh = null;
			WasReset = false;
		}
	}
}
