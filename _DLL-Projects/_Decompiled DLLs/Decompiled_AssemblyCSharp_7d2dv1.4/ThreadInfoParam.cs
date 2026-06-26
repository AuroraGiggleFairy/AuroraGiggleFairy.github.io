public class ThreadInfoParam
{
	public ThreadContainer[] ThreadContListA;

	public int CntThreadContList;

	public int LengthThreadContList;

	public int ResLevel;

	public int OutId;

	public DistantChunk[] ForwardChunkToDeleteIdA;

	public DistantChunk[] BackwardChunkToDeleteIdA;

	public int CntForwardChunkToDeleteId;

	public int LengthForwardChunkToDeleteId;

	public int CntBackwardChunkToDeleteId;

	public int LengthBackwardChunkToDeleteId;

	public DistantChunk[][] ForwardChunkSeamToAdjust;

	public DistantChunk[][] BackwardChunkSeamToAdjust;

	public int[][] ForwardEdgeId;

	public int[][] BackwardEdgeId;

	public int CntForwardChunkSeamToAdjust;

	public int CntBackwardChunkSeamToAdjust;

	public int FDLengthForwardChunkSeamToAdjust;

	public int FDLengthBackwardChunkSeamToAdjust;

	public int[] SDLengthForwardChunkSeamToAdjust;

	public int[] SDLengthBackwardChunkSeamToAdjust;

	[PublicizedFrom(EAccessModifier.Private)]
	public int TmpArraySizeFristDim = 150;

	[PublicizedFrom(EAccessModifier.Private)]
	public int TmpArraySizeSecondDim = 64;

	public bool IsThreadDone;

	public bool IsCoroutineDone;

	public bool IsBigCapacity;

	public bool IsAsynchronous;

	public ThreadInfoParam()
	{
		IsBigCapacity = false;
		ThreadContListA = new ThreadContainer[TmpArraySizeFristDim * 20];
		ForwardChunkToDeleteIdA = new DistantChunk[TmpArraySizeFristDim];
		BackwardChunkToDeleteIdA = new DistantChunk[TmpArraySizeFristDim * 2];
		ForwardChunkSeamToAdjust = new DistantChunk[TmpArraySizeFristDim][];
		BackwardChunkSeamToAdjust = new DistantChunk[TmpArraySizeFristDim][];
		ForwardEdgeId = new int[TmpArraySizeFristDim][];
		BackwardEdgeId = new int[TmpArraySizeFristDim][];
		SDLengthForwardChunkSeamToAdjust = new int[TmpArraySizeFristDim];
		SDLengthBackwardChunkSeamToAdjust = new int[TmpArraySizeFristDim];
		for (int i = 0; i < TmpArraySizeFristDim; i++)
		{
			ForwardChunkSeamToAdjust[i] = new DistantChunk[TmpArraySizeSecondDim];
			BackwardChunkSeamToAdjust[i] = new DistantChunk[TmpArraySizeSecondDim];
			ForwardEdgeId[i] = new int[TmpArraySizeSecondDim];
			BackwardEdgeId[i] = new int[TmpArraySizeSecondDim];
		}
		FDLengthForwardChunkSeamToAdjust = 0;
		FDLengthBackwardChunkSeamToAdjust = 0;
		CntForwardChunkSeamToAdjust = 0;
		CntBackwardChunkSeamToAdjust = 0;
		ResLevel = 0;
		OutId = 0;
		IsThreadDone = false;
		IsCoroutineDone = false;
	}

	public ThreadInfoParam(DistantChunkMap _CMap, int _ResLevel, int _OutId, bool _IsBigCapacity)
	{
		ThreadContListA = new ThreadContainer[TmpArraySizeFristDim * 20];
		ForwardChunkToDeleteIdA = new DistantChunk[TmpArraySizeFristDim];
		BackwardChunkToDeleteIdA = new DistantChunk[TmpArraySizeFristDim * 2];
		ForwardChunkSeamToAdjust = new DistantChunk[TmpArraySizeFristDim][];
		BackwardChunkSeamToAdjust = new DistantChunk[TmpArraySizeFristDim][];
		ForwardEdgeId = new int[TmpArraySizeFristDim][];
		BackwardEdgeId = new int[TmpArraySizeFristDim][];
		SDLengthForwardChunkSeamToAdjust = new int[TmpArraySizeFristDim];
		SDLengthBackwardChunkSeamToAdjust = new int[TmpArraySizeFristDim];
		for (int i = 0; i < TmpArraySizeFristDim; i++)
		{
			ForwardChunkSeamToAdjust[i] = new DistantChunk[TmpArraySizeSecondDim];
			BackwardChunkSeamToAdjust[i] = new DistantChunk[TmpArraySizeSecondDim];
			ForwardEdgeId[i] = new int[TmpArraySizeSecondDim];
			BackwardEdgeId[i] = new int[TmpArraySizeSecondDim];
			SDLengthForwardChunkSeamToAdjust[i] = 0;
			SDLengthBackwardChunkSeamToAdjust[i] = 0;
		}
		Init(_CMap, _ResLevel, _OutId, _IsBigCapacity);
	}

	public void Init(DistantChunkMap _CMap, int _ResLevel, int _OutId, bool _IsBigCapacity)
	{
		IsBigCapacity = _IsBigCapacity;
		CntThreadContList = 0;
		LengthThreadContList = 0;
		CntForwardChunkToDeleteId = 0;
		LengthForwardChunkToDeleteId = 0;
		CntBackwardChunkToDeleteId = 0;
		LengthBackwardChunkToDeleteId = 0;
		for (int i = 0; i < TmpArraySizeFristDim; i++)
		{
			SDLengthForwardChunkSeamToAdjust[i] = 0;
			SDLengthBackwardChunkSeamToAdjust[i] = 0;
		}
		CntForwardChunkSeamToAdjust = 0;
		CntBackwardChunkSeamToAdjust = 0;
		FDLengthForwardChunkSeamToAdjust = 0;
		FDLengthBackwardChunkSeamToAdjust = 0;
		ResLevel = _ResLevel;
		OutId = _OutId;
		IsThreadDone = false;
		IsCoroutineDone = false;
	}

	public void ClearAll(ThreadContainerPool TmpThContPool = null)
	{
		if (TmpThContPool != null)
		{
			while (CntThreadContList < LengthThreadContList)
			{
				TmpThContPool.ReturnObject(ThreadContListA[CntThreadContList], IsClearItem: true);
				ThreadContListA[CntThreadContList] = null;
				CntThreadContList++;
			}
		}
		IsThreadDone = true;
		IsCoroutineDone = true;
	}
}
