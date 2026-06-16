using System;
using System.Collections.Generic;
using System.Xml.Linq;

public static class SignLibraryMigrations
{
	public delegate XElement MigrationFunc(XElement root);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<int, MigrationFunc> Migrations = new Dictionary<int, MigrationFunc>
	{
		{ 0, MigrateV0ToV1 },
		{ 1, MigrateV1ToV2 }
	};

	public static XElement Migrate(XElement root, int fromVersion, int toVersion)
	{
		if (toVersion != fromVersion + 1)
		{
			throw new ArgumentException($"Migrations must be sequential. Cannot migrate from v{fromVersion} to v{toVersion}");
		}
		if (!Migrations.TryGetValue(fromVersion, out var value))
		{
			throw new InvalidOperationException($"No migration registered for v{fromVersion} to v{toVersion}");
		}
		Log.Out($"  Applying migration: v{fromVersion} -> v{toVersion}");
		XElement xElement = value(root);
		xElement.SetAttributeValue("version", toVersion);
		return xElement;
	}

	public static bool CanMigrate(int fromVersion, int toVersion)
	{
		for (int i = fromVersion; i < toVersion; i++)
		{
			if (!Migrations.ContainsKey(i))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement VisitLayers(XElement root, Action<XElement, string> visitor)
	{
		foreach (XElement item in root.Elements(XNames.sign))
		{
			ProcessLayers(item.Elements("layer"), visitor);
		}
		return root;
		[PublicizedFrom(EAccessModifier.Internal)]
		static void ProcessLayers(IEnumerable<XElement> layers, Action<XElement, string> action)
		{
			foreach (XElement layer in layers)
			{
				string attribute = layer.GetAttribute("type");
				action(layer, attribute);
				if (attribute == "GroupSignLayer")
				{
					ProcessLayers(layer.Elements("layer"), action);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement MigrateV0ToV1(XElement root)
	{
		return root;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement MigrateV1ToV2(XElement root)
	{
		return VisitLayers(root, [PublicizedFrom(EAccessModifier.Internal)] (XElement layer, string layerType) =>
		{
			if (layerType == "TextSignLayer")
			{
				layer.SetAttributeValue("direction", 0);
				layer.SetAttributeValue("spacing", 1);
			}
		});
	}
}
