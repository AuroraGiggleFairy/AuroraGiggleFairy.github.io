using System;
using UnityEngine;

public class Voxel
{
	public delegate bool DeletageNextBlockHit(int _x, int _y, int _z);

	public const int HM_All = 4095;

	public const int HM_None = 0;

	public const int HM_Transparent = 1;

	public const int HM_LiquidOnly = 2;

	public const int HM_Moveable = 4;

	public const int HM_Bullet = 8;

	public const int HM_Rocket = 16;

	public const int HM_Arrows = 32;

	public const int HM_NotMoveable = 64;

	public const int HM_Melee = 128;

	public const int HM_FirstNotEmptyBlock = 256;

	public static RaycastHit phyxRaycastHit;

	public static WorldRayHitInfo voxelRayHitInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i[] normalsI;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] normals;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockFace[] normalToFaces;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] hitMaskSeparator;

	[PublicizedFrom(EAccessModifier.Private)]
	static Voxel()
	{
		normalsI = new Vector3i[6]
		{
			new Vector3i(0, -1, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 1, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(1, 0, 0)
		};
		normals = new Vector3[6]
		{
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, 0f, -1f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(1f, 0f, 0f)
		};
		normalToFaces = new BlockFace[6]
		{
			BlockFace.Bottom,
			BlockFace.South,
			BlockFace.West,
			BlockFace.Top,
			BlockFace.North,
			BlockFace.East
		};
		hitMaskSeparator = new char[1] { ',' };
		voxelRayHitInfo = new WorldRayHitInfo();
	}

	public static bool Raycast(World _worldData, Ray ray, float distance, int _hitmask, float _sphereSize)
	{
		return Raycast(_worldData, ray, distance, -538488837, _hitmask, _sphereSize);
	}

	public static bool Raycast(World _worldData, Ray ray, float distance, bool bHitTransparentBlocks, bool bHitNotCollidableBlocks)
	{
		return Raycast(_worldData, ray, distance, -538488837, 0x42 | (bHitTransparentBlocks ? 1 : 0) | (bHitNotCollidableBlocks ? 4 : 0), 0f);
	}

	public static bool Raycast(World _world, Ray ray, float distance, int _layerMask, int _hitMask, float _sphereRadius)
	{
		return raycastNew(_world, ray, distance, _layerMask, _hitMask, _sphereRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void terrainMeshHit(World _world, Vector3 dirNormalized, string phsxTag, Vector3 phsxHitpPoint, HitInfoDetails.VoxelData lastHitData, int _layerMask, int _hitMask)
	{
		Vector3 worldPos = phsxHitpPoint + dirNormalized * 0.01f;
		int num = 0;
		if (phsxTag.Length > 2)
		{
			char c = phsxTag[phsxTag.Length - 2];
			char c2 = phsxTag[phsxTag.Length - 1];
			if (c >= '0' && c <= '9' && c2 >= '0' && c2 <= '9')
			{
				num += (c - 48) * 10;
				num += c2 - 48;
			}
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[num];
		if (chunkCluster == null)
		{
			return;
		}
		voxelRayHitInfo.hit.clrIdx = num;
		Vector3 vector = chunkCluster.ToLocalPosition(worldPos);
		dirNormalized = chunkCluster.ToLocalVector(dirNormalized);
		voxelRayHitInfo.hit.blockPos = World.worldToBlockPos(vector);
		voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos);
		if ((_layerMask & 0x40000) == 0 && voxelRayHitInfo.hit.blockValue.Block.MeshIndex == 3)
		{
			voxelRayHitInfo.hit.blockPos = OneVoxelStep(World.worldToBlockPos(vector), vector, dirNormalized, out voxelRayHitInfo.hit.pos, out voxelRayHitInfo.hit.blockFace);
			voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos);
		}
		BlockValue blockValue = voxelRayHitInfo.hit.blockValue;
		bool flag = phsxTag == "T_Mesh";
		bool flag2 = (!voxelRayHitInfo.hit.voxelData.IsOnlyAir() && flag && !blockValue.Block.shape.IsTerrain()) || (!flag && blockValue.Block.shape.IsTerrain());
		HitInfoDetails.VoxelData voxelData;
		if (flag2 || voxelRayHitInfo.hit.voxelData.Equals(lastHitData) || ((_hitMask & 2) == 0 && voxelRayHitInfo.hit.voxelData.IsOnlyWater()))
		{
			voxelRayHitInfo.lastBlockPos = voxelRayHitInfo.hit.blockPos;
			bool flag3 = true;
			if (voxelRayHitInfo.hit.voxelData.Equals(lastHitData) && !flag2 && !voxelRayHitInfo.hit.voxelData.IsOnlyWater())
			{
				lastHitData = voxelRayHitInfo.hit.voxelData;
				voxelRayHitInfo.hit.blockPos = OneVoxelStep(World.worldToBlockPos(vector), vector, dirNormalized, out voxelRayHitInfo.hit.pos, out voxelRayHitInfo.hit.blockFace);
				voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos);
			}
			else
			{
				flag3 = false;
			}
			if (flag3 && !voxelRayHitInfo.hit.voxelData.Equals(lastHitData))
			{
				return;
			}
			if (voxelRayHitInfo.hit.blockPos.y > 0)
			{
				voxelData = (voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos - Vector3i.up));
				if (!voxelData.Equals(lastHitData) && !voxelRayHitInfo.hit.voxelData.IsOnlyWater() && ((flag && voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain()) || (!flag && !voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain())))
				{
					voxelRayHitInfo.hit.blockFace = BlockFace.Top;
					voxelRayHitInfo.hit.blockPos = voxelRayHitInfo.hit.blockPos - Vector3i.up;
					return;
				}
			}
			if (voxelRayHitInfo.hit.blockPos.y < 255)
			{
				voxelData = (voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos + Vector3i.up));
				if (!voxelData.Equals(lastHitData) && !voxelRayHitInfo.hit.voxelData.IsOnlyWater() && ((flag && voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain()) || (!flag && !voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain())))
				{
					voxelRayHitInfo.hit.blockFace = BlockFace.Bottom;
					voxelRayHitInfo.hit.blockPos = voxelRayHitInfo.hit.blockPos + Vector3i.up;
					return;
				}
			}
			int num2 = calcBestNormalToRaycastHit(chunkCluster);
			voxelRayHitInfo.hit.blockPos = voxelRayHitInfo.hit.blockPos - normalsI[num2];
			voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos);
			voxelRayHitInfo.hit.blockFace = normalToFaces[num2];
			return;
		}
		int num3 = calcBestNormalToRaycastHit(chunkCluster);
		voxelRayHitInfo.hit.blockFace = normalToFaces[num3];
		Ray ray = new Ray(vector, -1f * dirNormalized);
		int num4 = 0;
		HitInfoDetails.VoxelData voxelData2;
		do
		{
			voxelRayHitInfo.lastBlockPos = OneVoxelStep(World.worldToBlockPos(ray.origin), ray.origin, ray.direction, out var hitPos, out var _);
			ray.origin = hitPos - dirNormalized * 0.01f;
			voxelData = (voxelData2 = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.lastBlockPos));
		}
		while (!voxelData.IsOnlyAir() && voxelData2.BlockValue.Block.isMultiBlock && !voxelData2.IsOnlyWater() && num4++ < 3);
		if (!(phsxTag == "T_Mesh_B") || !(MeshDescription.meshes[voxelRayHitInfo.hit.blockValue.Block.MeshIndex].Tag != "T_Mesh_B"))
		{
			return;
		}
		voxelData = (voxelData2 = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos + Vector3i.up));
		if (!voxelData.IsOnlyAir() && MeshDescription.meshes[voxelData2.BlockValue.Block.MeshIndex].Tag == "T_Mesh_B")
		{
			voxelRayHitInfo.hit.blockPos = voxelRayHitInfo.hit.blockPos + Vector3i.up;
			voxelRayHitInfo.hit.voxelData = voxelData2;
			return;
		}
		voxelData = (voxelData2 = HitInfoDetails.VoxelData.GetFrom(chunkCluster, voxelRayHitInfo.hit.blockPos - Vector3i.up));
		if (!voxelData.IsOnlyAir() && MeshDescription.meshes[voxelData2.BlockValue.Block.MeshIndex].Tag == "T_Mesh_B")
		{
			voxelRayHitInfo.hit.blockPos = voxelRayHitInfo.hit.blockPos - Vector3i.up;
			voxelRayHitInfo.hit.voxelData = voxelData2;
		}
	}

	public static Vector3i GoBackOnVoxels(ChunkCluster cc, Ray newRay, out BlockValue bv)
	{
		bv = BlockValue.Air;
		Vector3i zero = Vector3i.zero;
		int num = 0;
		BlockValue blockValue;
		do
		{
			zero = OneVoxelStep(World.worldToBlockPos(newRay.origin), newRay.origin, newRay.direction, out var hitPos, out var _);
			newRay.origin = hitPos + newRay.direction * 0.01f;
			blockValue = (bv = cc.GetBlock(zero));
		}
		while (!blockValue.isair && num++ < 3);
		return zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool raycastNew(World _world, Ray ray, float distance, int _layerMask, int _hitMask, float _sphereRadius)
	{
		bool flag = _sphereRadius > 0.01f;
		bool flag2 = (_hitMask & 1) != 0;
		bool flag3 = (_hitMask & 0x40) != 0;
		bool flag4 = (_hitMask & 4) != 0;
		bool flag5 = (_hitMask & 2) != 0;
		bool flag6 = (_hitMask & 8) != 0;
		bool flag7 = (_hitMask & 0x10) != 0;
		bool flag8 = (_hitMask & 0x80) != 0;
		bool flag9 = (_hitMask & 0x20) != 0;
		voxelRayHitInfo.Clear();
		voxelRayHitInfo.ray = ray;
		HitInfoDetails.VoxelData lastHitData = default(HitInfoDetails.VoxelData);
		int num = 0;
		while (num++ < 10 && distance > 0f)
		{
			Ray ray2 = new Ray(ray.origin - Origin.position, ray.direction);
			if (!(flag ? Physics.SphereCast(ray2, _sphereRadius, out phyxRaycastHit, distance, _layerMask) : Physics.Raycast(ray2, out phyxRaycastHit, distance, _layerMask)))
			{
				break;
			}
			Transform transform = phyxRaycastHit.collider.transform;
			Vector3 vector = phyxRaycastHit.point + Origin.position;
			string text = phyxRaycastHit.collider.transform.tag;
			voxelRayHitInfo.hitCollider = phyxRaycastHit.collider;
			voxelRayHitInfo.hitTriangleIdx = phyxRaycastHit.triangleIndex;
			Vector3 normalized = ray.direction.normalized;
			if (text == "T_Block")
			{
				GameUtils.FindMasterBlockForEntityModelBlock(_world, normalized, text, vector, transform, voxelRayHitInfo);
				text = "B_Mesh";
				if (voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
				{
					voxelRayHitInfo.fmcHit = voxelRayHitInfo.hit;
					voxelRayHitInfo.fmcHit.pos = vector - normalized * 0.01f;
				}
			}
			else if (text == "T_Deco")
			{
				if (DecoManager.Instance.GetParentBlockOfDecoration(transform, out voxelRayHitInfo.hit.blockPos, out var _decoObject))
				{
					voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(_world, voxelRayHitInfo.hit.blockPos);
					voxelRayHitInfo.hit.pos = vector - ray.direction * 0.1f;
					voxelRayHitInfo.hit.distanceSq = ((vector != Vector3.zero) ? (ray.origin - vector).sqrMagnitude : float.MaxValue);
					if (voxelRayHitInfo.hit.voxelData.IsOnlyAir())
					{
						BlockValue bv = _decoObject.bv;
						bv.damage = _decoObject.bv.Block.MaxDamage - 1;
						voxelRayHitInfo.hit.voxelData.Set(bv, voxelRayHitInfo.hit.voxelData.WaterValue);
					}
					voxelRayHitInfo.fmcHit = voxelRayHitInfo.hit;
				}
			}
			else
			{
				if (!GameUtils.IsBlockOrTerrain(text))
				{
					voxelRayHitInfo.transform = transform;
					voxelRayHitInfo.tag = text;
					voxelRayHitInfo.bHitValid = true;
					voxelRayHitInfo.hit.pos = vector;
					voxelRayHitInfo.hit.distanceSq = ((vector != Vector3.zero) ? (ray.origin - vector).sqrMagnitude : float.MaxValue);
					voxelRayHitInfo.fmcHit = voxelRayHitInfo.hit;
					return true;
				}
				terrainMeshHit(_world, normalized, text, vector, lastHitData, _layerMask, _hitMask);
				if (voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
				{
					voxelRayHitInfo.fmcHit = voxelRayHitInfo.hit;
					voxelRayHitInfo.fmcHit.blockPos = voxelRayHitInfo.lastBlockPos;
					voxelRayHitInfo.fmcHit.pos = vector - normalized * 0.01f;
				}
			}
			lastHitData = voxelRayHitInfo.hit.voxelData;
			Block block = voxelRayHitInfo.hit.blockValue.Block;
			if (voxelRayHitInfo.hit.voxelData.IsOnlyWater() && voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
			{
				voxelRayHitInfo.fmcHit.blockPos = voxelRayHitInfo.hit.blockPos;
				voxelRayHitInfo.fmcHit.voxelData = voxelRayHitInfo.hit.voxelData;
				voxelRayHitInfo.fmcHit.blockFace = BlockFace.Top;
				voxelRayHitInfo.fmcHit.pos = vector;
			}
			bool flag10 = block.IsSeeThrough(_world, voxelRayHitInfo.hit.clrIdx, voxelRayHitInfo.hit.blockPos, voxelRayHitInfo.hit.blockValue);
			if ((flag3 && block.IsCollideMovement && (flag2 || !flag10)) || (flag4 && !block.IsCollideMovement && !voxelRayHitInfo.hit.voxelData.IsOnlyWater()) || (flag6 && block.IsCollideBullets) || (flag7 && block.IsCollideRockets) || (flag9 && block.IsCollideArrows) || (flag8 && block.IsCollideMelee) || (flag5 && voxelRayHitInfo.hit.voxelData.IsOnlyWater()) || (flag2 && flag10))
			{
				voxelRayHitInfo.tag = text;
				voxelRayHitInfo.bHitValid = true;
				voxelRayHitInfo.hit.pos = vector;
				voxelRayHitInfo.hit.distanceSq = ((vector != Vector3.zero) ? (voxelRayHitInfo.ray.origin - vector).sqrMagnitude : float.MaxValue);
				return true;
			}
			if (!voxelRayHitInfo.hit.voxelData.IsOnlyWater())
			{
				voxelRayHitInfo.fmcHit.voxelData.Clear();
				lastHitData.Clear();
			}
			ray.origin = vector + normalized * 0.01f;
			distance = ((!(phyxRaycastHit.distance > 0.01f)) ? (distance - 0.01f) : (distance - phyxRaycastHit.distance));
		}
		return false;
	}

	public static Vector3i OneVoxelStep(Vector3i _voxelPos, Vector3 _origin, Vector3 _direction, out Vector3 hitPos, out BlockFace blockFace)
	{
		Vector3i vector3i = _voxelPos;
		int num = vector3i.x;
		int num2 = vector3i.y;
		int num3 = vector3i.z;
		int num4 = Math.Sign(_direction.x);
		int num5 = Math.Sign(_direction.y);
		int num6 = Math.Sign(_direction.z);
		Vector3i vector3i2 = new Vector3i(num + ((num4 > 0) ? 1 : 0), num2 + ((num5 > 0) ? 1 : 0), num3 + ((num6 > 0) ? 1 : 0));
		Vector3 value = new Vector3((Mathf.Abs(_direction.x) > 1E-05f) ? (((float)vector3i2.x - _origin.x) / _direction.x) : float.MaxValue, (Mathf.Abs(_direction.y) > 1E-05f) ? (((float)vector3i2.y - _origin.y) / _direction.y) : float.MaxValue, (Mathf.Abs(_direction.z) > 1E-05f) ? (((float)vector3i2.z - _origin.z) / _direction.z) : float.MaxValue);
		Vector3 value2 = new Vector3((Mathf.Abs(_direction.x) > 1E-05f) ? ((float)num4 / _direction.x) : float.MaxValue, (Mathf.Abs(_direction.y) > 1E-05f) ? ((float)num5 / _direction.y) : float.MaxValue, (Mathf.Abs(_direction.z) > 1E-05f) ? ((float)num6 / _direction.z) : float.MaxValue);
		hitPos = _origin;
		blockFace = BlockFace.Top;
		if (value.x < value.y && value.x < value.z && num4 != 0)
		{
			num += num4;
			hitPos = _origin + value.x * _direction;
			value.x += value2.x;
			blockFace = ((num4 > 0) ? BlockFace.West : BlockFace.East);
		}
		else if (value.y < value.z && num5 != 0)
		{
			num2 += num5;
			hitPos = _origin + value.y * _direction;
			value.y += value2.y;
			blockFace = ((num5 > 0) ? BlockFace.Bottom : BlockFace.Top);
		}
		else
		{
			if (num6 == 0)
			{
				Log.Error("Voxel error: GetNextBlockHit, tMax=" + value.ToCultureInvariantString() + ", tDelta=" + value2.ToCultureInvariantString());
				return Vector3i.zero;
			}
			num3 += num6;
			hitPos = _origin + value.z * _direction;
			value.z += value2.z;
			blockFace = ((num6 > 0) ? BlockFace.South : BlockFace.North);
		}
		return new Vector3i(num, num2, num3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int calcBestNormalToRaycastHit(ChunkCluster _cc)
	{
		float num = 0f;
		int result = 0;
		for (int i = 0; i < normals.Length; i++)
		{
			float num2 = Vector3.Dot(_cc.ToLocalVector(phyxRaycastHit.normal), normals[i]);
			if (num2 > 0f && num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static bool RaycastOnVoxels(World _world, Ray ray, float distance, int _layerMask, int _hitMask, float _sphereSize)
	{
		bool flag = _sphereSize > 0f;
		Vector3 zero = Vector3.zero;
		string empty = string.Empty;
		Transform transform = null;
		if (flag ? Physics.SphereCast(new Ray(ray.origin - Origin.position, ray.direction), _sphereSize, out phyxRaycastHit, distance, _layerMask) : Physics.Raycast(new Ray(ray.origin - Origin.position, ray.direction), out phyxRaycastHit, distance, _layerMask))
		{
			transform = phyxRaycastHit.collider.transform;
			zero = phyxRaycastHit.point + Origin.position;
			empty = phyxRaycastHit.collider.transform.tag;
			if (!GameManager.bVolumeBlocksEditing)
			{
				voxelRayHitInfo.bHitValid = true;
				voxelRayHitInfo.tag = empty;
				voxelRayHitInfo.hit.pos = zero;
				voxelRayHitInfo.transform = transform;
				return true;
			}
			if (transform.gameObject.layer == 28)
			{
				voxelRayHitInfo.bHitValid = true;
				voxelRayHitInfo.tag = empty;
				voxelRayHitInfo.hit.pos = zero;
				voxelRayHitInfo.transform = transform;
				voxelRayHitInfo.hit.blockPos = World.worldToBlockPos(zero - ray.direction * 0.1f);
				voxelRayHitInfo.lastBlockPos = voxelRayHitInfo.hit.blockPos;
				voxelRayHitInfo.hit.voxelData.Clear();
				return true;
			}
			Vector3 vector = Vector3.zero;
			string tag = string.Empty;
			if (GetNextBlockHit(_world, ray, distance, _hitMask, flag))
			{
				tag = "B_Mesh";
				vector = voxelRayHitInfo.hit.pos;
			}
			voxelRayHitInfo.ray = ray;
			float num = ((zero != Vector3.zero) ? (ray.origin - zero).sqrMagnitude : float.MaxValue);
			float num2 = ((vector != Vector3.zero) ? (ray.origin - vector).sqrMagnitude : float.MaxValue);
			if (num < num2)
			{
				DecoObject _decoObject;
				if (empty == "T_Block")
				{
					voxelRayHitInfo.tag = "B_Mesh";
					GameUtils.FindMasterBlockForEntityModelBlock(_world, ray.direction.normalized, empty, zero, transform, voxelRayHitInfo);
				}
				else if (empty == "T_Deco" && DecoManager.Instance.GetParentBlockOfDecoration(transform, out voxelRayHitInfo.hit.blockPos, out _decoObject))
				{
					voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(_world, voxelRayHitInfo.hit.blockPos);
					voxelRayHitInfo.tag = empty;
					voxelRayHitInfo.transform = transform;
					if (voxelRayHitInfo.hit.voxelData.IsOnlyAir())
					{
						BlockValue bv = _decoObject.bv;
						bv.damage = _decoObject.bv.Block.MaxDamage - 1;
						voxelRayHitInfo.hit.voxelData.Set(bv, voxelRayHitInfo.hit.voxelData.WaterValue);
					}
				}
				else
				{
					voxelRayHitInfo.tag = empty;
					voxelRayHitInfo.transform = transform;
				}
				voxelRayHitInfo.bHitValid = true;
				voxelRayHitInfo.hit.pos = zero;
				voxelRayHitInfo.hit.distanceSq = num;
				voxelRayHitInfo.fmcHit.blockPos = voxelRayHitInfo.hit.blockPos;
				voxelRayHitInfo.fmcHit.voxelData = voxelRayHitInfo.hit.voxelData;
				voxelRayHitInfo.fmcHit.pos = voxelRayHitInfo.hit.pos;
				return true;
			}
			if (num2 != float.MaxValue)
			{
				voxelRayHitInfo.bHitValid = true;
				voxelRayHitInfo.tag = tag;
				voxelRayHitInfo.hit.pos = vector;
				voxelRayHitInfo.hit.distanceSq = num2;
				return true;
			}
			return false;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GetNextBlockHit(World _worldData, Ray ray, float _distance, int _hitMask, bool bCastSphere)
	{
		bool flag = (_hitMask & 1) != 0;
		bool flag2 = (_hitMask & 0x40) != 0;
		bool flag3 = (_hitMask & 4) != 0;
		bool flag4 = (_hitMask & 2) != 0;
		bool flag5 = (_hitMask & 8) != 0;
		bool flag6 = (_hitMask & 0x10) != 0;
		bool flag7 = (_hitMask & 0x20) != 0;
		bool flag8 = (_hitMask & 0x80) != 0;
		bool flag9 = (_hitMask & 0x100) != 0;
		voxelRayHitInfo.Clear();
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(ray.origin.x), Utils.Fastfloor(ray.origin.y), Utils.Fastfloor(ray.origin.z));
		int num = vector3i.x;
		int num2 = vector3i.y;
		int num3 = vector3i.z;
		int num4 = Math.Sign(ray.direction.x);
		int num5 = Math.Sign(ray.direction.y);
		int num6 = Math.Sign(ray.direction.z);
		Vector3i vector3i2 = new Vector3i(num + ((num4 > 0) ? 1 : 0), num2 + ((num5 > 0) ? 1 : 0), num3 + ((num6 > 0) ? 1 : 0));
		Vector3 value = new Vector3((Mathf.Abs(ray.direction.x) > 1E-05f) ? (((float)vector3i2.x - ray.origin.x) / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? (((float)vector3i2.y - ray.origin.y) / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? (((float)vector3i2.z - ray.origin.z) / ray.direction.z) : float.MaxValue);
		Vector3 value2 = new Vector3((Mathf.Abs(ray.direction.x) > 1E-05f) ? ((float)num4 / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? ((float)num5 / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? ((float)num6 / ray.direction.z) : float.MaxValue);
		Vector3 vector = ray.origin;
		float num7 = _distance * _distance;
		while (true)
		{
			HitInfoDetails.VoxelData voxelData = HitInfoDetails.VoxelData.GetFrom(_worldData, new Vector3i(num, num2, num3));
			if (!voxelData.IsOnlyAir())
			{
				if (flag9)
				{
					voxelRayHitInfo.hit.blockPos = new Vector3i(num, num2, num3);
					voxelRayHitInfo.hit.voxelData = voxelData;
					voxelRayHitInfo.bHitValid = true;
					voxelRayHitInfo.hit.pos = vector;
					return true;
				}
				Block block = voxelData.BlockValue.Block;
				if (voxelData.IsOnlyWater() && voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
				{
					voxelRayHitInfo.fmcHit.blockPos = new Vector3i(num, num2, num3);
					voxelRayHitInfo.fmcHit.voxelData = voxelData;
					voxelRayHitInfo.fmcHit.blockFace = voxelRayHitInfo.hit.blockFace;
					voxelRayHitInfo.fmcHit.pos = vector;
				}
				bool flag10 = false;
				flag10 = block.IsSeeThrough(_worldData, 0, new Vector3i(num, num2, num3), voxelData.BlockValue);
				if (((flag2 && block.IsCollideMovement && (flag || !flag10)) || (flag3 && !block.IsCollideMovement && !voxelData.IsOnlyWater()) || (flag5 && block.IsCollideBullets) || (flag6 && block.IsCollideRockets) || (flag7 && block.IsCollideArrows) || (flag8 && block.IsCollideMelee) || (flag4 && voxelData.IsOnlyWater()) || (flag && flag10)) && block.intersectRayWithBlock(voxelData.BlockValue, num, num2, num3, ray, out var _, _worldData))
				{
					voxelRayHitInfo.hit.blockPos = new Vector3i(num, num2, num3);
					voxelRayHitInfo.hit.voxelData = voxelData;
					voxelRayHitInfo.bHitValid = true;
					voxelRayHitInfo.hit.pos = vector;
					if (voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
					{
						voxelRayHitInfo.fmcHit.blockPos = new Vector3i(num, num2, num3);
						voxelRayHitInfo.fmcHit.voxelData = voxelData;
						voxelRayHitInfo.fmcHit.blockFace = voxelRayHitInfo.hit.blockFace;
						voxelRayHitInfo.fmcHit.pos = vector;
					}
					return true;
				}
			}
			if ((vector - ray.origin).sqrMagnitude > num7)
			{
				return false;
			}
			voxelRayHitInfo.lastBlockPos = new Vector3i(num, num2, num3);
			if (value.x < value.y && value.x < value.z && num4 != 0)
			{
				num += num4;
				vector = ray.origin + value.x * ray.direction;
				value.x += value2.x;
				voxelRayHitInfo.hit.blockFace = ((num4 > 0) ? BlockFace.West : BlockFace.East);
				continue;
			}
			if (value.y < value.z && num5 != 0)
			{
				num2 += num5;
				vector = ray.origin + value.y * ray.direction;
				value.y += value2.y;
				voxelRayHitInfo.hit.blockFace = ((num5 > 0) ? BlockFace.Bottom : BlockFace.Top);
				continue;
			}
			if (num6 == 0)
			{
				break;
			}
			num3 += num6;
			vector = ray.origin + value.z * ray.direction;
			value.z += value2.z;
			voxelRayHitInfo.hit.blockFace = ((num6 > 0) ? BlockFace.South : BlockFace.North);
		}
		Log.Error("Voxel error: GetNextBlockHit, tMax=" + value.ToCultureInvariantString() + ", tDelta=" + value2.ToCultureInvariantString());
		return false;
	}

	public static void GetCellsOnRay(Ray ray, DeletageNextBlockHit _delegateCallback)
	{
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(ray.origin.x), Utils.Fastfloor(ray.origin.y), Utils.Fastfloor(ray.origin.z));
		int num = vector3i.x;
		int num2 = vector3i.y;
		int num3 = vector3i.z;
		int num4 = Math.Sign(ray.direction.x);
		int num5 = Math.Sign(ray.direction.y);
		int num6 = Math.Sign(ray.direction.z);
		Vector3i vector3i2 = new Vector3i(num + ((num4 > 0) ? 1 : 0), num2 + ((num5 > 0) ? 1 : 0), num3 + ((num6 > 0) ? 1 : 0));
		Vector3 value = new Vector3((Mathf.Abs(ray.direction.x) > 1E-05f) ? (((float)vector3i2.x - ray.origin.x) / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? (((float)vector3i2.y - ray.origin.y) / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? (((float)vector3i2.z - ray.origin.z) / ray.direction.z) : float.MaxValue);
		Vector3 value2 = new Vector3((Mathf.Abs(ray.direction.x) > 1E-05f) ? ((float)num4 / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? ((float)num5 / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? ((float)num6 / ray.direction.z) : float.MaxValue);
		while (_delegateCallback(num, num2, num3))
		{
			if (value.x < value.y && value.x < value.z && num4 != 0)
			{
				num += num4;
				value.x += value2.x;
				continue;
			}
			if (value.y < value.z && num5 != 0)
			{
				num2 += num5;
				value.y += value2.y;
				continue;
			}
			if (num6 != 0)
			{
				num3 += num6;
				value.z += value2.z;
				continue;
			}
			Log.Error("Voxel error: GetCellsOnRay, tMax=" + value.ToCultureInvariantString() + ", tDelta=" + value2.ToCultureInvariantString());
			break;
		}
	}

	public static bool BlockHit(WorldRayHitInfo hitInfo, Vector3i blockPos)
	{
		HitInfoDetails.VoxelData voxelData = HitInfoDetails.VoxelData.GetFrom(GameManager.Instance.World, blockPos);
		if (voxelData.IsOnlyAir())
		{
			hitInfo.bHitValid = false;
			return false;
		}
		hitInfo.bHitValid = true;
		hitInfo.tag = "B_Mesh";
		hitInfo.hit.pos = World.blockToTransformPos(blockPos);
		hitInfo.hit.blockPos = blockPos;
		hitInfo.hit.voxelData = voxelData;
		hitInfo.fmcHit = hitInfo.hit;
		return true;
	}

	public static int ToHitMask(string _maskNames)
	{
		int num = 0;
		if (_maskNames.Length > 0)
		{
			string[] array = _maskNames.Split(hitMaskSeparator, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i])
				{
				case "Arrow":
					num |= 0x20;
					break;
				case "Bullet":
					num |= 8;
					break;
				case "LiquidOnly":
					num |= 2;
					break;
				case "Melee":
					num |= 0x80;
					break;
				case "Moveable":
					num |= 4;
					break;
				case "NotMoveable":
					num |= 0x40;
					break;
				case "Rocket":
					num |= 0x10;
					break;
				case "Transparent":
					num |= 1;
					break;
				}
			}
		}
		return num;
	}
}
