using System;
using System.Collections.Generic;
using UnityEngine;

public class NguiWdwTerrainEditor : MonoBehaviour, INGuiButtonOnClick
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameManager gm;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IBlockTool> tools;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Transform> toolButtons;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTools.Brush brush;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject toolButtonPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform thisWindow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform toolGrid;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform projectorParent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Projector brushSizeProjector;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Projector brushFalloffPojector;

	public Vector3i lastPosition;

	public Vector3 lastDirection;

	public Texture2D[] buttonTextures;

	public string[] buttonNames;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel sizeVal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel falloffVal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel strengthVal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UIAnchor anchor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UIPanel panel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIWindowManager nguiWindowManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int size;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float hardness;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float flow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		nguiWindowManager = GetComponentInParent<NGUIWindowManager>();
		gm = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		if (!GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		thisWindow = base.transform;
		toolGrid = thisWindow.Find("Toolbox/ToolGrid");
		sizeVal = thisWindow.Find("Toolbox/Sliders/1_Size/Value").GetComponent<UILabel>();
		falloffVal = thisWindow.Find("Toolbox/Sliders/2_Falloff/Value").GetComponent<UILabel>();
		strengthVal = thisWindow.Find("Toolbox/Sliders/3_Strength/Value").GetComponent<UILabel>();
		anchor = thisWindow.GetComponent<UIAnchor>();
		tools = new List<IBlockTool>();
		brush = new BlockTools.Brush(BlockTools.Brush.BrushShape.Sphere, 1, 10, 80);
		toolButtonPrefab = thisWindow.Find("Toolbox/ToolButton").gameObject;
		toolButtons = new List<Transform>();
		tools.Add(new BlockToolTerrainAdjust(brush, this));
		tools.Add(new BlockToolTerrainSmoothing(brush, this));
		tools.Add(new BlockToolTerrainPaint(brush, this));
		GameObject gameObject = Resources.Load("Prefabs/prefabTerrainBrush") as GameObject;
		if (!(gameObject == null))
		{
			projectorParent = UnityEngine.Object.Instantiate(gameObject).transform;
			for (int i = 0; i < tools.Count; i++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(toolButtonPrefab);
				obj.SetActive(value: true);
				obj.name = tools[i].ToString() + " Button";
				Transform transform = obj.transform;
				transform.parent = toolGrid;
				transform.localPosition = Vector3.zero;
				transform.localScale = Vector3.one;
				transform.GetComponent<NGuiButtonOnClickHandler>().OnClickDelegate = this;
				toolButtons.Add(transform);
			}
			toolGrid.GetComponent<UIGrid>().repositionNow = true;
			gm.SetActiveBlockTool(tools[0]);
			panel = GetComponent<UIPanel>();
		}
	}

	public void InGameMenuOpen(bool _isOpen)
	{
		if (_isOpen)
		{
			anchor.pixelOffset = new Vector2(150f, -7f);
		}
		else
		{
			anchor.pixelOffset = new Vector2(0f, -7f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		if (projectorParent != null)
		{
			projectorParent.gameObject.SetActive(value: true);
		}
		if (gm != null)
		{
			gm.SetActiveBlockTool(tools[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		if (projectorParent != null)
		{
			projectorParent.gameObject.SetActive(value: false);
		}
		if (gm != null)
		{
			gm.SetActiveBlockTool(null);
		}
	}

	public void OnSizeChange()
	{
		float value = UIProgressBar.current.value;
		size = (int)(value * 32f);
		brush.Falloff = size;
		brush.Size = (int)((float)size * hardness);
		sizeVal.text = size.ToString();
	}

	public void OnFalloffChange()
	{
		float value = UIProgressBar.current.value;
		hardness = value;
		brush.Falloff = size;
		brush.Size = (int)((float)size * hardness);
		falloffVal.text = hardness.ToCultureInvariantString();
	}

	public void OnStrengthChange()
	{
		float value = UIProgressBar.current.value;
		flow = value;
		brush.Strength = (int)(flow * 127f);
		strengthVal.text = flow.ToCultureInvariantString();
	}

	public void HideWindow(bool _hide)
	{
		if (_hide)
		{
			panel.alpha = 0f;
		}
		else
		{
			panel.alpha = 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (projectorParent != null)
		{
			projectorParent.parent = null;
			projectorParent.position = lastPosition.ToVector3();
			projectorParent.localScale = Vector3.one;
			projectorParent.Find("Size").transform.localScale = Vector3.one * (brush.Size * 2);
			projectorParent.Find("Falloff").transform.localScale = Vector3.one * (brush.Falloff * 2);
		}
		InGameMenuOpen(nguiWindowManager.WindowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID));
	}

	public void NGuiButtonOnClick(Transform _t)
	{
		for (int i = 0; i < toolButtons.Count; i++)
		{
			if (_t == toolButtons[i])
			{
				gm.SetActiveBlockTool(tools[i]);
				Log.Out(tools[i].ToString());
			}
			toolButtons[i].Find("Highlight").gameObject.SetActive(_t == toolButtons[i]);
		}
	}
}
