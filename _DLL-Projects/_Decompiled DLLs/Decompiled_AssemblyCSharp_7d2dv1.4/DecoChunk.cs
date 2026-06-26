using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DecoChunk
{
	public int decoChunkX;

	public int decoChunkZ;

	public int drawX;

	public int drawZ;

	public bool isDecorated;

	public bool isModelsUpdated;

	public bool isGameObjectUpdated;

	public GameObject rootObj;

	public Dictionary<long, List<DecoObject>> decosPerSmallChunks = new Dictionary<long, List<DecoObject>>(64);

	public OcclusionManager.OccludeeZone occludeeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Transform> occlusionTs = new List<Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GameObjectPool.AsyncItem> asyncItems = new List<GameObjectPool.AsyncItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, List<DecoObject>> models = new Dictionary<string, List<DecoObject>>();

	public DecoChunk(int _x, int _z, int _drawX, int _drawZ)
	{
		Reset(_x, _z, _drawX, _drawZ);
	}

	public void Reset(int _x, int _z, int _drawX, int _drawZ)
	{
		decoChunkX = _x;
		decoChunkZ = _z;
		drawX = _drawX;
		drawZ = _drawZ;
		decosPerSmallChunks.Clear();
		isDecorated = false;
		isModelsUpdated = false;
		isGameObjectUpdated = false;
	}

	public void RestoreGeneratedDecos(Predicate<DecoObject> decoObjectValidator = null)
	{
		foreach (long key in decosPerSmallChunks.Keys)
		{
			RestoreGeneratedDecos(key, decoObjectValidator);
		}
	}

	public void RestoreGeneratedDecos(long smallChunkKey, Predicate<DecoObject> decoObjectValidator = null)
	{
		if (!decosPerSmallChunks.TryGetValue(smallChunkKey, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			DecoObject decoObject = value[num];
			if (decoObjectValidator == null || decoObjectValidator(decoObject))
			{
				switch (decoObject.state)
				{
				case DecoState.GeneratedInactive:
					decoObject.state = DecoState.GeneratedActive;
					isModelsUpdated = false;
					break;
				case DecoState.Dynamic:
					RemoveDecoObject(decoObject);
					break;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int MakeKey16(int _x, int _z)
	{
		return (_x << 16) | (_z & 0xFFFF);
	}

	public static int ToDecoChunkPos(float _worldPos)
	{
		return Utils.Fastfloor(_worldPos / 128f);
	}

	public static int ToDecoChunkPos(int _worldPos)
	{
		if (_worldPos >= 0)
		{
			return _worldPos / 128;
		}
		return (_worldPos - 128 + 1) / 128;
	}

	public void UpdateGameObject()
	{
		if (!rootObj)
		{
			rootObj = new GameObject();
		}
		SetVisible(_bVisible: true);
		rootObj.name = "DecoC_" + decoChunkX + "_" + decoChunkZ;
		rootObj.transform.position = new Vector3(drawX * 128, 0f, drawZ * 128) - Origin.position;
		isGameObjectUpdated = true;
	}

	public IEnumerator UpdateModels(World _world, MicroStopwatch ms)
	{
		SetVisible(_bVisible: true);
		foreach (KeyValuePair<long, List<DecoObject>> decosPerSmallChunk in decosPerSmallChunks)
		{
			List<DecoObject> value = decosPerSmallChunk.Value;
			for (int i = 0; i < value.Count; i++)
			{
				DecoObject decoObject = value[i];
				if (decoObject.state != DecoState.GeneratedInactive && !decoObject.go && decoObject.asyncItem == null)
				{
					string modelName = decoObject.GetModelName();
					if (!models.TryGetValue(modelName, out var value2))
					{
						value2 = new List<DecoObject>();
						models.Add(modelName, value2);
					}
					value2.Add(decoObject);
				}
			}
		}
		foreach (KeyValuePair<string, List<DecoObject>> model in models)
		{
			List<DecoObject> value3 = model.Value;
			GameObjectPool.AsyncItem objectsForTypeAsync = GameObjectPool.Instance.GetObjectsForTypeAsync(model.Key, value3.Count, CreateGameObjectCallback, value3);
			if (objectsForTypeAsync != null)
			{
				asyncItems.Add(objectsForTypeAsync);
				for (int j = 0; j < value3.Count; j++)
				{
					value3[j].asyncItem = objectsForTypeAsync;
				}
			}
			if (ms.ElapsedMicroseconds > 900)
			{
				yield return null;
				ms.ResetAndRestart();
			}
		}
		models.Clear();
		isModelsUpdated = true;
	}

	public void CreateGameObjectCallback(object _userData, UnityEngine.Object[] _objs, int _objsCount, bool _isAsync)
	{
		List<DecoObject> list = (List<DecoObject>)_userData;
		Transform transform = rootObj.transform;
		for (int i = 0; i < _objsCount; i++)
		{
			GameObject gameObject = (GameObject)_objs[i];
			list[i].CreateGameObjectCallback(gameObject, transform, _isAsync);
			occlusionTs.Add(gameObject.transform);
		}
		if (occlusionTs.Count > 0)
		{
			if (OcclusionManager.Instance.cullDecorations)
			{
				OcclusionManager.Instance.AddDeco(this, occlusionTs);
			}
			occlusionTs.Clear();
		}
	}

	public void AddDecoObject(DecoObject _decoObject, bool _tryInstantiate = false)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(_decoObject.pos.x), World.toChunkXZ(_decoObject.pos.z));
		if (!decosPerSmallChunks.TryGetValue(key, out var value))
		{
			value = new List<DecoObject>(64);
			decosPerSmallChunks.Add(key, value);
		}
		value.Add(_decoObject);
		if (!_tryInstantiate)
		{
			return;
		}
		if (ThreadManager.IsMainThread() && (bool)rootObj)
		{
			_decoObject.CreateGameObject(this, rootObj.transform);
			if (OcclusionManager.Instance.cullDecorations && (bool)_decoObject.go)
			{
				occlusionTs.Add(_decoObject.go.transform);
				OcclusionManager.Instance.AddDeco(this, occlusionTs);
				occlusionTs.Clear();
			}
		}
		else
		{
			isModelsUpdated = false;
		}
	}

	public DecoObject GetDecoObjectAt(Vector3i _worldBlockPos)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(_worldBlockPos.x), World.toChunkXZ(_worldBlockPos.z));
		if (!decosPerSmallChunks.TryGetValue(key, out var value))
		{
			return null;
		}
		foreach (DecoObject item in value)
		{
			if (item.pos.x == _worldBlockPos.x && item.pos.z == _worldBlockPos.z && item.state != DecoState.GeneratedInactive)
			{
				return item;
			}
		}
		return null;
	}

	public bool RemoveDecoObject(Vector3i _worldBlockPos)
	{
		DecoObject decoObjectAt = GetDecoObjectAt(_worldBlockPos);
		if (decoObjectAt == null)
		{
			return false;
		}
		RemoveDecoObject(decoObjectAt);
		return true;
	}

	public void RemoveDecoObject(DecoObject deco)
	{
		if (deco.state == DecoState.Dynamic)
		{
			long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(deco.pos.x), World.toChunkXZ(deco.pos.z));
			if (decosPerSmallChunks.TryGetValue(key, out var value))
			{
				value.Remove(deco);
			}
		}
		else
		{
			deco.state = DecoState.GeneratedInactive;
		}
		if (OcclusionManager.Instance.cullDecorations && (bool)deco.go)
		{
			OcclusionManager.Instance.RemoveDeco(this, deco.go.transform);
		}
		deco.Destroy();
	}

	public void Destroy()
	{
		if (OcclusionManager.Instance.cullDecorations)
		{
			OcclusionManager.Instance.RemoveDecoChunk(this);
		}
		foreach (KeyValuePair<long, List<DecoObject>> decosPerSmallChunk in decosPerSmallChunks)
		{
			List<DecoObject> value = decosPerSmallChunk.Value;
			for (int i = 0; i < value.Count; i++)
			{
				value[i].Destroy();
			}
		}
		for (int j = 0; j < asyncItems.Count; j++)
		{
			GameObjectPool.Instance.CancelAsync(asyncItems[j]);
		}
		asyncItems.Clear();
		isModelsUpdated = false;
		isGameObjectUpdated = false;
		UnityEngine.Object.Destroy(rootObj);
	}

	public void SetVisible(bool _bVisible)
	{
		if ((bool)rootObj && rootObj.activeSelf != _bVisible)
		{
			rootObj.SetActive(_bVisible);
		}
	}

	public override string ToString()
	{
		return $"DecoChunk {decoChunkX},{decoChunkZ}";
	}
}
