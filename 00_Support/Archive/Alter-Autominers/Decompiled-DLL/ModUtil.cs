using UnityEngine;

public static class ModUtil
{
	public static Vector3i getOffset(BlockValue _blockValue)
	{
		Vector3i offset = Vector3i.zero;
		if (_blockValue.Block.Properties.Values.ContainsKey("BlockCheckOffset"))
		{
			offset = Vector3i.Parse(_blockValue.Block.Properties.Values["BlockCheckOffset"]);
			Vector3 vector = Quaternion.Euler(0f, 90 * _blockValue.rotation, 0f) * offset.ToVector3();
			offset = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
		}
		return offset;
	}
}
