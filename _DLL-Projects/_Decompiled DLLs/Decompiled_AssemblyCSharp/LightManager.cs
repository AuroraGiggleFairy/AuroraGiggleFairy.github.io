using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class LightManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum SearchDir
	{
		All,
		Forward,
		Right
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RegLightGroup
	{
		public Dictionary<Vector3, Light> lights;
	}

	[Preserve]
	public class NetPackageLight : NetPackage
	{
		public int entityId;

		public float lightLevel;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public NetPackageLight Setup(int _entityId, float _lightLevel = 0f)
		{
			entityId = _entityId;
			lightLevel = _lightLevel;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			entityId = _reader.ReadInt32();
			lightLevel = _reader.ReadSingle();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(entityId);
			_writer.Write(lightLevel);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (GameManager.Instance == null || GameManager.Instance.World == null)
			{
				return;
			}
			if (myServer != null)
			{
				Entity entity = GameManager.Instance.World.GetEntity(entityId);
				if (!(entity == null))
				{
					float num = GetLightLevel(entity.position + Vector3.up * 1.68f) + GetLightLevelFromMovingLights(entityId, entity.position + Vector3.up * 1.68f);
					num += ((EntityAlive)entity).GetLightLevel();
					myServer.SendLightLevel(entityId, num);
				}
			}
			else
			{
				SetPlayerLightLevel(lightLevel);
			}
		}

		public override int GetLength()
		{
			return 0;
		}
	}

	public class Server : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal m_localPlayer;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, Client> m_players = new Dictionary<int, Client>();

		public void SendLightLevel(int entityId, float lightLevel)
		{
			if (m_players.TryGetValue(entityId, out var value))
			{
				value.SetPlayerLightLevel(lightLevel);
			}
		}

		public void AttachLocalPlayer(EntityPlayerLocal localPlayer)
		{
			m_localPlayer = localPlayer;
		}

		public void EntityAddedToWorld(Entity entity, World world)
		{
			if (entity is EntityPlayer && (m_localPlayer == null || entity.entityId != m_localPlayer.entityId) && !m_players.TryGetValue(entity.entityId, out var value))
			{
				value = new Client(entity.entityId);
				m_players[entity.entityId] = value;
			}
		}

		public void EntityRemovedFromWorld(Entity entity, World world)
		{
			if (m_players.TryGetValue(entity.entityId, out var value))
			{
				m_players.Remove(entity.entityId);
				value.Dispose();
			}
		}

		public void Dispose()
		{
			foreach (KeyValuePair<int, Client> player in m_players)
			{
				player.Value.Dispose();
			}
			m_players = null;
			m_localPlayer = null;
		}
	}

	public class Client : IDisposable
	{
		public int entityId;

		public Client(int _entityId)
		{
			entityId = _entityId;
		}

		public void Dispose()
		{
		}

		public void SetPlayerLightLevel(float _lightLevel)
		{
			NetPackageLight package = NetPackageManager.GetPackage<NetPackageLight>().Setup(entityId, _lightLevel);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, entityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int raycastMask = 65536;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int enclosureRaycastMask = 8454417;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRegGridSize = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRegPosMask = -8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRegBins = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRegBinShift = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, RegLightGroup>[] regLightBins = new Dictionary<Vector3i, RegLightGroup>[512];

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<int, List<Light>> movingLights = new DictionaryList<int, List<Light>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Server myServer = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float PlayerLightLevel = 0f;

	public static bool ShowSearchPatternOn = false;

	public static bool ShowLightLevelOn = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Canvas debugUI = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Text debugText;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, GameObject> debugPoints = new Dictionary<Vector3i, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject LightLevelDebugPoints = new GameObject("LightLevelDebugPoints");

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3> lightsEffectingRemovals = new List<Vector3>();

	public static void Init()
	{
		for (int i = 0; i < 512; i++)
		{
			regLightBins[i] = new Dictionary<Vector3i, RegLightGroup>();
		}
	}

	public static void RegisterMovingLight(Entity owner, Light _light)
	{
		int key = ((owner == null) ? (-1) : owner.entityId);
		if (!movingLights.dict.TryGetValue(key, out var value))
		{
			value = new List<Light>();
			movingLights.Add(key, value);
		}
		value.Add(_light);
	}

	public static void UnRegisterMovingLight(Entity owner, Light _light)
	{
		int key = ((owner == null) ? (-1) : owner.entityId);
		if (movingLights.dict.TryGetValue(key, out var value))
		{
			value.Remove(_light);
			if (value.Count < 1)
			{
				movingLights.Remove(key);
			}
		}
	}

	public static float RegisterLight(Light _light)
	{
		float range = _light.range;
		if (range > 0f)
		{
			Vector3 vector = _light.transform.position + Origin.position;
			RegisterLight(vector - Vector3.one * range, _light, range, vector, SearchDir.All, _bRegister: true);
		}
		return range;
	}

	public static void UnRegisterLight(Vector3 _worldPosition, float _lightRange)
	{
		if (!(_lightRange <= 0f))
		{
			RegisterLight(_worldPosition - Vector3.one * _lightRange, null, _lightRange, _worldPosition, SearchDir.All, _bRegister: false);
		}
	}

	public static void Clear()
	{
		for (int i = 0; i < 512; i++)
		{
			regLightBins[i] = new Dictionary<Vector3i, RegLightGroup>();
		}
		movingLights.Clear();
		foreach (KeyValuePair<Vector3i, GameObject> debugPoint in debugPoints)
		{
			UnityEngine.Object.Destroy(debugPoint.Value);
		}
		debugPoints.Clear();
	}

	public static void ShowSearchPattern(bool _bShow = true)
	{
		ShowSearchPatternOn = _bShow;
		foreach (KeyValuePair<Vector3i, GameObject> debugPoint in debugPoints)
		{
			debugPoint.Value.SetActive(ShowSearchPatternOn);
		}
	}

	public static void ShowLightLevel(bool _bShow = true)
	{
		ShowLightLevelOn = _bShow;
	}

	public static void UpdateUI()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetLightLevelFromMovingLights(int _excludeEntityId, Vector3 _worldPosition)
	{
		float num = 0f;
		foreach (KeyValuePair<int, List<Light>> item in movingLights.dict)
		{
			if (item.Key == _excludeEntityId)
			{
				continue;
			}
			for (int i = 0; i < item.Value.Count; i++)
			{
				num += GetLightLevel(item.Value[i], _worldPosition, _bAllowPointToSpot: true);
				if (num >= 1f)
				{
					return 1f;
				}
			}
		}
		return Utils.FastClamp01(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetLightLevel(Light _light, Vector3 _worldPosition, bool _bAllowPointToSpot = false)
	{
		if (_light == null || !_light.isActiveAndEnabled)
		{
			return 0f;
		}
		Vector3 vector = _light.transform.position + Origin.position;
		Vector3 vector2 = _worldPosition - vector;
		float range = _light.range;
		float magnitude = vector2.magnitude;
		if (magnitude >= range)
		{
			return 0f;
		}
		Vector3 vector3 = vector;
		vector3.x = Mathf.Floor(vector3.x) + 0.02f;
		vector3.y = Mathf.Floor(vector3.y) + 0.02f;
		vector3.z = Mathf.Floor(vector3.z) + 0.02f;
		if (_worldPosition.x > vector3.x)
		{
			vector3.x += 0.96f;
		}
		if (_worldPosition.y > vector3.y)
		{
			vector3.y += 0.96f;
		}
		if (_worldPosition.z > vector3.z)
		{
			vector3.z += 0.96f;
		}
		Vector3 direction = _worldPosition - vector3;
		if (Physics.Raycast(new Ray(vector3 - Origin.position, direction), out var hitInfo, range + 0.25f, 65536) && hitInfo.distance < direction.magnitude - 0.25f)
		{
			return 0f;
		}
		LightType lightType = _light.type;
		float num = _light.spotAngle;
		if (_bAllowPointToSpot && (bool)_light.cookie)
		{
			float cookieSize = _light.cookieSize;
			float num2 = Mathf.Sqrt(Mathf.Pow(cookieSize * 0.5f, 2f) + Mathf.Pow(range, 2f));
			num = Mathf.Acos(cookieSize * 0.5f / num2) * (180f / MathF.PI);
			lightType = LightType.Spot;
		}
		Color color = _light.color;
		if (lightType != LightType.Spot)
		{
			_ = 2;
			return (1f - magnitude / range) * Utils.FastClamp01(_light.intensity) * SkyManager.GetLuma(color) * color.a;
		}
		float num3 = Vector3.Dot(_light.transform.forward, vector2.normalized);
		float num4 = 1f - num / 180f;
		num4 = Utils.FastClamp01(num4 * 1.1f);
		if (num3 < num4)
		{
			return 0f;
		}
		float f = Utils.FastClamp01(num3);
		if (num4 < 1f)
		{
			f = (num3 - num4) / (1f - num4);
		}
		f = Mathf.Pow(f, 1.25f);
		return (1f - magnitude / range) * Utils.FastClamp01(_light.intensity) * SkyManager.GetLuma(color) * f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float BlockLight(Vector3 _worldPosition)
	{
		float result = 1f;
		Vector3i blockPos = World.worldToBlockPos(_worldPosition);
		IChunk chunkFromWorldPos = GameManager.Instance.World.GetChunkFromWorldPos(blockPos);
		if (chunkFromWorldPos != null && blockPos.y >= 0 && blockPos.y < 255)
		{
			result = (int)Utils.FastMax(chunkFromWorldPos.GetLight(blockPos.x, blockPos.y, blockPos.z, Chunk.LIGHT_TYPE.SUN), chunkFromWorldPos.GetLight(blockPos.x, blockPos.y + 1, blockPos.z, Chunk.LIGHT_TYPE.SUN));
			result /= 15f;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float CalcShadeLight(Vector3 _worldPosition)
	{
		bool flag = false;
		Vector3 vector;
		if (SkyManager.GetSunIntensity() < 0.05f)
		{
			vector = SkyManager.GetMoonDirection();
			if (vector.y > -0.087f)
			{
				return 0f;
			}
			flag = true;
		}
		else
		{
			vector = SkyManager.GetSunLightDirection();
		}
		float result = ((!flag) ? 1 : (-1));
		Vector3 origin = _worldPosition - Origin.position;
		if (Physics.Raycast(new Ray(origin, -vector), out var hitInfo, float.PositiveInfinity, 8454417) && hitInfo.distance < float.PositiveInfinity)
		{
			result = 0f;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetLightLevel(Vector3 _worldPosition)
	{
		float num = 0f;
		Dictionary<Vector3, Light> lightsEffecting = GetLightsEffecting(_worldPosition);
		if (lightsEffecting != null && lightsEffecting.Count > 0)
		{
			lightsEffectingRemovals.Clear();
			foreach (KeyValuePair<Vector3, Light> item in lightsEffecting)
			{
				Light value = item.Value;
				if (value == null)
				{
					lightsEffectingRemovals.Add(item.Key);
					continue;
				}
				num += GetLightLevel(value, _worldPosition);
				if (!(num >= 1f))
				{
					continue;
				}
				break;
			}
			for (int num2 = lightsEffectingRemovals.Count - 1; num2 >= 0; num2--)
			{
				lightsEffecting.Remove(lightsEffectingRemovals[num2]);
			}
		}
		if (num >= 1f)
		{
			return 1f;
		}
		float num3 = Mathf.Pow(WorldEnvironment.AmbientTotal, 0.6f);
		num += num3 * 0.5f;
		num += BlockLight(_worldPosition) * num3 * 0.5f;
		if (num >= 1f)
		{
			return 1f;
		}
		float num4 = CalcShadeLight(_worldPosition);
		num = ((!(num4 >= 0f)) ? (num + SkyManager.GetMoonBrightness()) : (num + num4));
		return Utils.FastClamp01(num);
	}

	public static Dictionary<Vector3, Light> GetLightsEffecting(Vector3 _worldPosition)
	{
		Dictionary<Vector3i, RegLightGroup> obj = regLightBins[(Utils.Fastfloor(_worldPosition.x) >> 3) & 0x1FF];
		Vector3i key = World.worldToBlockPos(_worldPosition);
		key &= -8;
		if (!obj.TryGetValue(key, out var value))
		{
			return null;
		}
		return value.lights;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RegisterLight(Vector3 _searchPosition, Light _light, float _lightRange, Vector3 _lightPosition, SearchDir _, bool _bRegister)
	{
		int num = Mathf.CeilToInt(_lightRange * 0.89f);
		if (num <= 0)
		{
			return;
		}
		int num2 = num * 2;
		Vector3i vector3i = World.worldToBlockPos(_searchPosition);
		Vector3i vector3i2 = vector3i + new Vector3i(num2, num2, num2);
		Vector3i vector3i3 = vector3i & -8;
		Vector3i key = default(Vector3i);
		key.y = vector3i3.y;
		while (key.y <= vector3i2.y)
		{
			key.z = vector3i3.z;
			while (key.z <= vector3i2.z)
			{
				key.x = vector3i3.x;
				while (key.x <= vector3i2.x)
				{
					Dictionary<Vector3i, RegLightGroup> dictionary = regLightBins[(key.x >> 3) & 0x1FF];
					if (!dictionary.TryGetValue(key, out var value))
					{
						value.lights = new Dictionary<Vector3, Light>();
						dictionary.Add(key, value);
					}
					if (_bRegister)
					{
						value.lights[_lightPosition] = _light;
					}
					else
					{
						value.lights.Remove(_lightPosition);
						if (value.lights.Count == 0)
						{
							dictionary.Remove(key);
							value.lights = null;
						}
					}
					key.x += 8;
				}
				key.z += 8;
			}
			key.y += 8;
		}
	}

	public static void SetPlayerLightLevel(float _lightLevel)
	{
		PlayerLightLevel = _lightLevel;
	}

	public static float GetStealthLightLevel(EntityAlive _entity, out float selfLight)
	{
		if (myServer != null)
		{
			Vector3 position = _entity.position;
			position.y += 1.68f;
			float v = GetLightLevel(position) + GetLightLevelFromMovingLights(_entity.entityId, position);
			selfLight = _entity.GetLightLevel();
			return Utils.FastClamp01(v);
		}
		selfLight = 0f;
		return 0f;
	}

	public static float GetWorldLightLevelInRange(Vector3 pos, float distanceMax)
	{
		float lightLevel = GetLightLevel(pos);
		if (lightLevel >= 1f)
		{
			return 1f;
		}
		return lightLevel;
	}

	public static void LightChanged(Vector3 lightPos)
	{
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		for (int num = localPlayers.Count - 1; num >= 0; num--)
		{
			localPlayers[num].renderManager.reflectionManager.LightChanged(lightPos);
		}
	}

	public static void Dispose()
	{
		if (myServer != null)
		{
			myServer.Dispose();
			myServer = null;
		}
	}

	public static void AttachLocalPlayer(EntityPlayerLocal localPlayer, World world)
	{
		if (myServer != null)
		{
			myServer.AttachLocalPlayer(localPlayer);
		}
	}

	public static void EntityAddedToWorld(Entity entity, World world)
	{
		if (myServer != null)
		{
			myServer.EntityAddedToWorld(entity, world);
		}
	}

	public static void EntityRemovedFromWorld(Entity entity, World world)
	{
		if (!(entity == null) && myServer != null)
		{
			myServer.EntityRemovedFromWorld(entity, world);
		}
	}

	public static void CreateServer()
	{
		if (myServer == null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			myServer = new Server();
		}
	}
}
