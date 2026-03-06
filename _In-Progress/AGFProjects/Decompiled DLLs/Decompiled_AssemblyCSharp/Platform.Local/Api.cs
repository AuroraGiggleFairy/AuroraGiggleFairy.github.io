using System;

namespace Platform.Local;

public class Api : IPlatformApi
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EApiStatus ClientApiStatus { get; }

	public event Action ClientApiInitialized
	{
		add
		{
			lock (this)
			{
				value();
			}
		}
		remove
		{
		}
	}

	public void Init(IPlatform _owner)
	{
	}

	public bool InitClientApis()
	{
		return true;
	}

	public bool InitServerApis()
	{
		return true;
	}

	public void ServerApiLoaded()
	{
	}

	public void Update()
	{
	}

	public void Destroy()
	{
	}

	public float GetScreenBoundsValueFromSystem()
	{
		return 1f;
	}
}
