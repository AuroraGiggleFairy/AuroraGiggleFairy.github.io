using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugLines : MonoBehaviour
{
	public static Vector3 InsideOffsetV = new Vector3(0.05f, 0.05f, 0.05f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cName = "DebugLines";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, DebugLines> lines = new Dictionary<string, DebugLines>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string keyName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LineRenderer line;

	public static DebugLines Create(string _name, Transform _parentT, Color _color1, Color _color2, float _width1, float _width2, float _duration)
	{
		DebugLines debugLines = Create(_name, _parentT);
		debugLines.duration = _duration;
		LineRenderer lineRenderer = debugLines.line;
		lineRenderer.startColor = _color1;
		lineRenderer.startWidth = _width1;
		lineRenderer.endColor = _color2;
		lineRenderer.endWidth = _width2;
		return debugLines;
	}

	public static DebugLines Create(string _name, Transform _parentT, Vector3 _pos1, Vector3 _pos2, Color _color1, Color _color2, float _width1, float _width2, float _duration)
	{
		DebugLines debugLines = Create(_name, _parentT);
		debugLines.duration = _duration;
		LineRenderer lineRenderer = debugLines.line;
		lineRenderer.startColor = _color1;
		lineRenderer.startWidth = _width1;
		lineRenderer.endColor = _color2;
		lineRenderer.endWidth = _width2;
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, _pos1 - Origin.position);
		lineRenderer.SetPosition(1, _pos2 - Origin.position);
		return debugLines;
	}

	public static DebugLines CreateAttached(string _name, Transform _parentT, Vector3 _pos1, Vector3 _pos2, Color _color1, Color _color2, float _width1, float _width2, float _duration)
	{
		DebugLines debugLines = Create(_name, _parentT);
		debugLines.duration = _duration;
		LineRenderer lineRenderer = debugLines.line;
		lineRenderer.useWorldSpace = false;
		lineRenderer.startColor = _color1;
		lineRenderer.startWidth = _width1;
		lineRenderer.endColor = _color2;
		lineRenderer.endWidth = _width2;
		lineRenderer.positionCount = 2;
		Vector3 position = _parentT.InverseTransformPoint(_pos1 - Origin.position);
		lineRenderer.SetPosition(0, position);
		Vector3 position2 = _parentT.InverseTransformPoint(_pos2 - Origin.position);
		lineRenderer.SetPosition(1, position2);
		return debugLines;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DebugLines Create(string _name, Transform _parentT)
	{
		DebugLines value = null;
		string text = "DebugLines";
		if (_name != null)
		{
			text += _name;
			lines.TryGetValue(_name, out value);
		}
		if (!value)
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)Resources.Load("Prefabs/Debug/DebugLines"), _parentT);
			obj.name = text;
			value = obj.transform.GetComponent<DebugLines>();
			if (_name != null)
			{
				value.keyName = _name;
				lines[_name] = value;
			}
		}
		else
		{
			value.transform.SetParent(_parentT, worldPositionStays: false);
		}
		value.line = value.GetComponent<LineRenderer>();
		value.line.positionCount = 0;
		return value;
	}

	public void AddPoint(Vector3 _pos)
	{
		int positionCount = line.positionCount;
		line.positionCount = positionCount + 1;
		Vector3 position = _pos - Origin.position;
		if (!line.useWorldSpace)
		{
			position = base.transform.InverseTransformPoint(position);
		}
		line.SetPosition(positionCount, position);
	}

	public void AddCube(Vector3 _cornerPos1, Vector3 _cornerPos2)
	{
		Vector3 pos = _cornerPos1;
		Vector3 vector = _cornerPos2 - _cornerPos1;
		AddPoint(pos);
		pos.x += vector.x;
		AddPoint(pos);
		pos.y += vector.y;
		AddPoint(pos);
		pos.y -= vector.y;
		AddPoint(pos);
		pos.z += vector.z;
		AddPoint(pos);
		pos.y += vector.y;
		AddPoint(pos);
		pos.y -= vector.y;
		AddPoint(pos);
		pos.x -= vector.x;
		AddPoint(pos);
		pos.y += vector.y;
		AddPoint(pos);
		pos.y -= vector.y;
		AddPoint(pos);
		pos.z -= vector.z;
		AddPoint(pos);
		pos.y += vector.y;
		AddPoint(pos);
		pos.x += vector.x;
		AddPoint(pos);
		pos.z += vector.z;
		AddPoint(pos);
		pos.x -= vector.x;
		AddPoint(pos);
		pos.z -= vector.z;
		AddPoint(pos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		duration -= Time.deltaTime;
		if (duration <= 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (keyName != null)
		{
			lines.Remove(keyName);
		}
	}
}
