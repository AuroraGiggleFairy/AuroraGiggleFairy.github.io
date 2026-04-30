using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	public static T Instance;

	public bool IsPersistant;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (IsPersistant)
		{
			if (!Instance)
			{
				Instance = (T)this;
				Instance.singletonCreated();
				Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			Instance = (T)this;
			Instance.singletonCreated();
		}
		Instance.singletonAwake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDestroy()
	{
		if (this == Instance)
		{
			singletonDestroy();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void singletonAwake()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void singletonCreated()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void singletonDestroy()
	{
	}
}
