using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

public class PhysicsBodyLayout
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, PhysicsBodyLayout> bodyLayouts = new Dictionary<string, PhysicsBodyLayout>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PhysicsBodyColliderConfiguration> colliders = new List<PhysicsBodyColliderConfiguration>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	public string Name => name;

	public List<PhysicsBodyColliderConfiguration> Colliders => colliders;

	public static PhysicsBodyLayout[] BodyLayouts
	{
		get
		{
			PhysicsBodyLayout[] array = new PhysicsBodyLayout[bodyLayouts.Count];
			bodyLayouts.CopyValuesTo(array);
			return array;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PhysicsBodyLayout New(string _name)
	{
		if (bodyLayouts.ContainsKey(_name))
		{
			throw new Exception("duplicate physics body!");
		}
		PhysicsBodyLayout physicsBodyLayout = new PhysicsBodyLayout();
		physicsBodyLayout.name = _name;
		bodyLayouts[_name] = physicsBodyLayout;
		return physicsBodyLayout;
	}

	public static PhysicsBodyLayout New()
	{
		int num = 0;
		string key;
		while (true)
		{
			key = $"unnamed{num}";
			if (!bodyLayouts.ContainsKey(key))
			{
				break;
			}
			num++;
		}
		return New(key);
	}

	public bool Rename(string newName)
	{
		if (bodyLayouts.ContainsKey(newName))
		{
			return false;
		}
		bodyLayouts.Remove(name);
		bodyLayouts[newName] = this;
		name = newName;
		return true;
	}

	public static bool Remove(string _name)
	{
		return bodyLayouts.Remove(_name);
	}

	public static PhysicsBodyLayout Find(string _name)
	{
		PhysicsBodyLayout value = null;
		bodyLayouts.TryGetValue(_name, out value);
		return value;
	}

	public static void Reset()
	{
		bodyLayouts.Clear();
	}

	public static PhysicsBodyLayout Read(XElement _e)
	{
		if (!_e.HasAttribute("name"))
		{
			throw new Exception("Physics body needs a name");
		}
		PhysicsBodyLayout physicsBodyLayout = New(_e.GetAttribute("name"));
		foreach (XElement item in _e.Elements("collider"))
		{
			physicsBodyLayout.colliders.Add(PhysicsBodyColliderConfiguration.Read(item));
		}
		return physicsBodyLayout;
	}

	public void Write(XmlElement _elem)
	{
		XmlElement elem = _elem.AddXmlElement("body").SetAttrib("name", name);
		for (int i = 0; i < colliders.Count; i++)
		{
			colliders[i].Write(elem);
		}
	}
}
