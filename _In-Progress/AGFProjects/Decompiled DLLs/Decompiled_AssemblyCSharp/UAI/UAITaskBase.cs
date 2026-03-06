using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAITaskBase
{
	public Dictionary<string, string> Parameters = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool parmsInitialized;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	public virtual void Init(Context _context)
	{
		_context.ActionData.Initialized = true;
		_context.ActionData.Started = false;
		_context.ActionData.Executing = false;
		if (!parmsInitialized)
		{
			initializeParameters();
			parmsInitialized = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void initializeParameters()
	{
	}

	public virtual void Start(Context _context)
	{
		_context.ActionData.Started = true;
		_context.ActionData.Executing = true;
	}

	public virtual void Update(Context _context)
	{
	}

	public virtual void Stop(Context _context)
	{
		_context.ActionData.Executing = false;
	}

	public virtual void Reset(Context _context)
	{
		_context.ActionData.ClearData();
	}
}
