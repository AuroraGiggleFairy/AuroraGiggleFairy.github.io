using UnityEngine;

public class DistantChunkMapInfo
{
	public Vector4[] ChunkTriggerArea;

	public Vector2i[][] ChunkToDelete;

	public Vector2i[][] ChunkToAdd;

	public Vector2i[][] ChunkToConvDel;

	public Vector2i[][] ChunkToConvAdd;

	public Vector4[][] ChunkToConvAddEdgeFactor;

	public Vector4[][] ChunkToAddEdgeFactor;

	public Vector2i[][][] ChunkEdgeToOwnResLevel;

	public int[][][] ChunkEdgeToOwnRLEdgeId;

	public Vector2i[][][] ChunkEdgeToNextResLevel;

	public int[][][] ChunkEdgeToNextRLEdgeId;

	public Vector2i[] ChunkLLIntPos;

	public Vector2[] ChunkLLPos;

	public int[][] NeighbResLevel;

	public float[][] EdgeResFactor;

	public float NextResLevelEdgeFactor;

	public int NbChunk;

	public int NbCurChunkInOneNextLevelChunk;

	public int ResLevel;

	public int LayerId = 28;

	public int ChunkDataListResLevel;

	public int ChunkResolution;

	public int ColliderResolution;

	public bool IsColliderEnabled;

	public float ResRadius;

	public int IntResRadius;

	public Vector2i LLIntArea;

	public float ChunkWidth;

	public float UnitStep;

	public Vector2 ShiftVec;

	public Vector3 ChunkExtraShiftVector;

	public DistantChunkBasicMesh BaseMesh;

	public int[][] EdgeMap;

	public int[] SouthMap;

	public int[] WestMap;

	public int[] NorthMap;

	public int[] EastMap;
}
