using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;

public class ParticleEffect
{
	public enum Attachment : byte
	{
		None,
		Head,
		Pelvis
	}

	public struct EntityData
	{
		public int id;

		public Transform t;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string prefix = "p_";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEntitySameParticleMax = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleType type;

	[PublicizedFrom(EAccessModifier.Private)]
	public Attachment attachment;

	public Vector3 pos;

	public Quaternion rot;

	public Color color;

	public float lightValue;

	public int ParticleId;

	public string soundName;

	public int opqueTextureId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int parentEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentTransform;

	public string debugName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform RootT;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Transform> loadedTs = new Dictionary<int, Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, List<EntityData>> entityParticles = new Dictionary<int, List<EntityData>>();

	public static void Init()
	{
		RootT = GameObject.Find("/Particles").transform;
		Origin.Add(RootT, 0);
		entityParticles.Clear();
	}

	public static IEnumerator LoadResources()
	{
		loadedTs.Clear();
		LoadManager.AddressableAssetsRequestTask<GameObject> loadTask = LoadManager.LoadAssetsFromAddressables<GameObject>("particleeffects", [PublicizedFrom(EAccessModifier.Internal)] (string address) =>
		{
			if (!address.EndsWith(".prefab"))
			{
				return false;
			}
			StringSpan.CharSplitEnumerator splitEnumerator = ((StringSpan)address).GetSplitEnumerator('/');
			if (!splitEnumerator.MoveNext())
			{
				Log.Error("Particle effect at " + address + " did not have expected folder name");
				return false;
			}
			if (!splitEnumerator.MoveNext())
			{
				Log.Error("Particle effect at " + address + " did not have expected name format");
				return false;
			}
			StringSpan current2 = splitEnumerator.Current;
			StringSpan value = prefix;
			return current2.IndexOf(value) == 0;
		});
		while (!loadTask.IsDone)
		{
			yield return null;
		}
		List<GameObject> list = new List<GameObject>();
		loadTask.CollectResults(list);
		foreach (GameObject item in list)
		{
			string name = item.name;
			name = name.Substring(prefix.Length);
			int key = ToId(name);
			if (loadedTs.ContainsKey(key))
			{
				Log.Error("Particle Effect " + name + " already exists! Skipping it!");
			}
			else
			{
				loadedTs.Add(key, item.transform);
			}
		}
		yield return null;
	}

	public static void LoadAsset(string _path)
	{
		DataLoader.DataPathIdentifier identifier = DataLoader.ParseDataPathIdentifier(_path);
		if (identifier.IsBundle)
		{
			int key = ToId(_path);
			if (loadedTs.ContainsKey(key))
			{
				Log.Warning("Particle Effect {0} already exists! Skipping it!", _path);
			}
			else
			{
				Transform value = DataLoader.LoadAsset<Transform>(identifier);
				loadedTs.Add(key, value);
			}
		}
	}

	public ParticleEffect()
	{
	}

	public ParticleEffect(ParticleType _type, Vector3 _pos, float _lightValue, Color _color)
	{
		type = _type;
		pos = _pos;
		lightValue = _lightValue;
		color = _color;
	}

	public ParticleEffect(ParticleType _type, Vector3 _pos, float _lightValue, Color _color, Transform _parentTransform)
		: this(_type, _pos, _lightValue, _color)
	{
		SetParent(_parentTransform);
	}

	public ParticleEffect(ParticleType _type, Vector3 _pos, float _lightValue, Color _color, string _soundName, Transform _parentTransform)
		: this(_type, _pos, _lightValue, _color)
	{
		soundName = _soundName;
		SetParent(_parentTransform);
	}

	public ParticleEffect(string _name, Vector3 _pos, Quaternion _rot, float _lightValue, Color _color)
		: this(_name, _pos, _rot, _lightValue, _color, null, null)
	{
	}

	public ParticleEffect(string _name, Vector3 _pos, Quaternion _rot, float _lightValue, Color _color, string _soundName, Transform _parentTransform)
		: this(_name, _pos, _lightValue, _color, _soundName, _parentTransform, _OLDCreateColliders: false)
	{
		rot = _rot;
	}

	public ParticleEffect(string _name, Vector3 _pos, float _lightValue, Color _color, string _soundName, Transform _parentTransform, bool _OLDCreateColliders)
		: this(ParticleType.Dynamic, _pos, _lightValue, _color)
	{
		ParticleId = ((_name != null) ? ToId(_name) : 0);
		debugName = _name;
		soundName = _soundName;
		SetParent(_parentTransform);
	}

	public ParticleEffect(string _name, Vector3 _pos, float _lightValue, Color _color, string _soundName, int _parentEntityId, Attachment _attachment)
		: this(ParticleType.Dynamic, _pos, _lightValue, _color)
	{
		ParticleId = ((_name != null) ? ToId(_name) : 0);
		debugName = _name;
		soundName = _soundName;
		SetParent(_parentEntityId, _attachment);
	}

	public static int ToId(string _name)
	{
		return _name.GetHashCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetParentTransform()
	{
		if (!parentTransform && parentEntityId != -1)
		{
			GameManager.Instance.World.Entities.dict.TryGetValue(parentEntityId, out var value);
			if ((bool)value)
			{
				parentTransform = value.transform;
				if (attachment != Attachment.None)
				{
					Transform transform = attachment switch
					{
						Attachment.Head => value.emodel.GetHeadTransform(), 
						Attachment.Pelvis => value.emodel.GetPelvisTransform(), 
						_ => null, 
					};
					if ((bool)transform)
					{
						parentTransform = transform;
					}
				}
			}
		}
		return parentTransform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParent(Transform _parentT)
	{
		parentTransform = null;
		attachment = Attachment.None;
		if ((bool)_parentT)
		{
			Entity component = _parentT.GetComponent<Entity>();
			parentEntityId = (component ? component.entityId : (-1));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParent(int _entityId, Attachment _attachment = Attachment.None)
	{
		parentTransform = null;
		attachment = _attachment;
		parentEntityId = _entityId;
	}

	public static Transform GetDynamicTransform(int _particleId)
	{
		if (loadedTs.TryGetValue(_particleId, out var value))
		{
			return value;
		}
		Log.Error($"Unknown particle effect: {_particleId}");
		return null;
	}

	public static bool IsAvailable(string _name)
	{
		return loadedTs.ContainsKey(ToId(_name));
	}

	public void Read(BinaryReader _br)
	{
		ParticleId = _br.ReadInt32();
		pos = StreamUtils.ReadVector3(_br);
		rot = StreamUtils.ReadQuaterion(_br);
		color = StreamUtils.ReadColor32(_br);
		soundName = _br.ReadString();
		if (soundName == string.Empty)
		{
			soundName = null;
		}
		parentEntityId = _br.ReadInt32();
		attachment = (Attachment)_br.ReadByte();
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(ParticleId);
		StreamUtils.Write(_bw, pos);
		StreamUtils.Write(_bw, rot);
		StreamUtils.WriteColor32(_bw, color);
		_bw.Write((soundName != null) ? soundName : string.Empty);
		_bw.Write(parentEntityId);
		_bw.Write((byte)attachment);
	}

	public static Transform SpawnParticleEffect(ParticleEffect _pe, int _entityThatCausedIt, bool _forceCreation = false, bool _isWorldPos = false)
	{
		if (_pe.soundName != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.World.aiDirector.OnSoundPlayedAtPosition(_entityThatCausedIt, _pe.pos, _pe.soundName, 1f);
		}
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(_pe.soundName))
		{
			Manager.Play(_pe.pos, _pe.soundName, _entityThatCausedIt);
		}
		List<EntityData> value = null;
		Transform t;
		if (_entityThatCausedIt != -1 && !_forceCreation)
		{
			int num = -1;
			if (entityParticles.TryGetValue(_entityThatCausedIt, out value))
			{
				int num2 = 0;
				for (int num3 = value.Count - 1; num3 >= 0; num3--)
				{
					if (!value[num3].t)
					{
						value.RemoveAt(num3);
						num--;
					}
					else if (value[num3].id == _pe.ParticleId && ++num2 >= 3)
					{
						num = num3;
					}
				}
			}
			else
			{
				value = new List<EntityData>();
				entityParticles[_entityThatCausedIt] = value;
			}
			if (num >= 0 && _pe.attachment == Attachment.None)
			{
				EntityData item = value[num];
				value.RemoveAt(num);
				value.Add(item);
				t = item.t;
				t.position = _pe.pos - Origin.position;
				t.rotation = _pe.rot;
				ParticleSystem[] componentsInChildren = t.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem obj in componentsInChildren)
				{
					obj.Clear();
					obj.Play();
				}
				TemporaryObject[] componentsInChildren2 = t.GetComponentsInChildren<TemporaryObject>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].Restart();
				}
				return null;
			}
		}
		Transform dynamicTransform = GetDynamicTransform(_pe.ParticleId);
		if (!dynamicTransform)
		{
			return null;
		}
		t = ((!_isWorldPos) ? Object.Instantiate(dynamicTransform, _pe.pos, _pe.rot) : Object.Instantiate(dynamicTransform, _pe.pos - Origin.position, _pe.rot));
		if (!t)
		{
			return null;
		}
		if (value != null)
		{
			EntityData item2 = default(EntityData);
			item2.id = _pe.ParticleId;
			item2.t = t;
			value.Add(item2);
		}
		Renderer[] componentsInChildren3 = t.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren3)
		{
			if (renderer.GetComponent<ParticleSystem>() == null)
			{
				renderer.material.SetColor("_Color", _pe.color);
			}
		}
		if (_pe.opqueTextureId != 0)
		{
			Material material = t.GetComponent<ParticleSystem>().GetComponent<Renderer>().material;
			TextureAtlas textureAtlas = MeshDescription.meshes[0].textureAtlas;
			material.SetTexture("_MainTex", textureAtlas.diffuseTexture);
			material.SetTexture("_BumpMap", textureAtlas.normalTexture);
			material.SetFloat("_TexI", textureAtlas.uvMapping[_pe.opqueTextureId].index);
			if (material.HasProperty("_OffsetUV"))
			{
				Rect uv = textureAtlas.uvMapping[_pe.opqueTextureId].uv;
				material.SetVector("_OffsetUV", new Vector4(uv.x, uv.y, uv.width, uv.height));
			}
		}
		Transform transform = _pe.GetParentTransform();
		if ((bool)transform)
		{
			t.SetParent(transform, worldPositionStays: false);
			if (_pe.attachment != Attachment.None)
			{
				t.localPosition = _pe.pos;
			}
			else
			{
				t.localPosition = Vector3.zero;
			}
			t.localRotation = Quaternion.identity;
		}
		else
		{
			t.SetParent(RootT, worldPositionStays: false);
		}
		return t;
	}
}
