using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class SpawnPointManager : ISelectionBoxCallback
{
	public SpawnPointList spawnPointList = new SpawnPointList();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool bAddToSelectionBoxes;

	public SpawnPointManager(bool _bAddToSelectionBoxes = true)
	{
		bAddToSelectionBoxes = _bAddToSelectionBoxes;
		if (_bAddToSelectionBoxes)
		{
			SelectionBoxManager.Instance.CategoryStartPoint.SetCallback(this);
		}
	}

	public void Cleanup()
	{
		if (bAddToSelectionBoxes)
		{
			SelectionBoxManager.Instance.CategoryStartPoint.Clear();
		}
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		return true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		Vector3i vector3i = Vector3i.Parse(_box.name);
		Vector3i vector3i2 = vector3i + new Vector3i(_moveVector);
		_box.SetPositionAndSize(vector3i2, Vector3i.one);
		SelectionCategory category = _box.Category;
		Vector3i vector3i3 = vector3i2;
		category.RenameBox(_box, vector3i3.ToString() ?? "");
		SpawnPoint spawnPoint = spawnPointList.Find(vector3i);
		if (spawnPoint != null)
		{
			spawnPoint.spawnPosition.position = vector3i2.ToVector3();
		}
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		if (_checkCanDeleteOnly)
		{
			return true;
		}
		Vector3i blockPos = Vector3i.Parse(_box.name);
		SpawnPoint spawnPoint = spawnPointList.Find(blockPos);
		if (spawnPoint != null)
		{
			spawnPointList.Remove(spawnPoint);
		}
		return true;
	}

	public bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		_windowManager.SwitchVisible(XUiC_StartPointEditor.ID);
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
	}

	public bool Load(string _path)
	{
		if (!SdFile.Exists(_path + "/spawnpoints.xml"))
		{
			return false;
		}
		try
		{
			foreach (XElement item in new XmlFile(_path, "spawnpoints").XmlDoc.Root.Elements("spawnpoint"))
			{
				Vector3 position = StringParsers.ParseVector3(item.GetAttribute("position"));
				Vector3 vector = Vector3.zero;
				if (item.HasAttribute("rotation"))
				{
					vector = StringParsers.ParseVector3(item.GetAttribute("rotation"));
				}
				spawnPointList.Add(new SpawnPoint(position, vector.y));
			}
		}
		catch (Exception ex)
		{
			Log.Error("Loading spawnpoints xml file for level '" + Path.GetFileName(_path) + "': " + ex.Message);
		}
		return true;
	}

	public bool Save(string _path)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement node = xmlDocument.AddXmlElement("spawnpoints");
			for (int i = 0; i < spawnPointList.Count; i++)
			{
				SpawnPoint spawnPoint = spawnPointList[i];
				Vector3 position = spawnPoint.spawnPosition.position;
				string value = position.x.ToCultureInvariantString() + "," + position.y.ToCultureInvariantString() + "," + position.z.ToCultureInvariantString();
				node.AddXmlElement("spawnpoint").SetAttrib("position", value).SetAttrib("rotation", "0," + spawnPoint.spawnPosition.heading.ToCultureInvariantString() + ",0");
			}
			xmlDocument.SdSave(_path + "/spawnpoints.xml");
			return true;
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			return false;
		}
	}
}
