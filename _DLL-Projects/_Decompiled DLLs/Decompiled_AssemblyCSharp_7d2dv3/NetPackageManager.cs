using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Profiling;

public static class NetPackageManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public interface IPackageInformation
	{
		NetPackage GetRawPackage();

		void FreePackage(NetPackage _package);

		void GetStats(out int _packages, out int _totalSize);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NetPackageInformation<TPackage> : IPackageInformation where TPackage : NetPackage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static NetPackageInformation<TPackage> instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly TPackage[] pool;

		[PublicizedFrom(EAccessModifier.Private)]
		public int poolSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int capacity;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ConstructorInfo ctor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSampler = CustomSampler.Create("NPM.PI.GetPackage");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSamplerPool = CustomSampler.Create("Pooled");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSamplerNew = CustomSampler.Create("New");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler getSamplerType = CustomSampler.Create(typeof(TPackage).Name);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler freeSampler = CustomSampler.Create("NPM.PI.FreePackage");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler freeSamplerPool = CustomSampler.Create("ToPool");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler freeSamplerCleanup = CustomSampler.Create("Cleanup");

		public static NetPackageInformation<TPackage> Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new NetPackageInformation<TPackage>();
				}
				return instance;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public NetPackageInformation()
		{
			Type typeFromHandle = typeof(TPackage);
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			ctor = typeFromHandle.GetConstructor(bindingAttr, null, CallingConventions.Any, Type.EmptyTypes, null);
			if (!typeof(IMemoryPoolableObject).IsAssignableFrom(typeFromHandle))
			{
				return;
			}
			capacity = 10;
			MethodInfo method = typeFromHandle.GetMethod("GetPoolSize", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
			if (method != null)
			{
				if (method.ReturnType == typeof(int))
				{
					capacity = (int)method.Invoke(null, null);
				}
				else
				{
					Log.Warning("Poolable NetPackage has GetPoolSize method with wrong return type");
				}
			}
			pool = new TPackage[capacity];
		}

		public TPackage GetPackage()
		{
			if (pool != null)
			{
				lock (pool)
				{
					if (poolSize > 0)
					{
						poolSize--;
						TPackage result = pool[poolSize];
						pool[poolSize] = null;
						return result;
					}
				}
			}
			return (TPackage)ctor.Invoke(null);
		}

		public NetPackage GetRawPackage()
		{
			return GetPackage();
		}

		public void FreePackage(NetPackage _package)
		{
			if (pool == null)
			{
				return;
			}
			IMemoryPoolableObject memoryPoolableObject = (IMemoryPoolableObject)_package;
			lock (pool)
			{
				if (poolSize < capacity)
				{
					memoryPoolableObject.Reset();
					pool[poolSize] = (TPackage)_package;
					poolSize++;
				}
				else
				{
					memoryPoolableObject.Cleanup();
				}
			}
		}

		public void GetStats(out int _packages, out int _totalSize)
		{
			_packages = 0;
			_totalSize = 0;
			if (pool == null)
			{
				return;
			}
			lock (pool)
			{
				_packages = poolSize;
			}
		}

		public void Cleanup()
		{
			if (pool == null)
			{
				return;
			}
			lock (pool)
			{
				for (int i = 0; i < poolSize; i++)
				{
					((IMemoryPoolableObject)pool[i]).Cleanup();
				}
			}
		}
	}

	public class UnknownNetPackageException : Exception
	{
		public UnknownNetPackageException(int _packageId)
			: base("Unknown NetPackage ID: " + _packageId)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Type[] packageIdToClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IPackageInformation[] packageIdToPackageInformation;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Type, int> packageClassToPackageId;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, Type> knownPackageTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Type packageIdsType;

	public static int KnownPackageCount => knownPackageTypes.Count;

	public static Type[] PackageMappings => packageIdToClass;

	[PublicizedFrom(EAccessModifier.Private)]
	static NetPackageManager()
	{
		knownPackageTypes = new CaseInsensitiveStringDictionary<Type>();
		packageIdsType = typeof(NetPackagePackageIds);
		Log.Out("NetPackageManager Init");
		ReflectionHelpers.FindTypesImplementingBase(typeof(NetPackage), [PublicizedFrom(EAccessModifier.Internal)] (Type _type) =>
		{
			knownPackageTypes.Add(_type.Name, _type);
		});
	}

	public static void ResetMappings()
	{
		packageIdToClass = null;
		packageIdToPackageInformation = null;
		packageClassToPackageId = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddPackageMapping(int _id, Type _type)
	{
		packageIdToClass[_id] = _type;
		packageClassToPackageId[_type] = _id;
		IPackageInformation packageInformation = (IPackageInformation)typeof(NetPackageInformation<>).MakeGenericType(_type).GetProperty("Instance").GetValue(null, null);
		packageIdToPackageInformation[_id] = packageInformation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupBaseMapping()
	{
		packageIdToClass = new Type[KnownPackageCount];
		packageIdToPackageInformation = new IPackageInformation[KnownPackageCount];
		packageClassToPackageId = new Dictionary<Type, int>();
		AddPackageMapping(0, packageIdsType);
	}

	public static void StartServer()
	{
		ResetMappings();
		SetupBaseMapping();
		int num = 1;
		foreach (KeyValuePair<string, Type> knownPackageType in knownPackageTypes)
		{
			if (!(knownPackageType.Value == packageIdsType))
			{
				AddPackageMapping(num, knownPackageType.Value);
				num++;
			}
		}
	}

	public static void StartClient()
	{
		ResetMappings();
		SetupBaseMapping();
	}

	public static void IdMappingsReceived(string[] _mappings)
	{
		for (int i = 0; i < _mappings.Length; i++)
		{
			if (!knownPackageTypes.TryGetValue(_mappings[i], out var value))
			{
				Log.Error("[NET] Unknown package type " + _mappings[i] + ", can not proceed connecting to server");
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameManager.Instance.ShowMessagePlayerDenied(new GameUtils.KickPlayerData(GameUtils.EKickReason.UnknownNetPackage));
				break;
			}
			if (!(value == packageIdsType))
			{
				AddPackageMapping(i, value);
			}
		}
	}

	public static NetPackage ParsePackage(PooledBinaryReader _reader, ClientInfo _sender)
	{
		NetPackage rawPackage = getPackageInfoByType(_reader.ReadUInt16()).GetRawPackage();
		rawPackage.Sender = _sender;
		rawPackage.read(_reader);
		return rawPackage;
	}

	public static void FreePackage(NetPackage _package)
	{
		getPackageInfoByType(_package.PackageId).FreePackage(_package);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IPackageInformation getPackageInfoByType(int _packageTypeId)
	{
		if (_packageTypeId >= packageIdToPackageInformation.Length || packageIdToPackageInformation[_packageTypeId] == null)
		{
			throw new UnknownNetPackageException(_packageTypeId);
		}
		return packageIdToPackageInformation[_packageTypeId];
	}

	public static TPackage GetPackage<TPackage>() where TPackage : NetPackage
	{
		return NetPackageInformation<TPackage>.Instance.GetPackage();
	}

	public static int GetPackageId(Type _type)
	{
		return packageClassToPackageId[_type];
	}

	public static string GetPackageName(int _id)
	{
		return packageIdToClass[_id].ToString();
	}

	public static void LogStats()
	{
		Log.Out("NetPackage pool stats:");
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < packageIdToPackageInformation.Length; i++)
		{
			IPackageInformation packageInformation = packageIdToPackageInformation[i];
			if (packageInformation != null)
			{
				packageInformation.GetStats(out var _packages, out var _totalSize);
				Log.Out("    {0}: {1} packages, {2} Bytes", GetPackageName(i), _packages, _totalSize);
				num += _packages;
				num2 += _totalSize;
			}
		}
		Log.Out("  Total: {0} packages, {1} Bytes", num, num2);
	}
}
