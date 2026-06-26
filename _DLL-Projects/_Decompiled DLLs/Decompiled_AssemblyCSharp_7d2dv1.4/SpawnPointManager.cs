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
			SelectionBoxManager.Instance.GetCategory("StartPoint").SetCallback(this);
		}
	}

	public void Cleanup()
	{
		if (bAddToSelectionBoxes)
		{
			SelectionBoxManager.Instance.GetCategory("StartPoint").Clear();
		}
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		return true;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		Vector3i vector3i = Vector3i.Parse(_name);
		Vector3i vector3i2 = vector3i + new Vector3i(_moveVector);
		SelectionBoxManager.Instance.GetCategory(_category).GetBox(_name).SetPositionAndSize(vector3i2, Vector3i.one);
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory(_category);
		Vector3i vector3i3 = vector3i2;
		category.RenameBox(_name, vector3i3.ToString() ?? "");
		SpawnPoint spawnPoint = spawnPointList.Find(vector3i);
		if (spawnPoint != null)
		{
			spawnPoint.spawnPosition.position = vector3i2.ToVector3();
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		Vector3i blockPos = Vector3i.Parse(_name);
		SpawnPoint spawnPoint = spawnPointList.Find(blockPos);
		if (spawnPoint != null)
		{
			spawnPointList.Remove(spawnPoint);
		}
		return true;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		if (SelectionBoxManager.Instance.GetSelected(out var _selectedCategory, out var _) && _selectedCategory.Equals("StartPoint"))
		{
			_windowManager.SwitchVisible(XUiC_StartPointEditor.ID);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
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
