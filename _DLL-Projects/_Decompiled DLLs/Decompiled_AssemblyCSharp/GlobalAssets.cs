using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class GlobalAssets
{
	public const string ShaderMappingFile = "shaders.json";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> shaders;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> LoadShaderMappings()
	{
		return JsonUtility.FromJson<AssetMappings>(File.ReadAllText(Path.Combine(Addressables.RuntimePath, "shaders.json"))).ToDictionary();
	}

	public static Shader FindShader(string name)
	{
		if (shaders == null)
		{
			shaders = LoadShaderMappings();
		}
		if (shaders.TryGetValue(name, out var value))
		{
			return LoadManager.LoadAssetFromAddressables<Shader>(value, null, null, false, true).Asset;
		}
		return Shader.Find(name);
	}
}
