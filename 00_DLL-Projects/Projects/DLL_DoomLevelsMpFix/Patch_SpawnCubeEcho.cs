using System;
using System.IO;
using DoomLevels;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The boss cube is a local object. On a dedicated server it still times the enemy
	/// spawn, and the sound is broadcast from there. Remote clients need their own mesh.
	/// They do not spawn a second enemy.
	/// </summary>
	internal static class CubeEcho
	{
		private const string BundleName = "doommodels";
		private const string PrefabKey = "bosscube";

		private static GameObject _prefab;
		private static bool _looked;
		private static bool _missingLogged;

		internal static void Send(Vector3 from, Vector3 to)
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			World world = GameManager.Instance?.World;
			if (net == null || !net.IsServer || world?.Players?.list == null || SpawnCube.Active.Count == 0)
			{
				return;
			}

			SpawnCube cube = SpawnCube.Active[SpawnCube.Active.Count - 1];
			float travel = Traverse.Create(cube).Field("_travel").GetValue<float>();
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player == null || !player.isEntityRemote || !Instances.Inside(player.entityId, from))
				{
					continue;
				}

				net.SendPackage(PackageEmit.Take<NetPackageDoomSpawnCube>().Setup(from, to, travel), false, player.entityId);
			}
		}

		internal static void Show(Vector3 from, Vector3 to, float travel)
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}

			GameObject prefab = Prefab();
			if (prefab == null)
			{
				return;
			}

			GameObject go = UnityEngine.Object.Instantiate(prefab, from, Quaternion.identity);
			go.name = "doom_spawn_cube_echo";
			CubeEchoMotion motion = go.AddComponent<CubeEchoMotion>();
			motion.Begin(from, to, travel);
		}

		private static GameObject Prefab()
		{
			if (_looked)
			{
				return _prefab;
			}

			_looked = true;
			Type bundleType = Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule");
			if (bundleType == null)
			{
				return null;
			}

			var loaded = bundleType.GetMethod("GetAllLoadedAssetBundles", Type.EmptyTypes)?.Invoke(null, null) as System.Collections.IEnumerable;
			if (loaded == null)
			{
				return null;
			}

			foreach (object bundle in loaded)
			{
				if (bundle == null)
				{
					continue;
				}

				string name = bundleType.GetProperty("name")?.GetValue(bundle, null) as string;
				if (name != BundleName)
				{
					continue;
				}

				var load = bundleType.GetMethod("LoadAsset", new[] { typeof(string), typeof(Type) });
				_prefab = load?.Invoke(bundle, new object[] { PrefabKey, typeof(GameObject) }) as GameObject;
				break;
			}

			if (_prefab == null && !_missingLogged)
			{
				_missingLogged = true;
				Log.Warning("[DoomMultiplayer] spawn cube prefab '" + PrefabKey + "' not in " + BundleName);
			}

			return _prefab;
		}
	}

	internal sealed class CubeEchoMotion : MonoBehaviour
	{
		private const float SpinDegreesPerSecond = 220f;
		private const float LightRange = 8f;
		private const float LightIntensity = 2.2f;
		private const float FlickerHz = 9f;

		private Vector3 _from;
		private Vector3 _to;
		private float _travel;
		private float _elapsed;
		private Light _light;

		internal void Begin(Vector3 from, Vector3 to, float travel)
		{
			_from = from;
			_to = to;
			_travel = Mathf.Max(0.2f, travel);
			_light = gameObject.AddComponent<Light>();
			_light.type = LightType.Point;
			_light.color = new Color(1f, 0.55f, 0.2f);
			_light.range = LightRange;
			_light.intensity = LightIntensity;
			_light.shadows = LightShadows.None;
			_light.renderMode = LightRenderMode.ForcePixel;
		}

		private void Update()
		{
			_elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(_elapsed / _travel);
			transform.position = Vector3.Lerp(_from, _to, t) - Origin.position;
			transform.Rotate(SpinDegreesPerSecond * Time.deltaTime, SpinDegreesPerSecond * 0.7f * Time.deltaTime, 0f, Space.Self);
			if (_light != null)
			{
				_light.intensity = LightIntensity * (0.85f + 0.15f * Mathf.Sin(Time.time * FlickerHz * Mathf.PI * 2f));
			}

			if (t < 1f)
			{
				return;
			}

			Destroy(gameObject);
		}
	}

	[Preserve]
	public abstract class NetPackageDoomSpawnCube : NetPackage
	{
		private float _fx;
		private float _fy;
		private float _fz;
		private float _tx;
		private float _ty;
		private float _tz;
		private float _travel;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		internal NetPackageDoomSpawnCube Setup(Vector3 from, Vector3 to, float travel)
		{
			_fx = from.x;
			_fy = from.y;
			_fz = from.z;
			_tx = to.x;
			_ty = to.y;
			_tz = to.z;
			_travel = travel;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_fx = _br.ReadSingle();
			_fy = _br.ReadSingle();
			_fz = _br.ReadSingle();
			_tx = _br.ReadSingle();
			_ty = _br.ReadSingle();
			_tz = _br.ReadSingle();
			_travel = _br.ReadSingle();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			((BinaryWriter)_bw).Write(_fx);
			((BinaryWriter)_bw).Write(_fy);
			((BinaryWriter)_bw).Write(_fz);
			((BinaryWriter)_bw).Write(_tx);
			((BinaryWriter)_bw).Write(_ty);
			((BinaryWriter)_bw).Write(_tz);
			((BinaryWriter)_bw).Write(_travel);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			CubeEcho.Show(new Vector3(_fx, _fy, _fz), new Vector3(_tx, _ty, _tz), _travel);
		}

		public int Length()
		{
			return 28;
		}
	}
}
