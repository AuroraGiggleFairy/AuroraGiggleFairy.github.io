using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionBox : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class SizeTextMeshDefinition
	{
		public readonly string Name;

		public readonly Vector3 Position;

		public readonly Vector3 Rotation;

		public readonly char[] Arrows;

		public SizeTextMeshDefinition(string _name, Vector3 _position, Vector3 _rotation, char[] _arrows)
		{
			Name = _name;
			Position = _position;
			Rotation = _rotation;
			Arrows = _arrows;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float boundsPadding = 0.16f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxFacingDirectionDistance = 62500;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int zTestShaderProperty = Shader.PropertyToID("_ZTest");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int colorShaderProperty = Shader.PropertyToID("_Color");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SelectionCategory ownerCategory;

	public object UserData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject frame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextMesh captionMesh;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextMesh[] sizeMeshes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size = Vector3i.zero;

	public Bounds bounds = BoundsUtils.BoundsForMinMax(Vector3.zero, Vector3.one);

	public RenderCubeType focusType = RenderCubeType.FullBlockBothSides;

	public bool bAlwaysDrawDirection;

	public bool bDrawDirection;

	public float facingDirection;

	public Vector3 AxisOrigin;

	public readonly List<Vector3> Axises = new List<Vector3>();

	public readonly List<Vector3i> AxisesI = new List<Vector3i>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i hightlightedAxis = Vector3i.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter m_MeshFilter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer m_MeshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int>[] subMeshTriangles = new List<int>[6];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2> m_Uvs = new List<Vector2>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3> m_Vertices = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Material[] materialsArr = new Material[6];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int collLayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string collTag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentChunkMeshIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCreated;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool[] faceColorsSet = new bool[6];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color curColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color curFrameColor = inActiveFrameColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool curShowingThroughWalls;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color inActiveFrameColor = Color.blue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color activeFrameColor = Color.green;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SizeTextMeshDefinition[] SizeTextMeshDefs = new SizeTextMeshDefinition[5]
	{
		new SizeTextMeshDefinition("Top", Vector3.up + new Vector3(0f, 0f, 0.2f), new Vector3(90f, 0f, 0f), new char[3] { '↔', '↗', '↕' }),
		new SizeTextMeshDefinition("Front", Vector3.back * 0.5f + Vector3.up * 0.5f, new Vector3(0f, 0f, 0f), new char[3] { '↔', '↕', '↗' }),
		new SizeTextMeshDefinition("Back", Vector3.forward * 0.5f + Vector3.up * 0.5f, new Vector3(0f, 180f, 0f), new char[3] { '↔', '↕', '↗' }),
		new SizeTextMeshDefinition("Left", Vector3.left * 0.5f + Vector3.up * 0.5f, new Vector3(0f, 90f, 0f), new char[3] { '↗', '↕', '↔' }),
		new SizeTextMeshDefinition("Right", Vector3.right * 0.5f + Vector3.up * 0.5f, new Vector3(0f, -90f, 0f), new char[3] { '↗', '↕', '↔' })
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		GameObject gameObject = new GameObject("Box");
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = bounds.size;
		m_MeshFilter = gameObject.AddComponent<MeshFilter>();
		m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
		for (int i = 0; i < materialsArr.Length; i++)
		{
			materialsArr[i] = new Material(Resources.Load<Shader>("Shaders/SelectionBox"));
			materialsArr[i].renderQueue = -1;
		}
		m_MeshRenderer.materials = materialsArr;
		for (int j = 0; j < subMeshTriangles.Length; j++)
		{
			subMeshTriangles[j] = new List<int>();
		}
		ResetAllFacesColor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(camPostRender));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(camPostRender));
		Utils.CleanupMaterials(materialsArr);
		Utils.CleanupMaterialsOfRenderer(m_MeshRenderer);
		if (frame != null)
		{
			UnityEngine.Object.Destroy(frame);
		}
	}

	public void SetOwner(SelectionCategory _selectionCategory)
	{
		ownerCategory = _selectionCategory;
	}

	public void SetAllFacesColor(Color _c, bool useAlphaMultiplier = true)
	{
		if (useAlphaMultiplier)
		{
			_c.a *= SelectionBoxManager.Instance.AlphaMultiplier;
		}
		if (m_MeshRenderer != null)
		{
			Material[] materials = m_MeshRenderer.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i].color = _c;
			}
		}
		curColor = _c;
	}

	public void ResetAllFacesColor()
	{
		SetAllFacesColor(curColor, useAlphaMultiplier: false);
	}

	public void SetFaceColor(BlockFace _face, Color _c)
	{
		m_MeshRenderer.materials[(uint)_face].color = _c;
		faceColorsSet[(uint)_face] = true;
	}

	public void SetCaption(string _text)
	{
		if (captionMesh == null)
		{
			GameObject gameObject = new GameObject("Caption");
			gameObject.transform.parent = base.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			gameObject.transform.localPosition = new Vector3(0f, bounds.size.y + 0.1f, 0f);
			captionMesh = gameObject.AddMissingComponent<TextMesh>();
			captionMesh.alignment = TextAlignment.Center;
			captionMesh.anchor = TextAnchor.MiddleCenter;
			captionMesh.fontSize = 20;
			captionMesh.color = Color.green;
			gameObject.SetActive(value: true);
		}
		captionMesh.text = _text;
	}

	public void SetCaptionVisibility(bool _visible)
	{
		if (!(captionMesh == null))
		{
			captionMesh.gameObject.SetActive(_visible);
		}
	}

	public void SetPositionAndSize(Vector3 _pos, Vector3i _size)
	{
		base.transform.localPosition = _pos + new Vector3((float)_size.x * 0.5f, -0.1f, (float)_size.z * 0.5f) - Origin.position;
		bounds = BoundsUtils.BoundsForMinMax(_pos, _pos + _size);
		bounds.size += new Vector3(0.16f, 0.16f, 0.16f);
		Transform boxTransform = GetBoxTransform();
		if (boxTransform != null)
		{
			boxTransform.localScale = bounds.size;
		}
		if (size != _size)
		{
			BuildFrame();
			if (captionMesh != null)
			{
				captionMesh.transform.localPosition = new Vector3(0f, bounds.size.y + 0.1f, 0f);
			}
			UpdateSizeMeshes(_size);
		}
		size = _size;
	}

	public void SetVisible(bool _visible)
	{
		if (base.gameObject.activeSelf == _visible)
		{
			return;
		}
		base.gameObject.SetActive(_visible);
		if (_visible)
		{
			ResetAllFacesColor();
			SetFrameActive(_active: false);
		}
		string text = ownerCategory.name;
		if (!(text == "SleeperVolume"))
		{
			if (text == "POIMarker")
			{
				POIMarkerToolManager.ShowPOIMarkers(_visible);
			}
		}
		else
		{
			SleeperVolumeToolManager.ShowSleepers(_visible);
		}
	}

	public Vector3i GetScale()
	{
		return new Vector3i(bounds.size);
	}

	public Transform GetBoxTransform()
	{
		return base.transform.Find("Box");
	}

	public void SetFrameActive(bool _active)
	{
		SetFrameColor(_active ? activeFrameColor : inActiveFrameColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFrameColor(Color _color)
	{
		curFrameColor = _color;
		if (frame != null)
		{
			frame.GetComponent<MeshRenderer>().material.SetColor(colorShaderProperty, _color);
		}
	}

	public void ShowThroughWalls(bool _bShow)
	{
		if (curShowingThroughWalls != _bShow)
		{
			curShowingThroughWalls = _bShow;
			int value = (_bShow ? 8 : 4);
			if (frame != null)
			{
				frame.GetComponent<MeshRenderer>().material.SetInt(zTestShaderProperty, value);
			}
			Material[] materials = m_MeshRenderer.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i].SetInt(zTestShaderProperty, value);
			}
		}
	}

	public void EnableCollider(string _tag, int _layer)
	{
		collLayer = _layer;
		collTag = _tag;
	}

	public void HighlightAxis(Vector3i _hightlightedAxis)
	{
		hightlightedAxis = _hightlightedAxis;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (!bCreated && createMesh())
		{
			bCreated = true;
		}
	}

	public void SetSizeVisibility(bool _visible)
	{
		if (_visible && sizeMeshes == null)
		{
			sizeMeshes = new TextMesh[SizeTextMeshDefs.Length];
			for (int i = 0; i < sizeMeshes.Length; i++)
			{
				SizeTextMeshDefinition sizeTextMeshDefinition = SizeTextMeshDefs[i];
				GameObject obj = new GameObject("Size_" + sizeTextMeshDefinition.Name);
				obj.transform.parent = base.transform;
				obj.transform.localScale = Vector3.one;
				obj.transform.localPosition = Vector3.Scale(bounds.size, sizeTextMeshDefinition.Position);
				obj.transform.rotation = Quaternion.Euler(sizeTextMeshDefinition.Rotation);
				TextMesh textMesh = obj.AddMissingComponent<TextMesh>();
				sizeMeshes[i] = textMesh;
				textMesh.alignment = TextAlignment.Center;
				textMesh.anchor = TextAnchor.MiddleCenter;
				textMesh.characterSize = 0.1f;
				textMesh.fontSize = 20;
				textMesh.color = Color.green;
				textMesh.text = sizeTextMeshDefinition.Name;
				obj.SetActive(value: true);
			}
			UpdateSizeMeshes(size);
		}
		if (sizeMeshes != null)
		{
			TextMesh[] array = sizeMeshes;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].gameObject.SetActive(_visible);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSizeMeshes(Vector3i _newSize)
	{
		if (sizeMeshes != null)
		{
			for (int i = 0; i < sizeMeshes.Length; i++)
			{
				SizeTextMeshDefinition sizeTextMeshDefinition = SizeTextMeshDefs[i];
				sizeMeshes[i].text = $"{sizeTextMeshDefinition.Arrows[0]}{_newSize.x} {sizeTextMeshDefinition.Arrows[1]}{_newSize.y} {sizeTextMeshDefinition.Arrows[2]}{_newSize.z}";
				sizeMeshes[i].transform.localPosition = Vector3.Scale(bounds.size, sizeTextMeshDefinition.Position);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void camPostRender(Camera _cam)
	{
		if (_cam != Camera.main)
		{
			return;
		}
		Vector3 position = GameManager.Instance.World.GetPrimaryPlayer().position;
		Vector3 center = bounds.center;
		float sqrMagnitude = (position - center).sqrMagnitude;
		bool flag = SelectionBoxManager.Instance.Selection?.box == this;
		if ((!flag && sqrMagnitude > 62500f) || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		GUIUtils.SetupLines(_cam, 3f);
		if (bDrawDirection && (flag || bAlwaysDrawDirection))
		{
			Vector3 vector = base.transform.position + new Vector3(0f, bounds.size.y, 0f);
			float max = Mathf.Min(15f, 4f * Mathf.Max(bounds.size.x, bounds.size.z));
			float num = Mathf.Clamp((_cam.transform.position - vector).magnitude / 10f, 1f, max);
			Vector3 vector2 = Quaternion.AngleAxis(facingDirection, Vector3.up) * Vector3.forward;
			vector += vector2 * 0.5f * Math.Min(bounds.size.x - 1f, bounds.size.z - 1f);
			GUIUtils.DrawTriangleWide(vector, vector2, Vector3.up, num * 0.5f, Color.black);
		}
		if (!flag || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			return;
		}
		Vector3 vector3 = (AxisOrigin = base.transform.position + new Vector3(0f, bounds.size.y * 0.5f, 0f));
		float num2 = Mathf.Max(1f, (_cam.transform.position - vector3).magnitude / 10f);
		Color colorA = new Color(0.3f, 0.05f, 0.05f);
		Color color = new Color(1f, 0.6f, 0.6f);
		Color colorA2 = new Color(0.05f, 0.2f, 0.05f);
		Color color2 = new Color(0f, 0.7f, 0f);
		Color color3 = new Color(0.6f, 1f, 0.6f);
		Color colorA3 = new Color(0.05f, 0.05f, 0.3f);
		Color color4 = new Color(0.4f, 0.4f, 1f);
		Color color5 = new Color(0.7f, 0.7f, 1f);
		Axises.Clear();
		AxisesI.Clear();
		if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 1)
		{
			Axises.Add(vector3 + num2 * Vector3.right);
			Axises.Add(vector3 + num2 * Vector3.up);
			Axises.Add(vector3 + num2 * Vector3.forward);
			AxisesI.Add(Vector3i.right);
			AxisesI.Add(Vector3i.up);
			AxisesI.Add(Vector3i.forward);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[0], colorA, (hightlightedAxis.x != 0) ? color : Color.red);
			GUIUtils.DrawTriangleWide(Axises[0], (Axises[0] - AxisOrigin).normalized, Vector3.up, num2 * 0.125f, (hightlightedAxis.x != 0) ? Color.yellow : Color.red);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[1], colorA2, (hightlightedAxis.y != 0) ? color3 : color2);
			GUIUtils.DrawTriangleWide(Axises[1], (Axises[1] - AxisOrigin).normalized, Vector3.right, num2 * 0.125f, (hightlightedAxis.y != 0) ? Color.yellow : color2);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[2], colorA3, (hightlightedAxis.z != 0) ? color5 : color4);
			GUIUtils.DrawTriangleWide(Axises[2], (Axises[2] - AxisOrigin).normalized, Vector3.up, num2 * 0.125f, (hightlightedAxis.z != 0) ? Color.yellow : color4);
		}
		else if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 2)
		{
			if (ownerCategory.callback.OnSelectionBoxIsAvailable(ownerCategory.name, EnumSelectionBoxAvailabilities.CanResize))
			{
				Axises.Add(vector3 + num2 * Vector3.right);
				Axises.Add(vector3 - num2 * Vector3.right);
				Axises.Add(vector3 + num2 * Vector3.up);
				Axises.Add(vector3 - num2 * Vector3.up);
				Axises.Add(vector3 + num2 * Vector3.forward);
				Axises.Add(vector3 - num2 * Vector3.forward);
				Axises.Add(vector3);
				AxisesI.Add(Vector3i.right);
				AxisesI.Add(Vector3i.left);
				AxisesI.Add(Vector3i.up);
				AxisesI.Add(Vector3i.down);
				AxisesI.Add(Vector3i.forward);
				AxisesI.Add(Vector3i.back);
				AxisesI.Add(Vector3i.one);
				GUIUtils.DrawLineWide(AxisOrigin, Axises[0], colorA, (hightlightedAxis.x > 0) ? color : Color.red);
				GUIUtils.DrawRectWide(Axises[0], (Axises[0] - AxisOrigin).normalized, Vector3.up, num2 * 0.125f, (hightlightedAxis.x > 0) ? Color.yellow : Color.red);
				GUIUtils.DrawLineWide(AxisOrigin, Axises[1], colorA, (hightlightedAxis.x < 0) ? color : Color.red);
				GUIUtils.DrawRectWide(Axises[1], (Axises[1] - AxisOrigin).normalized, Vector3.up, num2 * 0.125f, (hightlightedAxis.x < 0) ? Color.yellow : Color.red);
				GUIUtils.DrawLineWide(AxisOrigin, Axises[2], colorA2, (hightlightedAxis.y > 0) ? color3 : color2);
				GUIUtils.DrawRectWide(Axises[2], (Axises[2] - AxisOrigin).normalized, Vector3.right, num2 * 0.125f, (hightlightedAxis.y > 0) ? Color.yellow : color2);
				GUIUtils.DrawLineWide(AxisOrigin, Axises[3], colorA2, (hightlightedAxis.y < 0) ? color3 : color2);
				GUIUtils.DrawRectWide(Axises[3], (Axises[3] - AxisOrigin).normalized, Vector3.right, num2 * 0.125f, (hightlightedAxis.y < 0) ? Color.yellow : color2);
				GUIUtils.DrawLineWide(AxisOrigin, Axises[4], colorA3, (hightlightedAxis.z > 0) ? color5 : color4);
				GUIUtils.DrawRectWide(Axises[4], (Axises[4] - AxisOrigin).normalized, Vector3.up, num2 * 0.125f, (hightlightedAxis.z > 0) ? Color.yellow : color4);
				GUIUtils.DrawLineWide(AxisOrigin, Axises[5], colorA3, (hightlightedAxis.z < 0) ? color5 : color4);
				GUIUtils.DrawRectWide(Axises[5], (Axises[5] - AxisOrigin).normalized, Vector3.up, num2 * 0.125f, (hightlightedAxis.z < 0) ? Color.yellow : color4);
			}
		}
		else if (GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 3 && ownerCategory.callback.OnSelectionBoxIsAvailable(ownerCategory.name, EnumSelectionBoxAvailabilities.CanMirror))
		{
			Axises.Add(vector3 + num2 * Vector3.right);
			Axises.Add(vector3 - num2 * Vector3.right);
			Axises.Add(vector3 + num2 * Vector3.up);
			Axises.Add(vector3 - num2 * Vector3.up);
			Axises.Add(vector3 + num2 * Vector3.forward);
			Axises.Add(vector3 - num2 * Vector3.forward);
			AxisesI.Add(Vector3i.right);
			AxisesI.Add(Vector3i.left);
			AxisesI.Add(Vector3i.up);
			AxisesI.Add(Vector3i.down);
			AxisesI.Add(Vector3i.forward);
			AxisesI.Add(Vector3i.back);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[0], colorA, (hightlightedAxis.x > 0) ? color : Color.red);
			GUIUtils.DrawLineWide(Axises[0] - Vector3.up * 0.2f, Axises[0] + Vector3.up * 0.2f, (hightlightedAxis.x > 0) ? Color.yellow : Color.red);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[1], colorA, (hightlightedAxis.x < 0) ? color : Color.red);
			GUIUtils.DrawLineWide(Axises[1] - Vector3.up * 0.2f, Axises[1] + Vector3.up * 0.2f, (hightlightedAxis.x < 0) ? Color.yellow : Color.red);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[2], colorA2, (hightlightedAxis.y > 0) ? color3 : color2);
			GUIUtils.DrawLineWide(Axises[2] - Vector3.right * 0.2f, Axises[2] + Vector3.right * 0.2f, (hightlightedAxis.y > 0) ? Color.yellow : color2);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[3], colorA2, (hightlightedAxis.y < 0) ? color3 : color2);
			GUIUtils.DrawLineWide(Axises[3] - Vector3.right * 0.2f, Axises[3] + Vector3.right * 0.2f, (hightlightedAxis.y < 0) ? Color.yellow : color2);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[4], colorA3, (hightlightedAxis.z > 0) ? color5 : color4);
			GUIUtils.DrawLineWide(Axises[4] - Vector3.right * 0.2f, Axises[4] + Vector3.right * 0.2f, (hightlightedAxis.z > 0) ? Color.yellow : color4);
			GUIUtils.DrawLineWide(AxisOrigin, Axises[5], colorA3, (hightlightedAxis.z < 0) ? color5 : color4);
			GUIUtils.DrawLineWide(Axises[5] - Vector3.right * 0.2f, Axises[5] + Vector3.right * 0.2f, (hightlightedAxis.z < 0) ? Color.yellow : color4);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildFrame()
	{
		if (frame != null)
		{
			UnityEngine.Object.Destroy(frame);
			frame = null;
		}
		frame = new GameObject("Frame");
		frame.transform.parent = base.transform;
		frame.transform.localScale = Vector3.one;
		frame.transform.localPosition = Vector3.zero;
		float num = 0.02f;
		Bounds bounds = this.bounds;
		bounds.center = new Vector3(0f, this.bounds.size.y / 2f, 0f);
		Mesh mesh = new Mesh();
		mesh.Clear(keepVertexLayout: false);
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y + num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y + num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.min.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y - num, bounds.min.z - num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.min.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.min.x - num, bounds.max.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.min.x + num, bounds.max.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.min.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y + num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.min.y + num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.max.z + num));
		list.Add(new Vector3(bounds.max.x - num, bounds.max.y - num, bounds.max.z - num));
		list.Add(new Vector3(bounds.max.x + num, bounds.max.y - num, bounds.max.z - num));
		for (int i = 0; i < 12; i++)
		{
			list2.Add(i * 8);
			list2.Add(i * 8 + 1);
			list2.Add(i * 8 + 2);
			list2.Add(i * 8 + 2);
			list2.Add(i * 8 + 3);
			list2.Add(i * 8);
			list2.Add(i * 8 + 3);
			list2.Add(i * 8 + 2);
			list2.Add(i * 8 + 7);
			list2.Add(i * 8 + 7);
			list2.Add(i * 8 + 4);
			list2.Add(i * 8 + 3);
			list2.Add(i * 8 + 4);
			list2.Add(i * 8 + 7);
			list2.Add(i * 8 + 6);
			list2.Add(i * 8 + 6);
			list2.Add(i * 8 + 5);
			list2.Add(i * 8 + 4);
			list2.Add(i * 8 + 5);
			list2.Add(i * 8 + 6);
			list2.Add(i * 8 + 1);
			list2.Add(i * 8 + 1);
			list2.Add(i * 8);
			list2.Add(i * 8 + 5);
			list2.Add(i * 8 + 1);
			list2.Add(i * 8 + 6);
			list2.Add(i * 8 + 7);
			list2.Add(i * 8 + 7);
			list2.Add(i * 8 + 2);
			list2.Add(i * 8 + 1);
			list2.Add(i * 8 + 5);
			list2.Add(i * 8);
			list2.Add(i * 8 + 3);
			list2.Add(i * 8 + 3);
			list2.Add(i * 8 + 4);
			list2.Add(i * 8 + 5);
		}
		mesh.SetVertices(list);
		mesh.SetIndices(list2.ToArray(), MeshTopology.Triangles, 0);
		frame.AddComponent<MeshFilter>().mesh = mesh;
		frame.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/SleeperVolumeFrame"));
		SetFrameColor(curFrameColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addQuad(Vector3 _v1, Vector2 _uv1, Vector3 _v2, Vector2 _uv2, Vector3 _v3, Vector2 _uv3, Vector3 _v4, Vector2 _uv4, BlockFace _face)
	{
		m_Vertices.Add(_v1);
		m_Vertices.Add(_v2);
		m_Vertices.Add(_v3);
		m_Vertices.Add(_v4);
		m_Uvs.Add(_uv1);
		m_Uvs.Add(_uv2);
		m_Uvs.Add(_uv3);
		m_Uvs.Add(_uv4);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex + 2);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex + 1);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex + 3);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex + 2);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex);
		currentChunkMeshIndex += 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addTriangle(Vector3 _v1, Vector2 _uv1, Vector3 _v2, Vector2 _uv2, Vector3 _v3, Vector2 _uv3, BlockFace _face)
	{
		m_Vertices.Add(_v1);
		m_Vertices.Add(_v2);
		m_Vertices.Add(_v3);
		m_Uvs.Add(_uv1);
		m_Uvs.Add(_uv2);
		m_Uvs.Add(_uv3);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex + 2);
		subMeshTriangles[(uint)_face].Add(currentChunkMeshIndex + 1);
		currentChunkMeshIndex += 3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool createMesh()
	{
		float num = -0.5f;
		float num2 = 0f;
		float num3 = -0.5f;
		Bounds bounds = BoundsUtils.BoundsForMinMax(Vector3.zero, Vector3.one);
		num += bounds.min.x;
		num2 += bounds.min.y;
		num3 += bounds.min.z;
		float num4 = bounds.max.x - bounds.min.x;
		float num5 = bounds.max.y - bounds.min.y;
		float num6 = bounds.max.z - bounds.min.z;
		for (int i = 0; i < subMeshTriangles.Length; i++)
		{
			subMeshTriangles[i].Clear();
		}
		m_Uvs.Clear();
		m_Vertices.Clear();
		currentChunkMeshIndex = 0;
		if (focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockInnerSides)
		{
			addQuad(new Vector3(num, num2, num3), new Vector2(1f, 0f), new Vector3(num, num2 + num5, num3), new Vector2(1f, 1f), new Vector3(num + num4, num2 + num5, num3), new Vector2(0f, 1f), new Vector3(num + num4, num2, num3), new Vector2(0f, 0f), BlockFace.South);
			addQuad(new Vector3(num, num2, num3 + num6), new Vector2(0f, 0f), new Vector3(num, num2 + num5, num3 + num6), new Vector2(0f, 1f), new Vector3(num, num2 + num5, num3), new Vector2(1f, 1f), new Vector3(num, num2, num3), new Vector2(1f, 0f), BlockFace.West);
			addQuad(new Vector3(num + num4, num2, num3 + num6), new Vector2(0f, 0f), new Vector3(num + num4, num2 + num5, num3 + num6), new Vector2(0f, 1f), new Vector3(num, num2 + num5, num3 + num6), new Vector2(1f, 1f), new Vector3(num, num2, num3 + num6), new Vector2(1f, 0f), BlockFace.North);
			addQuad(new Vector3(num + num4, num2, num3), new Vector2(1f, 0f), new Vector3(num + num4, num2 + num5, num3), new Vector2(1f, 1f), new Vector3(num + num4, num2 + num5, num3 + num6), new Vector2(0f, 1f), new Vector3(num + num4, num2, num3 + num6), new Vector2(0f, 0f), BlockFace.East);
			addQuad(new Vector3(num, num2 + num5, num3), new Vector2(1f, 0f), new Vector3(num, num2 + num5, num3 + num6), new Vector2(1f, 1f), new Vector3(num + num4, num2 + num5, num3 + num6), new Vector2(0f, 1f), new Vector3(num + num4, num2 + num5, num3), new Vector2(0f, 0f), BlockFace.Top);
			addQuad(new Vector3(num + num4, num2, num3), new Vector2(0f, 0f), new Vector3(num + num4, num2, num3 + num6), new Vector2(0f, 1f), new Vector3(num, num2, num3 + num6), new Vector2(1f, 1f), new Vector3(num, num2, num3), new Vector2(1f, 0f), BlockFace.Bottom);
		}
		if (focusType == RenderCubeType.FaceS || focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockOuterSides)
		{
			addQuad(new Vector3(num + num4, num2, num3), new Vector2(1f, 0f), new Vector3(num + num4, num2 + num5, num3), new Vector2(1f, 1f), new Vector3(num, num2 + num5, num3), new Vector2(0f, 1f), new Vector3(num, num2, num3), new Vector2(0f, 0f), BlockFace.South);
		}
		if (focusType == RenderCubeType.FaceW || focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockOuterSides)
		{
			addQuad(new Vector3(num, num2, num3), new Vector2(0f, 0f), new Vector3(num, num2 + num5, num3), new Vector2(0f, 1f), new Vector3(num, num2 + num5, num3 + num6), new Vector2(1f, 1f), new Vector3(num, num2, num3 + num6), new Vector2(1f, 0f), BlockFace.West);
		}
		if (focusType == RenderCubeType.FaceN || focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockOuterSides)
		{
			addQuad(new Vector3(num, num2, num3 + num6), new Vector2(0f, 0f), new Vector3(num, num2 + num5, num3 + num6), new Vector2(0f, 1f), new Vector3(num + num4, num2 + num5, num3 + num6), new Vector2(1f, 1f), new Vector3(num + num4, num2, num3 + num6), new Vector2(1f, 0f), BlockFace.North);
		}
		if (focusType == RenderCubeType.FaceE || focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockOuterSides)
		{
			addQuad(new Vector3(num + num4, num2, num3 + num6), new Vector2(1f, 0f), new Vector3(num + num4, num2 + num5, num3 + num6), new Vector2(1f, 1f), new Vector3(num + num4, num2 + num5, num3), new Vector2(0f, 1f), new Vector3(num + num4, num2, num3), new Vector2(0f, 0f), BlockFace.East);
		}
		if (focusType == RenderCubeType.FaceTop || focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockOuterSides)
		{
			addQuad(new Vector3(num + num4, num2 + num5, num3), new Vector2(1f, 0f), new Vector3(num + num4, num2 + num5, num3 + num6), new Vector2(1f, 1f), new Vector3(num, num2 + num5, num3 + num6), new Vector2(0f, 1f), new Vector3(num, num2 + num5, num3), new Vector2(0f, 0f), BlockFace.Top);
		}
		if (focusType == RenderCubeType.FaceBottom || focusType == RenderCubeType.FullBlockBothSides || focusType == RenderCubeType.FullBlockOuterSides)
		{
			addQuad(new Vector3(num, num2, num3), new Vector2(0f, 0f), new Vector3(num, num2, num3 + num6), new Vector2(0f, 1f), new Vector3(num + num4, num2, num3 + num6), new Vector2(1f, 1f), new Vector3(num + num4, num2, num3), new Vector2(1f, 0f), BlockFace.Bottom);
		}
		m_MeshFilter.mesh.Clear();
		m_MeshFilter.mesh.vertices = m_Vertices.ToArray();
		if (m_Uvs.Count > 0)
		{
			m_MeshFilter.mesh.uv = m_Uvs.ToArray();
		}
		m_MeshFilter.mesh.subMeshCount = subMeshTriangles.Length;
		for (int j = 0; j < subMeshTriangles.Length; j++)
		{
			m_MeshFilter.mesh.SetTriangles(subMeshTriangles[j].ToArray(), j);
		}
		m_MeshFilter.mesh.RecalculateNormals();
		if (collTag != null)
		{
			m_MeshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = copyMeshAndAddBackFaces(m_MeshFilter.mesh);
			GameObject obj = m_MeshFilter.gameObject;
			obj.tag = collTag;
			obj.layer = collLayer;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh copyMeshAndAddBackFaces(Mesh _mesh)
	{
		Vector3[] vertices = _mesh.vertices;
		List<int> list = new List<int>();
		for (int i = 0; i < _mesh.subMeshCount; i++)
		{
			int[] triangles = _mesh.GetTriangles(i);
			foreach (int item in triangles)
			{
				list.Add(item);
			}
		}
		int count = list.Count;
		for (int k = 0; k < count; k += 3)
		{
			list.Add(list[k]);
			list.Add(list[k + 2]);
			list.Add(list[k + 1]);
		}
		return new Mesh
		{
			vertices = vertices,
			triangles = list.ToArray()
		};
	}
}
