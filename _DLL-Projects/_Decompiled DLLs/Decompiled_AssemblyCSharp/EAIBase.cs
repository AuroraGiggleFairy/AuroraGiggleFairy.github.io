using System.Runtime.CompilerServices;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EAIBase
{
	public EAIManager manager;

	public EntityAlive theEntity;

	public float executeWaitTime;

	public float executeDelay;

	public int MutexBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedTypeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string shortedTypeName;

	public GameRandom Random
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return manager.random;
		}
	}

	public float RandomFloat
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return manager.random.RandomFloat;
		}
	}

	public virtual void Init(EntityAlive _theEntity)
	{
		executeDelay = 0.5f;
		manager = _theEntity.aiManager;
		theEntity = _theEntity;
	}

	public virtual void SetData(DictionarySave<string, string> data)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void GetData(DictionarySave<string, string> data, string name, ref float value)
	{
		if (data.TryGetValue(name, out var _value) && StringParsers.TryParseFloat(_value, out var _result))
		{
			value = _result;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void GetData(DictionarySave<string, string> data, string name, ref int value)
	{
		if (data.TryGetValue(name, out var _value) && StringParsers.TryParseSInt32(_value, out var _result))
		{
			value = _result;
		}
	}

	public abstract bool CanExecute();

	public virtual bool Continue()
	{
		return CanExecute();
	}

	public virtual bool IsContinuous()
	{
		return true;
	}

	public virtual void Start()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void Update()
	{
	}

	public virtual bool IsPathUsageBlocked(PathEntity _path)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Vector3 GetTargetPos(EntityAlive theEntity)
	{
		if (theEntity.GetAttackTarget() != null)
		{
			return theEntity.GetAttackTarget().position;
		}
		return theEntity.InvestigatePosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool EntityHasTarget(EntityAlive theEntity)
	{
		if (!(theEntity.GetAttackTarget() != null))
		{
			return theEntity.HasInvestigatePosition;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetRandom(int maxExclusive)
	{
		return manager.random.RandomRange(maxExclusive);
	}

	public override string ToString()
	{
		if (shortedTypeName == null)
		{
			shortedTypeName = GetTypeName().Substring(3);
		}
		return shortedTypeName;
	}

	public string GetTypeName()
	{
		if (cachedTypeName == null)
		{
			cachedTypeName = GetType().Name;
		}
		return cachedTypeName;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EAIBase()
	{
	}
}
