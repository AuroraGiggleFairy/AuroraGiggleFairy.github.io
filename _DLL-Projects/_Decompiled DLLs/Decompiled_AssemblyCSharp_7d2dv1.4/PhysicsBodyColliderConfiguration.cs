using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class PhysicsBodyColliderConfiguration
{
	public string Tag = "";

	public int CollisionLayer;

	public int RagdollLayer;

	public Vector3 CollisionScale = Vector3.one;

	public Vector3 RagdollScale = Vector3.one;

	public Vector3 CollisionOffset = Vector3.zero;

	public Vector3 RagdollOffset = Vector3.zero;

	public string Path = "";

	public EnumColliderType Type = EnumColliderType.Detail;

	public EnumColliderEnabledFlags EnabledFlags = EnumColliderEnabledFlags.All;

	public PhysicsBodyColliderConfiguration()
	{
	}

	public PhysicsBodyColliderConfiguration(PhysicsBodyColliderConfiguration otherConfig)
	{
		Tag = otherConfig.Tag;
		CollisionLayer = otherConfig.CollisionLayer;
		RagdollLayer = otherConfig.RagdollLayer;
		CollisionScale = otherConfig.CollisionScale;
		CollisionOffset = otherConfig.CollisionOffset;
		RagdollScale = otherConfig.RagdollScale;
		RagdollOffset = otherConfig.RagdollOffset;
		Path = otherConfig.Path;
		Type = otherConfig.Type;
		EnabledFlags = otherConfig.EnabledFlags;
	}

	public void Write(XmlElement _elem)
	{
		XmlElement node = _elem.AddXmlElement("collider");
		node.AddXmlKeyValueProperty("tag", Tag);
		node.AddXmlKeyValueProperty("path", Path);
		node.AddXmlKeyValueProperty("collisionLayer", CollisionLayer.ToString());
		node.AddXmlKeyValueProperty("ragdollLayer", RagdollLayer.ToString());
		node.AddXmlKeyValueProperty("collisionScale", vecToString(CollisionScale));
		node.AddXmlKeyValueProperty("ragdollScale", vecToString(RagdollScale));
		node.AddXmlKeyValueProperty("collisionOffset", vecToString(CollisionOffset));
		node.AddXmlKeyValueProperty("ragdollOffset", vecToString(RagdollOffset));
		node.AddXmlKeyValueProperty("type", Type.ToStringCached());
		string text = "";
		if ((EnabledFlags & EnumColliderEnabledFlags.Collision) != EnumColliderEnabledFlags.Disabled)
		{
			text += "collision";
		}
		if ((EnabledFlags & EnumColliderEnabledFlags.Ragdoll) != EnumColliderEnabledFlags.Disabled)
		{
			text = ((text.Length != 0) ? (text + ";ragdoll") : "ragdoll");
		}
		if (text.Length == 0)
		{
			text = "disabled";
		}
		node.AddXmlKeyValueProperty("flags", text);
	}

	public static PhysicsBodyColliderConfiguration Read(XElement _e)
	{
		PhysicsBodyColliderConfiguration physicsBodyColliderConfiguration = new PhysicsBodyColliderConfiguration();
		DynamicProperties dynamicProperties = new DynamicProperties();
		foreach (XElement item in _e.Elements("property"))
		{
			dynamicProperties.Add(item);
		}
		physicsBodyColliderConfiguration.Tag = dynamicProperties.GetStringValue("tag");
		physicsBodyColliderConfiguration.Path = dynamicProperties.GetStringValue("path");
		if (dynamicProperties.Contains("collisionLayer"))
		{
			physicsBodyColliderConfiguration.CollisionLayer = int.Parse(dynamicProperties.GetStringValue("collisionLayer"));
			physicsBodyColliderConfiguration.RagdollLayer = int.Parse(dynamicProperties.GetStringValue("ragdollLayer"));
		}
		else
		{
			physicsBodyColliderConfiguration.CollisionLayer = int.Parse(dynamicProperties.GetStringValue("layer"));
			physicsBodyColliderConfiguration.RagdollLayer = physicsBodyColliderConfiguration.CollisionLayer;
		}
		physicsBodyColliderConfiguration.CollisionScale = vecFromString(dynamicProperties.GetStringValue("collisionScale"));
		physicsBodyColliderConfiguration.RagdollScale = vecFromString(dynamicProperties.GetStringValue("ragdollScale"));
		physicsBodyColliderConfiguration.CollisionOffset = vecFromString(dynamicProperties.GetStringValue("collisionOffset"));
		physicsBodyColliderConfiguration.RagdollOffset = vecFromString(dynamicProperties.GetStringValue("ragdollOffset"));
		physicsBodyColliderConfiguration.Type = EnumUtils.Parse<EnumColliderType>(dynamicProperties.GetStringValue("type"));
		physicsBodyColliderConfiguration.EnabledFlags = EnumColliderEnabledFlags.Disabled;
		string stringValue = dynamicProperties.GetStringValue("flags");
		if (stringValue != "disabled")
		{
			string[] array = stringValue.Split(';');
			foreach (string text in array)
			{
				if (text == "collision")
				{
					physicsBodyColliderConfiguration.EnabledFlags |= EnumColliderEnabledFlags.Collision;
				}
				else if (text == "ragdoll")
				{
					physicsBodyColliderConfiguration.EnabledFlags |= EnumColliderEnabledFlags.Ragdoll;
				}
			}
		}
		if (physicsBodyColliderConfiguration.RagdollLayer == 0 || physicsBodyColliderConfiguration.RagdollLayer == 27)
		{
			physicsBodyColliderConfiguration.RagdollLayer = 21;
		}
		return physicsBodyColliderConfiguration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string vecToString(Vector3 vec)
	{
		return vec.x.ToCultureInvariantString() + " " + vec.y.ToCultureInvariantString() + " " + vec.z.ToCultureInvariantString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 vecFromString(string str)
	{
		string[] array = str.Split(' ');
		if (array.Length != 3)
		{
			if (array.Length < 1)
			{
				throw new FormatException("Vector3 expected");
			}
			return new Vector3(StringParsers.ParseFloat(array[0]), StringParsers.ParseFloat(array[0]), StringParsers.ParseFloat(array[0]));
		}
		return new Vector3(StringParsers.ParseFloat(array[0]), StringParsers.ParseFloat(array[1]), StringParsers.ParseFloat(array[2]));
	}
}
