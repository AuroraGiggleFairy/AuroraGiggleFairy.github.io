using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGfx : ConsoleCmdAbstract
{
	public static float debugFloat;

	public static float debugFloat2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject distantTerrainObj;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "gfx" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Graphics commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Graphics commands:\naf <value> - anisotropic filtering off, on or force on (0, 1, 2)\ndr <scale> <min> <max> - set dynamic res scale (0 auto, .1 to 1 force, -1 for off) and min/max FPS\ndt <value> - toggle distant terrain or set value (0 or 1)\ndti <value> - set distant terrain instancing (0 or 1)\ndtmaxlod <value> - set distant terrain max LOD (0 to 5)\ndtpix <value> - set distant terrain pixel error (1 to 200)\nkey name <value> - set shader keyword (0 or 1)\npp name <value> - set postprocessing name (enable, ambientOcclusion (ao), auto exposure (ae), bloom, colorGrading (cg), etc.) to value (0 or 1)\nres <width> <height> - set screen resolution\nresetrev - clear graphics preset revision\nskin <value> - set skin bone count (1, 2, 4, 5+ (all))\nst name <value> - set streaming name (budget (0 disables), discard, forceload, reduction) to value\ntex - show texture info\ntexbias <value> - set bias on all textures\ntexlimit <value> - set limit (0-x)\nviewdist <value> - Set view distance in chunks";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		switch (text)
		{
		case "af":
		{
			int result10 = 0;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out result10);
			}
			QualitySettings.anisotropicFiltering = result10 switch
			{
				1 => AnisotropicFiltering.Enable, 
				0 => AnisotropicFiltering.Disable, 
				_ => AnisotropicFiltering.ForceEnable, 
			};
			break;
		}
		case "be":
		{
			int result15 = 0;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out result15);
			}
			string name2 = "";
			if (_params.Count >= 3)
			{
				name2 = _params[2];
			}
			int num3 = GameManager.Instance.World.m_ChunkManager.SetBlockEntitiesVisible(result15 != 0, name2);
			Log.Out("Set {0}", num3);
			break;
		}
		case "d":
			debugFloat = 0f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out debugFloat);
			}
			break;
		case "d2":
			debugFloat2 = 0f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out debugFloat2);
			}
			break;
		case "dr":
			if (GameManager.Instance.World != null)
			{
				float result2 = -1f;
				if (_params.Count >= 2)
				{
					float.TryParse(_params[1], out result2);
				}
				float result3 = -1f;
				if (_params.Count >= 3)
				{
					float.TryParse(_params[2], out result3);
				}
				float result4 = -1f;
				if (_params.Count >= 4)
				{
					float.TryParse(_params[3], out result4);
				}
				GameManager.Instance.World.GetPrimaryPlayer().renderManager.SetDynamicResolution(result2, result3, result4);
			}
			break;
		case "dt":
			if (FindDistantTerrain())
			{
				if (_params.Count < 2)
				{
					distantTerrainObj.SetActive(!distantTerrainObj.activeSelf);
				}
				else
				{
					distantTerrainObj.SetActive(_params[1] != "0");
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain " + (distantTerrainObj.activeSelf ? "active" : "not active"));
			}
			break;
		case "dti":
			if (FindDistantTerrain())
			{
				int result14 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out result14);
				}
				Terrain[] componentsInChildren = distantTerrainObj.GetComponentsInChildren<Terrain>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].drawInstanced = result14 != 0;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain instanced {0}", result14 != 0);
			}
			break;
		case "dtmaxlod":
			if (FindDistantTerrain())
			{
				int result5 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out result5);
				}
				Terrain[] componentsInChildren = distantTerrainObj.GetComponentsInChildren<Terrain>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].heightmapMaximumLOD = result5;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain maxlod {0}", result5);
			}
			break;
		case "dtpix":
			if (FindDistantTerrain())
			{
				float result = 5f;
				if (_params.Count >= 2)
				{
					float.TryParse(_params[1], out result);
				}
				Terrain[] componentsInChildren = distantTerrainObj.GetComponentsInChildren<Terrain>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].heightmapPixelError = result;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain pixerr {0}", result);
			}
			break;
		case "key":
		{
			if (_params.Count < 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No key name");
				break;
			}
			int result11 = 0;
			if (_params.Count >= 3)
			{
				int.TryParse(_params[2], out result11);
			}
			string keyword = _params[1].ToUpper();
			Shader.DisableKeyword(keyword);
			if (result11 != 0)
			{
				Shader.EnableKeyword(keyword);
			}
			break;
		}
		case "lodbias":
		{
			float lodBias = GameOptionsManager.GetLODBias();
			if (_params.Count >= 2)
			{
				lodBias = float.Parse(_params[1]);
			}
			QualitySettings.lodBias = lodBias;
			break;
		}
		case "mesh":
		{
			if (_params.Count < 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Missing params");
				break;
			}
			int num2 = int.Parse(_params[1]);
			string name = _params[2];
			float value = float.Parse(_params[3]);
			MeshDescription.meshes[num2].material.SetFloat(name, value);
			break;
		}
		case "pp":
		{
			if (_params.Count < 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No effect name");
				break;
			}
			float result8 = 0f;
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out result8);
			}
			float result9 = 0f;
			if (_params.Count >= 4)
			{
				float.TryParse(_params[3], out result9);
			}
			string command = _params[1].ToLower();
			SetPostProcessing(command, result8, result9);
			break;
		}
		case "res":
		{
			int result6 = 1920;
			int result7 = 1080;
			if (_params.Count >= 3)
			{
				int.TryParse(_params[1], out result6);
				result6 = Utils.FastClamp(result6, 640, 8192);
				int.TryParse(_params[2], out result7);
				result7 = Utils.FastClamp(result7, 480, 8192);
			}
			GameOptionsManager.SetResolution(result6, result7);
			break;
		}
		case "resetrev":
			GamePrefs.Set(EnumGamePrefs.OptionsGfxResetRevision, 0);
			GamePrefs.Instance.Save();
			break;
		case "skin":
		{
			int result16 = 2;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out result16);
				if (result16 < 1)
				{
					result16 = 1;
				}
				if (result16 > 4)
				{
					result16 = 255;
				}
			}
			QualitySettings.skinWeights = (SkinWeights)result16;
			break;
		}
		case "st":
		{
			if (_params.Count < 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No cmd name");
				break;
			}
			float result12 = 0f;
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out result12);
			}
			switch (_params[1].ToLower())
			{
			case "a":
			case "active":
			{
				if (_params.Count >= 3 && bool.TryParse(_params[2], out var result13))
				{
					QualitySettings.streamingMipmapsActive = result13;
				}
				else
				{
					QualitySettings.streamingMipmapsActive = !QualitySettings.streamingMipmapsActive;
				}
				break;
			}
			case "b":
			case "budget":
				QualitySettings.streamingMipmapsMemoryBudget = result12;
				break;
			case "d":
			case "discard":
				Texture.streamingTextureDiscardUnusedMips = result12 != 0f;
				break;
			case "f":
			case "forceload":
				Texture.streamingTextureForceLoadAll = result12 != 0f;
				break;
			case "r":
			case "reduction":
				QualitySettings.streamingMipmapsMaxLevelReduction = (int)result12;
				break;
			default:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown st " + _params[1]);
				break;
			}
			break;
		}
		case "tex":
			Resources.UnloadUnusedAssets();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Textures:");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" limit {0}, stream {1} (reduction {2})", GameRenderManager.TextureMipmapLimit, QualitySettings.streamingMipmapsActive, QualitySettings.streamingMipmapsMaxLevelReduction);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" mem - current {0} MB, target {1} MB, desired {2} MB, total {3} MB", Texture.currentTextureMemory / 1048576, Texture.targetTextureMemory / 1048576, Texture.desiredTextureMemory / 1048576, Texture.totalTextureMemory / 1048576);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" normal - count {0}, mem {1} MB", Texture.nonStreamingTextureCount, Texture.nonStreamingTextureMemory / 1048576);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" streaming - count {0}, pending {1}, loading {2}", Texture.streamingTextureCount, Texture.streamingTexturePendingLoadCount, Texture.streamingTextureLoadingCount);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" streaming - budget {0} MB, renderer count {1}, mip uploads {2}", QualitySettings.streamingMipmapsMemoryBudget, Texture.streamingRendererCount, Texture.streamingMipmapUploadCount);
			break;
		case "texbias":
		{
			float mipMapBias = 0f;
			if (_params.Count >= 2)
			{
				mipMapBias = float.Parse(_params[1]);
			}
			Texture[] array = Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[];
			for (int j = 0; j < array.Length; j++)
			{
				array[j].mipMapBias = mipMapBias;
			}
			break;
		}
		case "texlimit":
		{
			int textureMipmapLimit = 0;
			if (_params.Count >= 2)
			{
				textureMipmapLimit = int.Parse(_params[1]);
			}
			GameRenderManager.TextureMipmapLimit = textureMipmapLimit;
			break;
		}
		case "texreport":
		{
			string filename = string.Empty;
			if (_params.Count >= 2)
			{
				filename = _params[1];
			}
			LogTextureReport(filename);
			break;
		}
		case "viewdist":
			if (GameManager.Instance.World == null)
			{
				int num = 6;
				if (_params.Count >= 2)
				{
					num = int.Parse(_params[1]);
					num = Utils.FastClamp(num, 1, 22);
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("viewdist {0}", num);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, num);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Requires main menu");
			}
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindDistantTerrain()
	{
		if (!distantTerrainObj)
		{
			distantTerrainObj = GameObject.Find("/DistantUnityTerrain");
			if (!distantTerrainObj)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant terrain gameobject not found");
				return false;
			}
		}
		return true;
	}

	public void SetPostProcessing(string command, float value, float value2)
	{
		Camera main = Camera.main;
		if (!main)
		{
			return;
		}
		PostProcessLayer component = main.GetComponent<PostProcessLayer>();
		if (!component)
		{
			return;
		}
		PostProcessVolume component2 = main.GetComponent<PostProcessVolume>();
		if (!component2)
		{
			return;
		}
		HBAO component3 = main.GetComponent<HBAO>();
		NGSS_FrustumShadows_7DTD component4 = main.GetComponent<NGSS_FrustumShadows_7DTD>();
		PostProcessProfile profile = component2.profile;
		if (!profile)
		{
			return;
		}
		PostProcessLayer postProcessLayer = null;
		PostProcessEffectSettings postProcessEffectSettings = null;
		switch (command)
		{
		case "enable":
			component.enabled = value != 0f;
			break;
		case "autoexposure":
		case "ae":
			postProcessEffectSettings = profile.GetSetting<AutoExposure>();
			break;
		case "ambientocclusion":
		case "ao":
			component3.enabled = value != 0f;
			break;
		case "contactshadows":
			component4.enabled = value != 0f;
			break;
		case "antialiasing":
		case "aa":
			if ((bool)postProcessLayer)
			{
				GameManager.Instance.World?.GetPrimaryPlayer().renderManager.SetAntialiasing((int)value, value2, component);
			}
			break;
		case "bloom":
			postProcessEffectSettings = profile.GetSetting<Bloom>();
			break;
		case "chromaticaberration":
		case "ca":
			postProcessEffectSettings = profile.GetSetting<ChromaticAberration>();
			break;
		case "colorgrading":
		case "cg":
			postProcessEffectSettings = profile.GetSetting<ColorGrading>();
			break;
		case "depthoffield":
		case "dof":
			postProcessEffectSettings = profile.GetSetting<DepthOfField>();
			break;
		case "fog":
			component.fog.enabled = value != 0f;
			break;
		case "grain":
			postProcessEffectSettings = profile.GetSetting<Grain>();
			break;
		case "motionblur":
		case "mb":
			postProcessEffectSettings = profile.GetSetting<MotionBlur>();
			break;
		case "screenspacereflection":
		case "ssr":
			postProcessEffectSettings = profile.GetSetting<ScreenSpaceReflections>();
			break;
		case "ssrdist":
			profile.GetSetting<ScreenSpaceReflections>().maximumMarchDistance.Override(value);
			value = 1f;
			break;
		case "ssrit":
			profile.GetSetting<ScreenSpaceReflections>().maximumIterationCount.Override((int)value);
			value = 1f;
			break;
		case "ssrq":
			profile.GetSetting<ScreenSpaceReflections>().resolution.Override((value != 0f) ? ScreenSpaceReflectionResolution.FullSize : ScreenSpaceReflectionResolution.Downsampled);
			value = 1f;
			break;
		case "vignette":
			postProcessEffectSettings = profile.GetSetting<Vignette>();
			break;
		}
		if (postProcessEffectSettings != null)
		{
			component.enabled = false;
			postProcessEffectSettings.enabled.Override(value != 0f);
			component.enabled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogTextureReport(string filename)
	{
		bool streamingMipmapsActive = QualitySettings.streamingMipmapsActive;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Streaming Enabled, Streaming Texture Count,Non Streaming Texture Count,Total Texture Memory (No Budget),Memory Budget,Desired Streaming Memory Budget,Current Target Memory Budget,Non Streaming Memory");
		stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}\n", streamingMipmapsActive, Texture.streamingTextureCount, Texture.nonStreamingTextureCount, (double)Texture.totalTextureMemory * 9.5367431640625E-07, QualitySettings.streamingMipmapsMemoryBudget, (double)Texture.desiredTextureMemory * 9.5367431640625E-07, (double)Texture.targetTextureMemory * 9.5367431640625E-07, (double)Texture.nonStreamingTextureMemory * 9.5367431640625E-07);
		stringBuilder.AppendLine("Texture Name,Type,Is Readable,Streaming Enabled,Minimum Mip,Calculated Mip,Requested Mip,Desired Mip,Priority,Loaded Mip,Format,Width,Height,Mip Count,Texture Size (All Mips),Readable Size,Desired Size,Loaded Size, Is Streamed");
		Texture2D[] array = Resources.FindObjectsOfTypeAll<Texture2D>();
		foreach (Texture2D texture2D in array)
		{
			int num = ProfilerUtils.CalculateTextureSizeBytes(texture2D);
			int loadedMipmapLevel = texture2D.loadedMipmapLevel;
			int num2 = ProfilerUtils.CalculateTextureSizeBytes(texture2D, loadedMipmapLevel);
			int num3 = (texture2D.isReadable ? num : 0);
			int num4 = ProfilerUtils.CalculateTextureSizeBytes(texture2D, texture2D.desiredMipmapLevel);
			stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}\n", texture2D.name, "Texture2D", texture2D.isReadable, texture2D.streamingMipmaps, texture2D.minimumMipmapLevel, texture2D.calculatedMipmapLevel, texture2D.requestedMipmapLevel, texture2D.desiredMipmapLevel, texture2D.streamingMipmapsPriority, loadedMipmapLevel, texture2D.format, texture2D.width, texture2D.height, texture2D.mipmapCount, num, num3, num4, num2, texture2D.AreMipMapsStreamed());
		}
		Cubemap[] array2 = Resources.FindObjectsOfTypeAll<Cubemap>();
		foreach (Cubemap cubemap in array2)
		{
			int num5 = ProfilerUtils.CalculateTextureSizeBytes(cubemap);
			int loadedMipmapLevel2 = cubemap.loadedMipmapLevel;
			int num6 = ProfilerUtils.CalculateTextureSizeBytes(cubemap, loadedMipmapLevel2);
			int num7 = (cubemap.isReadable ? num5 : 0);
			int num8 = ProfilerUtils.CalculateTextureSizeBytes(cubemap, cubemap.desiredMipmapLevel);
			stringBuilder.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}\n", cubemap.name, "Cubemap", cubemap.isReadable, cubemap.streamingMipmaps, -1, -1, cubemap.requestedMipmapLevel, cubemap.desiredMipmapLevel, cubemap.streamingMipmapsPriority, loadedMipmapLevel2, cubemap.format, cubemap.width, cubemap.height, cubemap.mipmapCount, num5, num7, num8, num6, cubemap.AreMipMapsStreamed());
		}
		if (string.IsNullOrEmpty(filename))
		{
			Log.Out(stringBuilder.ToString());
			return;
		}
		string tempFileName = PlatformManager.NativePlatform.Utils.GetTempFileName(filename, ".csv");
		try
		{
			File.WriteAllText(tempFileName, stringBuilder.ToString());
			Log.Out("Wrote texreport to " + tempFileName);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}
}
