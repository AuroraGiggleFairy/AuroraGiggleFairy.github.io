using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class CameraPerspectives
{
	public class Perspective : IEquatable<Perspective>
	{
		public readonly string Name;

		public readonly Vector3 Position;

		public readonly Vector3 Direction;

		public readonly string Comment;

		public Perspective(string _name, Vector3 _position, Vector3 _direction, string _comment)
		{
			Name = _name;
			Position = _position;
			Direction = _direction;
			Comment = _comment;
		}

		public Perspective(string _name, EntityPlayerLocal _player, string _comment = null)
		{
			Name = _name;
			if (_player.movementInput.bDetachedCameraMove)
			{
				Position = _player.cameraTransform.position - Constants.cDefaultCameraPlayerOffset;
				Direction = _player.cameraTransform.localEulerAngles;
				Direction.x = 0f - Direction.x;
			}
			else
			{
				Position = _player.GetPosition();
				Direction = _player.rotation;
			}
			Comment = _comment;
		}

		public void ToPlayer(EntityPlayerLocal _player)
		{
			if (_player.movementInput.bDetachedCameraMove)
			{
				_player.cameraTransform.position = Position + Constants.cDefaultCameraPlayerOffset;
				Vector3 direction = Direction;
				direction.x = 0f - direction.x;
				_player.cameraTransform.localEulerAngles = direction;
			}
			else
			{
				_player.TeleportToPosition(Position, _onlyIfNotFlying: false, Direction);
			}
		}

		public bool Equals(Perspective _other)
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
			return Equals((Perspective)_obj);
		}

		public override int GetHashCode()
		{
			return (Position.GetHashCode() * 397) ^ Direction.GetHashCode();
		}

		public void ToXml(XmlElement _parent)
		{
			XmlElement element = _parent.AddXmlElement("position").SetAttrib("name", Name).SetAttrib("position", Position.ToString())
				.SetAttrib("direction", Direction.ToString());
			if (!string.IsNullOrEmpty(Comment))
			{
				element.SetAttrib("comment", Comment);
			}
		}

		public static Perspective FromXml(XmlElement _lineItem)
		{
			if (!_lineItem.HasAttribute("name"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'name' attribute: " + _lineItem.OuterXml);
				return null;
			}
			if (!_lineItem.HasAttribute("position"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'position' attribute: " + _lineItem.OuterXml);
				return null;
			}
			if (!_lineItem.HasAttribute("direction"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'direction' attribute: " + _lineItem.OuterXml);
				return null;
			}
			string attribute = _lineItem.GetAttribute("name");
			Vector3 position = StringParsers.ParseVector3(_lineItem.GetAttribute("position"));
			Vector3 direction = StringParsers.ParseVector3(_lineItem.GetAttribute("direction"));
			string comment = null;
			if (_lineItem.HasAttribute("comment"))
			{
				comment = _lineItem.GetAttribute("comment");
			}
			return new Perspective(attribute, position, direction, comment);
		}
	}

	public readonly SortedDictionary<string, Perspective> Perspectives = new SortedDictionary<string, Perspective>(StringComparer.OrdinalIgnoreCase);

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetFullFilePath()
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

	public CameraPerspectives(bool _load = true)
	{
		if (_load)
		{
			Load();
		}
	}

	public bool Load()
	{
		Perspectives.Clear();
		string fullFilePath = GetFullFilePath();
		if (!SdFile.Exists(fullFilePath))
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.SdLoad(fullFilePath);
		}
		catch (XmlException ex)
		{
			Log.Error("Failed loading camera file: " + ex.Message);
			return false;
		}
		if (xmlDocument.DocumentElement == null)
		{
			Log.Warning("Camera file has no root XML element.");
			return false;
		}
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Element && !(childNode.Name != "position"))
			{
				Perspective perspective = Perspective.FromXml((XmlElement)childNode);
				if (perspective != null && !Perspectives.TryAdd(perspective.Name, perspective))
				{
					Log.Warning("Duplicate camera perspective entry '" + perspective.Name + "'");
				}
			}
		}
		return true;
	}

	public void Save()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement parent = xmlDocument.AddXmlElement("camerapositions");
		foreach (KeyValuePair<string, Perspective> perspective in Perspectives)
		{
			perspective.Value.ToXml(parent);
		}
		xmlDocument.SdSave(GetFullFilePath());
	}
}
