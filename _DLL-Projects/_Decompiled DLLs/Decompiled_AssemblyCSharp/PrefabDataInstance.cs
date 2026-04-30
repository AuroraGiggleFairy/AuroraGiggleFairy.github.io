using UnityEngine;

public class PrefabDataInstance
{
	public int id;

	public PrefabData prefab;

	public Vector3i boundingBoxPosition;

	public byte rotation;

	public Color32 previewColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 previewColorDefault = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Vector3i boundingBoxSize => prefab.size;

	public Vector2i CenterXZ
	{
		get
		{
			Vector2i result = default(Vector2i);
			result.x = boundingBoxPosition.x + prefab.size.x / 2;
			result.y = boundingBoxPosition.z + prefab.size.z / 2;
			return result;
		}
	}

	public Vector2 CenterXZV2
	{
		get
		{
			Vector2 result = default(Vector2);
			result.x = (float)boundingBoxPosition.x + (float)prefab.size.x * 0.5f;
			result.y = (float)boundingBoxPosition.z + (float)prefab.size.z * 0.5f;
			return result;
		}
	}

	public PathAbstractions.AbstractedLocation location => prefab.location;

	public PrefabDataInstance(int _id, Vector3i _position, byte _rotation, PrefabData _prefabData)
	{
		id = _id;
		prefab = _prefabData;
		boundingBoxPosition = _position;
		rotation = _rotation;
		previewColor = previewColorDefault;
	}
}
