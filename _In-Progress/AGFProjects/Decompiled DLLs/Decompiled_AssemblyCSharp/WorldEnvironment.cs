using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class WorldEnvironment : MonoBehaviour
{
	public static DynamicProperties Properties;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientEquatorScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientGroundScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientSkyScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientSkyDesat;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientMoon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float dataAmbientInsideSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float dataAmbientInsideThreshold;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientInsideEquatorScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientInsideGroundScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataAmbientInsideSkyScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataFogPow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 dataFogWater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 dataFogWaterColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float dataTest;

	public const float cFogTransitionSpeed = 0.01f;

	public const float cBrightnessMin = 0.2f;

	public static float AmbientTotal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float dayTimeScalar;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float insideCurrent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color fogColorOverride;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fogDensityOverride = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float brightnessInOutDayNight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nightVisionBrightness;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnderWater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster.OnChunkVisibleDelegate chunkClusterVisibleDelegate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bTerrainActived;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkLODIndex;

	public void Init(WorldCreationData _wcd, World _world)
	{
		world = _world;
		createPrefab(_wcd);
		createTransforms(_wcd);
		createWeatherEffects(_wcd);
		createAtmosphere(_wcd);
		createDistantTerrain();
	}

	public static void OnXMLChanged()
	{
		DynamicProperties properties = Properties;
		if (properties != null)
		{
			properties.ParseVec("ambientEquatorScale", ref dataAmbientEquatorScale);
			properties.ParseVec("ambientGroundScale", ref dataAmbientGroundScale);
			properties.ParseVec("ambientSkyScale", ref dataAmbientSkyScale);
			properties.ParseVec("ambientSkyDesat", ref dataAmbientSkyDesat);
			properties.ParseVec("ambientMoon", ref dataAmbientMoon);
			properties.ParseFloat("ambientInsideSpeed", ref dataAmbientInsideSpeed);
			properties.ParseFloat("ambientInsideThreshold", ref dataAmbientInsideThreshold);
			properties.ParseVec("ambientInsideEquatorScale", ref dataAmbientInsideEquatorScale);
			properties.ParseVec("ambientInsideGroundScale", ref dataAmbientInsideGroundScale);
			properties.ParseVec("ambientInsideSkyScale", ref dataAmbientInsideSkyScale);
			properties.ParseVec("fogPower", ref dataFogPow);
			properties.ParseVec("fogWater", ref dataFogWater);
			properties.ParseVec("fogWaterColor", ref dataFogWaterColor);
			properties.ParseFloat("test", ref dataTest);
		}
	}

	public void CreateUnityTerrain()
	{
	}

	public static GameObject CreateUnityTerrainOld(string _levelName, int _sliceAtWidth, int _heightMapDataWidth, int _heightMapDataHeight, List<float[,]> _heightMapDataList, int _worldScale = 1, float _yOffsetTerrain = 1f, int _sliceAt = 2048, bool _bEditMode = true)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		MicroStopwatch microStopwatch2 = new MicroStopwatch();
		TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		Texture2D[] array = new Texture2D[12]
		{
			textureAtlasTerrain.diffuse[6],
			textureAtlasTerrain.diffuse[1],
			textureAtlasTerrain.diffuse[195],
			textureAtlasTerrain.diffuse[195],
			textureAtlasTerrain.diffuse[185],
			textureAtlasTerrain.diffuse[184],
			textureAtlasTerrain.diffuse[438],
			textureAtlasTerrain.diffuse[288],
			textureAtlasTerrain.diffuse[10],
			textureAtlasTerrain.diffuse[11],
			textureAtlasTerrain.diffuse[403],
			textureAtlasTerrain.diffuse[34]
		};
		Texture2D[] array2 = new Texture2D[12]
		{
			textureAtlasTerrain.normal[6],
			textureAtlasTerrain.normal[1],
			textureAtlasTerrain.normal[195],
			textureAtlasTerrain.normal[195],
			textureAtlasTerrain.normal[185],
			textureAtlasTerrain.normal[184],
			textureAtlasTerrain.normal[438],
			textureAtlasTerrain.normal[288],
			textureAtlasTerrain.normal[10],
			textureAtlasTerrain.normal[11],
			textureAtlasTerrain.normal[403],
			textureAtlasTerrain.normal[34]
		};
		string fullPath = PathAbstractions.WorldsSearchPaths.GetLocation(_levelName).FullPath;
		int num = 4;
		int _width;
		int _height;
		Color32[] array3 = TextureUtils.LoadTexturePixels(fullPath + "/splat1", out _width, out _height);
		float num2 = (float)_width / (float)_heightMapDataWidth;
		string pathNoExtension = fullPath + "/splat2";
		float num3 = 0f;
		int _width2;
		int _height2;
		Color32[] array4 = TextureUtils.LoadTexturePixels(pathNoExtension, out _width2, out _height2);
		if (array4 != null)
		{
			num3 = (float)_width2 / (float)_heightMapDataWidth;
			num += 4;
		}
		string pathNoExtension2 = fullPath + "/splat3";
		float num4 = 0f;
		int _width3;
		int _height3;
		Color32[] array5 = TextureUtils.LoadTexturePixels(pathNoExtension2, out _width3, out _height3);
		if (array5 != null)
		{
			num4 = (float)_width3 / (float)_heightMapDataWidth;
			num += 4;
		}
		microStopwatch2.ResetAndRestart();
		for (int i = 0; i < array3.Length; i++)
		{
			Color32 color = array3[i];
			int num5 = color.r;
			int num6 = color.g;
			int num7 = color.b;
			int num8 = color.a;
			if (num > 4)
			{
				color = array4[i];
				num5 += color.r;
				num6 += color.g;
				num7 += color.b;
				num8 += color.a;
			}
			if (num > 8)
			{
				color = array5[i];
				num5 += color.r;
				num6 += color.g;
				num7 += color.b;
				num8 += color.a;
			}
			if (num5 + num6 + num7 + num8 < 255)
			{
				array3[i] = new Color32((byte)(array3[i].r + (255 - (num5 + num6 + num7 + num8))), array3[i].g, array3[i].b, array3[i].a);
			}
		}
		Log.Out("Splat1 color fix {0}ms", microStopwatch2.ElapsedMilliseconds);
		microStopwatch2.ResetAndRestart();
		GameObject gameObject = new GameObject("Terrain");
		float num9 = (float)(_heightMapDataWidth / 2) + 0.5f;
		gameObject.transform.position = new Vector3(-1f * num9 * (float)_worldScale, 1f, -1f * num9 * (float)_worldScale);
		Origin.Add(gameObject.transform, 0);
		int num10 = 0;
		int num11 = _heightMapDataWidth / _sliceAtWidth;
		int num12 = _heightMapDataHeight / _sliceAtWidth;
		Terrain[,] array6 = new Terrain[num11, num12];
		float[,,] array7 = new float[_sliceAtWidth, _sliceAtWidth, num];
		for (int j = 0; j < num12; j++)
		{
			for (int k = 0; k < num11; k++)
			{
				microStopwatch2.ResetAndRestart();
				TerrainData terrainData = new TerrainData();
				terrainData.heightmapResolution = _sliceAtWidth;
				terrainData.size = new Vector3(_sliceAtWidth * _worldScale, 256f, _sliceAtWidth * _worldScale);
				terrainData.SetHeights(0, 0, _heightMapDataList[num10++]);
				Log.Out("Setting heights to unity terrain took " + microStopwatch2.ElapsedMilliseconds + "ms");
				microStopwatch2.ResetAndRestart();
				terrainData.alphamapResolution = _sliceAtWidth;
				TerrainLayer[] array8 = new TerrainLayer[array.Length];
				for (int l = 0; l < array.Length; l++)
				{
					TerrainLayer terrainLayer = new TerrainLayer();
					terrainLayer.diffuseTexture = array[l];
					terrainLayer.normalMapTexture = array2[l];
					terrainLayer.tileSize = new Vector2(10f, 10f);
					array8[l] = terrainLayer;
				}
				terrainData.terrainLayers = array8;
				for (int m = 0; m < _sliceAtWidth; m++)
				{
					int num13 = j * _sliceAtWidth + m;
					int num14 = (int)((float)num13 * num2);
					for (int n = 0; n < _sliceAtWidth; n++)
					{
						int num15 = k * _sliceAtWidth + n;
						int num16 = (int)((float)num15 * num2 + (float)(num14 * _width)) % array3.Length;
						Color32 color2 = array3[num16];
						array7[m, n, 0] = (float)(int)color2.r * 0.003921569f;
						array7[m, n, 1] = (float)(int)color2.g * 0.003921569f;
						array7[m, n, 2] = (float)(int)color2.b * 0.003921569f;
						array7[m, n, 3] = (float)(int)color2.a * 0.003921569f;
						if (num > 4)
						{
							num16 = (int)((float)num15 * num3 + (float)((int)((float)num13 * num3) * _width2)) % array4.Length;
							color2 = array4[num16];
							array7[m, n, 4] = (float)(int)color2.r * 0.003921569f;
							array7[m, n, 5] = (float)(int)color2.g * 0.003921569f;
							array7[m, n, 6] = (float)(int)color2.b * 0.003921569f;
							array7[m, n, 7] = (float)(int)color2.a * 0.003921569f;
						}
						if (num > 8)
						{
							num16 = (int)((float)num15 * num4 + (float)((int)((float)num13 * num4) * _width3)) % array5.Length;
							color2 = array5[num16];
							array7[m, n, 8] = (float)(int)color2.r * 0.003921569f;
							array7[m, n, 9] = (float)(int)color2.g * 0.003921569f;
							array7[m, n, 10] = (float)(int)color2.b * 0.003921569f;
							array7[m, n, 11] = (float)(int)color2.a * 0.003921569f;
						}
					}
				}
				terrainData.SetAlphamaps(0, 0, array7);
				Log.Out("Splats took " + microStopwatch2.ElapsedMilliseconds + "ms");
				microStopwatch2.ResetAndRestart();
				GameObject gameObject2 = Terrain.CreateTerrainGameObject(terrainData);
				Terrain component = gameObject2.GetComponent<Terrain>();
				if (_bEditMode)
				{
					component.heightmapPixelError = 5f;
					component.basemapDistance = 2000f;
					gameObject2.AddComponent<TerrainDetectChanges>();
				}
				else
				{
					component.heightmapPixelError = 20f;
				}
				array6[k, j] = component;
				gameObject2.layer = 16;
				gameObject2.tag = "T_Mesh";
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = new Vector3(k * _sliceAtWidth * _worldScale, _yOffsetTerrain, j * _sliceAtWidth * _worldScale);
			}
		}
		for (int num17 = 0; num17 < num12; num17++)
		{
			for (int num18 = 0; num18 < num11; num18++)
			{
				array6[num18, num17].SetNeighbors((num18 > 0) ? array6[num18 - 1, num17] : null, (num17 > 0) ? array6[num18, num17 - 1] : null, (num18 < num11 - 1) ? array6[num18 + 1, num17] : null, (num17 < num12 - 1) ? array6[num18, num17 + 1] : null);
			}
		}
		Log.Out("Creating unity terrain took " + microStopwatch.ElapsedMilliseconds + "ms");
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createDistantTerrain()
	{
		if (!GameManager.IsDedicatedServer && !world.ChunkClusters[0].IsFixedSize)
		{
			chunkClusterVisibleDelegate = OnChunkDisplayed;
			world.ChunkClusters[0].OnChunkVisibleDelegates += chunkClusterVisibleDelegate;
		}
	}

	public void OnChunkDisplayed(long _key, bool _bDisplayed)
	{
		if (UnityDistantTerrainTest.Instance != null)
		{
			UnityDistantTerrainTest.Instance.OnChunkVisible(WorldChunkCache.extractX(_key), WorldChunkCache.extractZ(_key), _bDisplayed);
		}
	}

	public void Cleanup()
	{
		localPlayer = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createPrefab(WorldCreationData _wcd)
	{
		if (_wcd.Properties.Values.ContainsKey("WorldEnvironment.Prefab"))
		{
			UnityEngine.Object.Instantiate(Resources.Load<GameObject>(_wcd.Properties.Values["WorldEnvironment.Prefab"])).transform.parent = base.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createWeatherEffects(WorldCreationData _wcd)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createTransforms(WorldCreationData _wcd)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createAtmosphere(WorldCreationData _wcd)
	{
	}

	public Color GetAmbientColor()
	{
		if (world == null || world.BiomeAtmosphereEffects == null)
		{
			return Color.black;
		}
		return world.BiomeAtmosphereEffects.GetSkyColorSpectrum(dayTimeScalar);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AmbientSpectrumFrameUpdate()
	{
		if (world != null && world.BiomeAtmosphereEffects != null)
		{
			float target = 0f;
			if ((bool)localPlayer && localPlayer.PlayerStats.LightInsidePer >= dataAmbientInsideThreshold)
			{
				target = 1f;
			}
			insideCurrent = Mathf.MoveTowards(insideCurrent, target, dataAmbientInsideSpeed * Time.deltaTime);
			float dayPercent = SkyManager.dayPercent;
			float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
			float num2 = ((num < 0.5f) ? Utils.FastLerpUnclamped(0.2f, 1f, num * 2f) : (num + 0.5f));
			float b = Mathf.LerpUnclamped(Utils.FastMax(1f, num2), num2, insideCurrent);
			num2 = (brightnessInOutDayNight = Utils.FastLerp(num2, b, dayPercent * 2f));
			float moonAmbientScale = SkyManager.GetMoonAmbientScale(dataAmbientMoon.x, dataAmbientMoon.y);
			moonAmbientScale = Mathf.LerpUnclamped(moonAmbientScale, 1f, insideCurrent);
			num2 *= moonAmbientScale;
			num2 += nightVisionBrightness;
			Color skyColor = SkyManager.GetSkyColor();
			Color b2 = new Color(0.7f, 0.7f, 0.7f, 1f);
			float t = Mathf.LerpUnclamped(dataAmbientSkyDesat.y, dataAmbientSkyDesat.x, dayPercent);
			Color color = Color.LerpUnclamped(skyColor, b2, t);
			float a = Mathf.LerpUnclamped(dataAmbientSkyScale.y, dataAmbientSkyScale.x, dayPercent);
			float b3 = Mathf.LerpUnclamped(dataAmbientInsideSkyScale.y, dataAmbientInsideSkyScale.x, dayPercent);
			float num3 = Mathf.LerpUnclamped(a, b3, insideCurrent);
			float num4 = SkyManager.GetLuma(color) * num3;
			num3 *= num2;
			RenderSettings.ambientSkyColor = color * num3;
			float a2 = Mathf.LerpUnclamped(dataAmbientEquatorScale.y, dataAmbientEquatorScale.x, dayPercent);
			float b4 = Mathf.LerpUnclamped(dataAmbientInsideEquatorScale.y, dataAmbientInsideEquatorScale.x, dayPercent);
			a2 = Mathf.LerpUnclamped(a2, b4, insideCurrent);
			a2 *= num2;
			RenderSettings.ambientEquatorColor = SkyManager.GetFogColor() * a2;
			Color sunLightColor = SkyManager.GetSunLightColor();
			float a3 = Mathf.LerpUnclamped(dataAmbientGroundScale.y, dataAmbientGroundScale.x, dayPercent);
			float b5 = Mathf.LerpUnclamped(dataAmbientInsideGroundScale.y, dataAmbientInsideGroundScale.x, dayPercent);
			float num5 = Mathf.LerpUnclamped(a3, b5, insideCurrent);
			num4 += SkyManager.GetLuma(sunLightColor) * num5;
			num5 *= num2;
			RenderSettings.ambientGroundColor = sunLightColor * num5;
			AmbientTotal = num4 * moonAmbientScale;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpectrumsFrameUpdate()
	{
		if (world == null || !localPlayer)
		{
			return;
		}
		BiomeAtmosphereEffects biomeAtmosphereEffects = world.BiomeAtmosphereEffects;
		if (biomeAtmosphereEffects != null)
		{
			Color skyColorSpectrum = biomeAtmosphereEffects.GetSkyColorSpectrum(dayTimeScalar);
			skyColorSpectrum = WeatherManager.Instance.GetWeatherSpectrum(skyColorSpectrum, AtmosphereEffect.ESpecIdx.Sky, dayTimeScalar);
			SkyManager.SetSkyColor(skyColorSpectrum);
			skyColorSpectrum = biomeAtmosphereEffects.GetSunColorSpectrum(dayTimeScalar);
			skyColorSpectrum = WeatherManager.Instance.GetWeatherSpectrum(skyColorSpectrum, AtmosphereEffect.ESpecIdx.Sun, dayTimeScalar);
			SkyManager.SetSunColor(skyColorSpectrum);
			SkyManager.SetSunIntensity(skyColorSpectrum.a * 2f);
			skyColorSpectrum = biomeAtmosphereEffects.GetMoonColorSpectrum(dayTimeScalar);
			skyColorSpectrum = WeatherManager.Instance.GetWeatherSpectrum(skyColorSpectrum, AtmosphereEffect.ESpecIdx.Moon, dayTimeScalar);
			SkyManager.SetMoonLightColor(skyColorSpectrum);
			skyColorSpectrum = biomeAtmosphereEffects.GetFogColorSpectrum(dayTimeScalar);
			skyColorSpectrum = WeatherManager.Instance.GetWeatherSpectrum(skyColorSpectrum, AtmosphereEffect.ESpecIdx.Fog, dayTimeScalar);
			Color fogFadeColorSpectrum = biomeAtmosphereEffects.GetFogFadeColorSpectrum(dayTimeScalar);
			fogFadeColorSpectrum = WeatherManager.Instance.GetWeatherSpectrum(fogFadeColorSpectrum, AtmosphereEffect.ESpecIdx.FogFade, dayTimeScalar);
			SkyManager.SetFogFade(1f - fogFadeColorSpectrum.r - 0.5f, 1f - fogFadeColorSpectrum.a);
			Color b = new Color(skyColorSpectrum.r, skyColorSpectrum.g, skyColorSpectrum.b, 1f);
			b *= Utils.FastMin(brightnessInOutDayNight, 1f);
			float dayPercent = SkyManager.dayPercent;
			float num = Mathf.Pow(skyColorSpectrum.a, Utils.FastLerpUnclamped(dataFogPow.y, dataFogPow.x, dayPercent));
			num += WeatherManager.currentWeather.FogPercent();
			if (num > 1f)
			{
				num = 1f;
			}
			if (fogDensityOverride >= 0f)
			{
				num = fogDensityOverride;
				b = fogColorOverride;
			}
			float t = 0.01f;
			if (localPlayer.IsUnderwaterCamera)
			{
				isUnderWater = true;
				num = dataFogWater.x;
				t = dataFogWater.y;
				float num2 = 0.35f + SkyManager.dayPercent * 0.65f;
				b = new Color(dataFogWaterColor.x * num2, dataFogWaterColor.y * num2, dataFogWaterColor.z * num2, 1f);
			}
			else if (isUnderWater)
			{
				isUnderWater = false;
				t = 1f;
			}
			if (localPlayer.bPlayingSpawnIn)
			{
				t = 1f;
			}
			if (localPlayer.InWorldLookPercent < 1f)
			{
				b = Color.LerpUnclamped(new Color(0.5f, 0.5f, 0.2f), b, localPlayer.InWorldLookPercent);
				num = Utils.FastLerpUnclamped(0.6f, num, localPlayer.InWorldLookPercent);
			}
			SkyManager.SetFogColor(Color.Lerp(SkyManager.GetFogColor(), b, t));
			SkyManager.SetFogDensity(Mathf.Lerp(SkyManager.GetFogDensity(), num, t));
		}
	}

	public void WorldTimeChanged()
	{
		WeatherManager.SetWorldTime(world.worldTime);
		SkyManager.SetGameTime(world.worldTime);
		if (world.BiomeAtmosphereEffects != null)
		{
			world.BiomeAtmosphereEffects.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void enableAllDisplayedDistantChunks(bool _bEnable)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		dayTimeScalar = SkyManager.GetWorldRotation();
		if (!WeatherManager.Instance)
		{
			return;
		}
		WeatherManager.Instance.FrameUpdate();
		AmbientSpectrumFrameUpdate();
		if (localPlayer == null)
		{
			localPlayer = world.GetPrimaryPlayer();
			if (localPlayer == null)
			{
				return;
			}
		}
		if (!GameManager.IsDedicatedServer && UnityDistantTerrainTest.Instance != null)
		{
			UnityDistantTerrainTest.Instance.FrameUpdate(localPlayer);
		}
		SpectrumsFrameUpdate();
		IList<ChunkGameObject> displayedChunkGameObjects = world.m_ChunkManager.GetDisplayedChunkGameObjects();
		int count = displayedChunkGameObjects.Count;
		int num = count;
		if (num > 8)
		{
			num = 8;
		}
		for (int i = 0; i < num; i++)
		{
			if (++chunkLODIndex >= count)
			{
				chunkLODIndex = 0;
			}
			displayedChunkGameObjects[chunkLODIndex].CheckLODs();
		}
	}

	public void CreateLevelBorderBox(World _world)
	{
	}

	public void SetColliders(float _worldX, float _worldZ, float _worldXDim, float _worldZDim, float _waterSize, float _bDistance)
	{
	}

	public Color GetSunLightColor()
	{
		return world.BiomeAtmosphereEffects.GetSunColorSpectrum(dayTimeScalar);
	}

	public Color GetMoonLightColor()
	{
		return world.BiomeAtmosphereEffects.GetMoonColorSpectrum(dayTimeScalar);
	}

	public static float CalculateCelestialAngle(ulong _worldTime, float _t)
	{
		float num = ((float)(int)(_worldTime % 24000) + _t) / 24000f - 0.25f;
		if (num < 0f)
		{
			num += 1f;
		}
		if (num > 1f)
		{
			num -= 1f;
		}
		float num2 = num;
		num = 1f - (float)((Math.Cos((double)num * Math.PI) + 1.0) / 2.0);
		return num2 + (num - num2) / 3f;
	}

	public BiomeAtmosphereEffects GetBiomeAtmosphereEffects()
	{
		return world.BiomeAtmosphereEffects;
	}

	public int DistantTerrain_GetBlockIdAt(int x, int y, int z)
	{
		int pOIBlockIdOverride = world.ChunkCache.ChunkProvider.GetPOIBlockIdOverride(x, z);
		if (pOIBlockIdOverride != 0)
		{
			return pOIBlockIdOverride;
		}
		BlockValue topmostBlockValue = world.ChunkCache.ChunkProvider.GetBiomeProvider().GetTopmostBlockValue(x, z);
		if (!topmostBlockValue.isair)
		{
			return topmostBlockValue.type;
		}
		float _intensity;
		BiomeDefinition biomeAt = world.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt(x, z, out _intensity);
		if (biomeAt == null)
		{
			return 1;
		}
		topmostBlockValue = biomeAt.m_Layers[0].m_Block.blockValues[0];
		return topmostBlockValue.type;
	}

	public void SetFogOverride(Color _color = default(Color), float _density = -1f)
	{
		fogColorOverride = _color;
		fogDensityOverride = _density;
	}

	public void SetNightVision(float _brightness)
	{
		nightVisionBrightness = _brightness * 0.4f;
	}
}
