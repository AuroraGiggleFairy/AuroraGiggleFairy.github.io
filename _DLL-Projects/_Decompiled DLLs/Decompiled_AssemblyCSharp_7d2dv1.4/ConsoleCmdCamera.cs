using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCamera : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class CameraPosition : IEquatable<CameraPosition>
	{
		public readonly Vector3 Position;

		public readonly Vector3 Direction;

		public readonly string Comment;

		public CameraPosition(Vector3 _position, Vector3 _direction, string _comment)
		{
			Position = _position;
			Direction = _direction;
			Comment = _comment;
		}

		public bool Equals(CameraPosition _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			if (Position.Equals(_other.Position))
			{
				return Direction.Equals(_other.Direction);
			}
			return false;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (this == _obj)
			{
				return true;
			}
			if (_obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((CameraPosition)_obj);
		}

		public override int GetHashCode()
		{
			return (Position.GetHashCode() * 397) ^ Direction.GetHashCode();
		}
	}

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "camera", "cam" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lock/unlock camera movement or load/save a specific camera position";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n   1. cam save <name> [comment]\n   2. cam load <name>\n   3. cam list\n   4. cam lock\n   5. cam unlock\n1. Save the current player's position and camera view or the camera position\nand view if in detached mode under the given name. Optionally a more descriptive\ncomment can be supplied.\n2. Load the position and direction with the given name. If in detached camera\nmode the camera itself will be adjusted, otherwise the player will be teleported.\n3. List the saved camera positions.\n4/5. Lock/unlock the camera rotation. Can also be achieved with the \"Lock Camera\" key.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1)
		{
			if (!_senderInfo.IsLocalGame)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");
				return;
			}
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (_params[0].EqualsCaseInsensitive("lock"))
			{
				ExecuteLock(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("unlock"))
			{
				ExecuteUnlock(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("save"))
			{
				ExecuteSave(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("load"))
			{
				ExecuteLoad(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				ExecuteList(_params);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLock(List<string> _params, EntityPlayerLocal _epl)
	{
		_epl.movementInput.bCameraPositionLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteUnlock(List<string> _params, EntityPlayerLocal _epl)
	{
		_epl.movementInput.bCameraPositionLocked = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteSave(List<string> _params, EntityPlayerLocal _epl)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command requires a name for the position.");
			return;
		}
		string text = _params[1];
		string comment = ((_params.Count > 2) ? _params[2] : null);
		Vector3 position;
		Vector3 direction;
		if (_epl.movementInput.bDetachedCameraMove)
		{
			position = _epl.cameraTransform.position - Constants.cDefaultCameraPlayerOffset;
			direction = _epl.cameraTransform.localEulerAngles;
			direction.x = 0f - direction.x;
		}
		else
		{
			position = _epl.GetPosition();
			direction = _epl.rotation;
		}
		IDictionary<string, CameraPosition> dictionary = Load();
		dictionary[text] = new CameraPosition(position, direction, comment);
		Save(dictionary);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Position saved with name \"" + text + "\"");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLoad(List<string> _params, EntityPlayerLocal _epl)
	{
		CameraPosition value;
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No position name given.");
		}
		else if (!Load().TryGetValue(_params[1], out value))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Position name not found.");
		}
		else if (_epl.movementInput.bDetachedCameraMove)
		{
			_epl.cameraTransform.position = value.Position + Constants.cDefaultCameraPlayerOffset;
			Vector3 direction = value.Direction;
			direction.x = 0f - direction.x;
			_epl.cameraTransform.localEulerAngles = direction;
		}
		else
		{
			_epl.TeleportToPosition(value.Position, _onlyIfNotFlying: false, value.Direction);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList(List<string> _params)
	{
		IDictionary<string, CameraPosition> dictionary = Load();
		string text = ((_params.Count > 1) ? _params[1] : null);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved camera positions:");
		foreach (KeyValuePair<string, CameraPosition> item in dictionary)
		{
			if (text == null || item.Key.ContainsCaseInsensitive(text) || item.Value.Comment.ContainsCaseInsensitive(text))
			{
				string text2 = (string.IsNullOrEmpty(item.Value.Comment) ? "" : (" (" + item.Value.Comment + ")"));
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + item.Key + text2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFullFilePath()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return GameIO.GetSaveGameDir() + "/camerapositions.xml";
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return GameIO.GetSaveGameLocalDir() + "/camerapositions.xml";
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IDictionary<string, CameraPosition> Load()
	{
		SortedDictionary<string, CameraPosition> sortedDictionary = new SortedDictionary<string, CameraPosition>(StringComparer.OrdinalIgnoreCase);
		string fullFilePath = GetFullFilePath();
		if (!SdFile.Exists(fullFilePath))
		{
			return sortedDictionary;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.SdLoad(fullFilePath);
		}
		catch (XmlException ex)
		{
			Log.Error("Failed loading camera file: " + ex.Message);
			return sortedDictionary;
		}
		if (xmlDocument.DocumentElement == null)
		{
			Log.Warning("Camera file has no root XML element.");
			return sortedDictionary;
		}
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element || !(childNode.Name == "position"))
			{
				continue;
			}
			XmlElement xmlElement = (XmlElement)childNode;
			if (!xmlElement.HasAttribute("name"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'name' attribute: " + xmlElement.OuterXml);
				continue;
			}
			if (!xmlElement.HasAttribute("position"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'position' attribute: " + xmlElement.OuterXml);
				continue;
			}
			if (!xmlElement.HasAttribute("direction"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'direction' attribute: " + xmlElement.OuterXml);
				continue;
			}
			string attribute = xmlElement.GetAttribute("name");
			Vector3 position = StringParsers.ParseVector3(xmlElement.GetAttribute("position"));
			Vector3 direction = StringParsers.ParseVector3(xmlElement.GetAttribute("direction"));
			string comment = null;
			if (xmlElement.HasAttribute("comment"))
			{
				comment = xmlElement.GetAttribute("comment");
			}
			sortedDictionary.Add(attribute, new CameraPosition(position, direction, comment));
		}
		return sortedDictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save(IDictionary<string, CameraPosition> _positions)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement node = xmlDocument.AddXmlElement("camerapositions");
		foreach (KeyValuePair<string, CameraPosition> _position in _positions)
		{
			XmlElement element = node.AddXmlElement("position").SetAttrib("name", _position.Key).SetAttrib("position", _position.Value.Position.ToString())
				.SetAttrib("direction", _position.Value.Direction.ToString());
			if (!string.IsNullOrEmpty(_position.Value.Comment))
			{
				element.SetAttrib("comment", _position.Value.Comment);
			}
		}
		xmlDocument.SdSave(GetFullFilePath());
	}
}
