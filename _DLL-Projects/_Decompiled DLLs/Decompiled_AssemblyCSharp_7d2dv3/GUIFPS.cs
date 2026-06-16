using System;
using UnityEngine;

public class GUIFPS : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public FPS fps = new FPS(0.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string format;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int BaseTextSize = 13;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowGraph;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D texture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int curGraphXPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UITexture guiFpsGraphTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public long lastTotalMemory;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int gcSpikeCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBarHeight = 2500f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastResolution;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle boxStyle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int boxAreaHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int boxAreaWidth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowManager windowManager;

	public bool Enabled
	{
		get
		{
			return bEnabled;
		}
		set
		{
			if (bEnabled != value)
			{
				bEnabled = value;
				if (!value && guiFpsGraphTexture != null && guiFpsGraphTexture.enabled)
				{
					guiFpsGraphTexture.enabled = false;
				}
			}
		}
	}

	public bool ShowGraph
	{
		get
		{
			return bShowGraph;
		}
		set
		{
			if (bShowGraph != value)
			{
				bShowGraph = value;
				if (value && guiFpsGraphTexture == null)
				{
					initFpsGraph();
				}
				if (guiFpsGraphTexture.enabled != bShowGraph)
				{
					guiFpsGraphTexture.enabled = bShowGraph;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		windowManager = GetComponentInParent<GUIWindowManager>();
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _obj)
	{
		if (_obj == EnumGamePrefs.OptionsUiFpsScaling)
		{
			lastResolution = Vector2i.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (fps.Update())
		{
			format = $"{fps.Counter:F1} FPS";
		}
		if (bEnabled && bShowGraph)
		{
			updateFPSGraph();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (Enabled && windowManager.IsHUDEnabled())
		{
			Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
			if (lastResolution != vector2i)
			{
				float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsUiFpsScaling) * 13f;
				lastResolution = vector2i;
				boxStyle = new GUIStyle(GUI.skin.box);
				int num2 = ((vector2i.y <= 1200) ? Mathf.RoundToInt(num) : Mathf.RoundToInt((float)vector2i.y / (1200f / num)));
				boxStyle.fontSize = num2;
				boxAreaHeight = num2 + 10;
				boxAreaWidth = num2 * 7;
			}
			if (fps.Counter < 30f)
			{
				GUI.color = Color.yellow;
			}
			else if (fps.Counter < 10f)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.green;
			}
			GUI.Box(new Rect(14f, Screen.height / 2 + 40, boxAreaWidth, boxAreaHeight), format, boxStyle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationQuit()
	{
		if (texture != null)
		{
			UnityEngine.Object.Destroy(texture);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initFpsGraph()
	{
		texture = createGUITexture();
		guiFpsGraphTexture = base.gameObject.AddMissingComponent<UITexture>();
		guiFpsGraphTexture.mainTexture = texture;
		guiFpsGraphTexture.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D createGUITexture()
	{
		Texture2D texture2D = new Texture2D(1024, 256, TextureFormat.RGBA32, mipChain: false);
		for (int i = 0; i < texture2D.height; i++)
		{
			for (int j = 0; j < texture2D.width; j++)
			{
				texture2D.SetPixel(j, i, default(Color));
			}
		}
		texture2D.filterMode = FilterMode.Point;
		return texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFPSGraph()
	{
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		if (totalMemory < lastTotalMemory)
		{
			gcSpikeCounter = 3;
		}
		lastTotalMemory = totalMemory;
		int height = texture.height;
		int num = (int)Math.Min(height, Time.deltaTime * 2500f);
		float num2 = 1f / Time.deltaTime;
		Color color = ((num2 > 20f) ? ((!(num2 > 40f)) ? new Color(1f, 1f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.5f)) : ((!(num2 > 10f)) ? new Color(1f, 0f, 0f, 0.5f) : new Color(1f, 0.5f, 0f, 0.5f)));
		Color color2 = color;
		if (gcSpikeCounter-- > 0)
		{
			color2 = Color.magenta;
		}
		for (int i = 0; i <= num; i++)
		{
			texture.SetPixel(curGraphXPos, i, color2);
		}
		for (int j = num + 1; j < height; j++)
		{
			texture.SetPixel(curGraphXPos, j, new Color(0f, 0f, 0f, 0f));
		}
		for (int k = 0; k < height; k++)
		{
			texture.SetPixel(curGraphXPos + 1, k, new Color(0f, 0f, 0f, 0f));
		}
		for (int l = 10; l <= 60; l += 10)
		{
			texture.SetPixel(curGraphXPos, (int)(2500f / (float)l), new Color(1f, 1f, 1f, 0.5f));
			texture.SetPixel(curGraphXPos, (int)(2500f / (float)l) - 1, new Color(1f, 1f, 1f, 0.5f));
		}
		texture.Apply(updateMipmaps: false);
		curGraphXPos++;
		curGraphXPos %= texture.width - 1;
	}
}
