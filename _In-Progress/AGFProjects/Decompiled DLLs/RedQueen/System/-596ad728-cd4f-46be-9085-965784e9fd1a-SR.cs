using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.System.ValueTuple;

namespace System;

internal static class _003C596ad728_002Dcd4f_002D46be_002D9085_002D965784e9fd1a_003ESR
{
	private static ResourceManager s_resourceManager;

	private const string s_resourcesName = "FxResources.System.ValueTuple.SR";

	private static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(ResourceType));

	internal static string ArgumentException_ValueTupleIncorrectType => GetResourceString("ArgumentException_ValueTupleIncorrectType", null);

	internal static string ArgumentException_ValueTupleLastArgumentNotAValueTuple => GetResourceString("ArgumentException_ValueTupleLastArgumentNotAValueTuple", null);

	internal static Type ResourceType => typeof(SR);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string text = null;
		try
		{
			text = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		if (defaultString != null && resourceKey.Equals(text, StringComparison.Ordinal))
		{
			return defaultString;
		}
		return text;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}
}
