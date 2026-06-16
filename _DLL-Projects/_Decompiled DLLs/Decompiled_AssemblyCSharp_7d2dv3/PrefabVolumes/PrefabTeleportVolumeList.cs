using System.Collections.Generic;
using System.Text;

namespace PrefabVolumes;

public class PrefabTeleportVolumeList : PrefabVolumeListAbs<PrefabTeleportVolumeList, PrefabTeleportVolume>
{
	public override PrefabVolumeAbs.EVolumeType VolumeType => PrefabVolumeAbs.EVolumeType.Teleport;

	public override SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return SelectionBoxManager.Instance.CategoryTraderTeleport;
		}
	}

	public PrefabTeleportVolumeList(Prefab _owner)
		: base(_owner)
	{
	}

	public override void ReadFromProperties(DynamicProperties _properties)
	{
		List.Clear();
		Dictionary<string, string> values = _properties.Values;
		if (values.ContainsKey("TeleportVolumeSize") && values.ContainsKey("TeleportVolumeStart"))
		{
			List<Vector3i> list = StringParsers.ParseList(values["TeleportVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list2 = StringParsers.ParseList(values["TeleportVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			for (int num = 0; num < list2.Count; num++)
			{
				Vector3i startPos = list2[num];
				Vector3i size = ((num < list.Count) ? list[num] : Vector3i.one);
				PrefabTeleportVolume prefabTeleportVolume = new PrefabTeleportVolume();
				prefabTeleportVolume.Use(startPos, size);
				List.Add(prefabTeleportVolume);
			}
		}
	}

	public override void WriteToProperties(DynamicProperties _properties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (PrefabTeleportVolume item in List)
		{
			if (item.Used)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('#');
					stringBuilder2.Append('#');
				}
				stringBuilder.Append(item.size.ToString());
				stringBuilder2.Append(item.startPos.ToString());
			}
		}
		_properties.Values["TraderArea"] = (stringBuilder.Length > 0).ToString();
		if (stringBuilder.Length > 0)
		{
			_properties.Values["TeleportVolumeSize"] = stringBuilder.ToString();
			_properties.Values["TeleportVolumeStart"] = stringBuilder2.ToString();
		}
		else
		{
			_properties.Values.Remove("TeleportVolumeSize");
			_properties.Values.Remove("TeleportVolumeStart");
		}
	}

	public override bool CanCreateVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size)
	{
		if (Owner.bTraderArea)
		{
			return true;
		}
		XUiC_MessageBoxWindowGroup.ShowOk(LocalPlayerUI.GetUIForPrimaryPlayer().xui, Localization.Get("failed"), Localization.Get("xuiPrefabEditorTraderTeleportError"), null, _openMainMenuOnClose: false);
		return false;
	}

	public void AddExistingVolume(PrefabTeleportVolume _volume)
	{
		List.Add(_volume);
	}
}
