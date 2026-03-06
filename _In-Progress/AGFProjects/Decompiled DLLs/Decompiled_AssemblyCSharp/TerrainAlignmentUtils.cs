using System;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainAlignmentUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3> alignHighest = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3> alignHighest4 = new List<Vector3>();

	public static bool AlignToTerrain(Block block, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd, TerrainAlignmentMode alignmentMode)
	{
		BlockShape shape = _blockValue.Block.shape;
		if (!(shape is BlockShapeModelEntity blockShapeModelEntity))
		{
			return false;
		}
		Bounds blockPlacementBounds = GameUtils.GetBlockPlacementBounds(_blockValue.Block);
		if (_ebcd == null || !_ebcd.bHasTransform || (blockPlacementBounds.size.x <= 1f && blockPlacementBounds.size.z <= 1f))
		{
			return false;
		}
		if (_blockValue.ischild)
		{
			_blockPos += new Vector3i(_blockValue.parentx, _blockValue.parenty, _blockValue.parentz);
		}
		Quaternion rotation = shape.GetRotation(_blockValue);
		Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block, rotation);
		Transform transform = _ebcd.transform;
		transform.gameObject.SetActive(value: false);
		blockPlacementBounds.size -= new Vector3(1f, 0f, 1f);
		if (alignmentMode == TerrainAlignmentMode.Vehicle)
		{
			if (blockPlacementBounds.size.x > blockPlacementBounds.size.z)
			{
				if (blockPlacementBounds.size.x > 2.5f)
				{
					blockPlacementBounds.size = new Vector3(blockPlacementBounds.size.x - 2f, blockPlacementBounds.size.y, blockPlacementBounds.size.z);
				}
			}
			else if (blockPlacementBounds.size.z > 2.5f)
			{
				blockPlacementBounds.size = new Vector3(blockPlacementBounds.size.x, blockPlacementBounds.size.y, blockPlacementBounds.size.z - 2f);
			}
		}
		Vector3 vector = World.blockToTransformPos(_blockPos) - Origin.position + new Vector3(0f, 0.5f, 0f) + rotation * new Vector3(blockPlacementBounds.center.x, 0f, blockPlacementBounds.center.z);
		Vector3 zero = Vector3.zero;
		float num = ((alignmentMode == TerrainAlignmentMode.Vehicle) ? 0.2f : 0f);
		Vector3 vector2 = new Vector3(0f, 1f, 0f);
		uint num2 = 0u;
		for (float num3 = 0f - blockPlacementBounds.extents.z - 1f; num3 <= blockPlacementBounds.extents.z + 1.01f; num3 += 1f)
		{
			zero.z = Utils.FastClamp(num3, 0f - blockPlacementBounds.extents.z - 0.45f, blockPlacementBounds.extents.z + 0.45f);
			bool flag = Mathf.Abs(num3) > blockPlacementBounds.extents.z;
			for (float num4 = 0f - blockPlacementBounds.extents.x - 1f; num4 <= blockPlacementBounds.extents.x + 1.01f; num4 += 1f)
			{
				zero.x = Utils.FastClamp(num4, 0f - blockPlacementBounds.extents.x - 0.45f, blockPlacementBounds.extents.x + 0.45f);
				bool flag2 = Mathf.Abs(num4) > blockPlacementBounds.extents.x;
				if (Physics.Raycast(vector + rotation * zero + vector2, Vector3.down, out var hitInfo, 5f, 1082195968))
				{
					if (hitInfo.distance >= 0.5f && hitInfo.distance <= 1.95f)
					{
						num2++;
					}
					Vector3 point = hitInfo.point;
					if (!flag2 || !flag)
					{
						point.y -= num;
					}
					alignHighest.Add(point);
				}
			}
		}
		Quaternion quaternion = transform.rotation;
		Vector3 vector3 = transform.position;
		if (num2 != 0 && alignHighest.Count >= 3)
		{
			alignHighest.Sort([PublicizedFrom(EAccessModifier.Internal)] (Vector3 v1, Vector3 v2) => v2.y.CompareTo(v1.y));
			Quaternion quaternion2 = Quaternion.Inverse(rotation);
			for (int num5 = 0; num5 < 4; num5++)
			{
				for (int num6 = 0; num6 < alignHighest.Count; num6++)
				{
					Vector3 vector4 = quaternion2 * (alignHighest[num6] - vector);
					float num7 = (Mathf.Atan2(vector4.z, vector4.x) + MathF.PI) * 0.6366198f;
					if (num7 >= (float)num5 && num7 < (float)(num5 + 1))
					{
						alignHighest4.Add(alignHighest[num6]);
						break;
					}
				}
			}
			if (alignHighest4.Count >= 3)
			{
				alignHighest4.Sort([PublicizedFrom(EAccessModifier.Internal)] (Vector3 v1, Vector3 v2) => v2.y.CompareTo(v1.y));
				Vector3 vector5 = alignHighest4[0];
				Vector3 vector6 = Vector3.Cross(alignHighest4[2] - vector5, alignHighest4[1] - vector5);
				if (vector6.y > 0.1f || vector6.y < -0.1f)
				{
					vector6.Normalize();
					if (vector6.y < 0f)
					{
						vector6 *= -1f;
					}
					quaternion = Quaternion.FromToRotation(Vector3.up, vector6) * rotation;
					Vector3 vector7 = _blockPos.ToVector3Center() - Origin.position;
					vector7 += rotatedOffset;
					Plane plane = new Plane(vector6, vector5);
					Ray ray = new Ray(vector7, Vector3.down);
					if (plane.Raycast(ray, out var enter))
					{
						vector7.y = ray.GetPoint(enter).y + rotatedOffset.y;
						vector3 = vector7;
					}
				}
			}
		}
		transform.gameObject.SetActive(value: true);
		alignHighest.Clear();
		alignHighest4.Clear();
		if (transform.position != vector3 || transform.rotation != quaternion)
		{
			transform.SetPositionAndRotation(vector3, quaternion);
			return true;
		}
		return false;
	}
}
