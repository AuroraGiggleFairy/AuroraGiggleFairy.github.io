using System;
using System.Reflection;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// 3.2 stores these as fields. 3.3 exposes them as properties.
	/// A direct read compiles to only one of those and breaks the other version.
	/// </summary>
	internal static class ItemInventoryAccess
	{
		internal static EntityAlive Holding(ItemInventoryData data)
		{
			if (data == null)
			{
				return null;
			}

			PropertyInfo property = typeof(ItemInventoryData).GetProperty("holdingEntity");
			if (property != null)
			{
				return (EntityAlive)property.GetValue(data, null);
			}

			FieldInfo field = typeof(ItemInventoryData).GetField("holdingEntity");
			return field != null ? (EntityAlive)field.GetValue(data) : null;
		}

		internal static int MetaOf(ItemValue value)
		{
			if (value == null)
			{
				return 0;
			}

			PropertyInfo property = typeof(ItemValue).GetProperty("Meta");
			if (property != null)
			{
				return Convert.ToInt32(property.GetValue(value, null));
			}

			FieldInfo field = typeof(ItemValue).GetField("Meta") ?? typeof(ItemValue).GetField("meta");
			return field != null ? Convert.ToInt32(field.GetValue(value)) : 0;
		}

		internal static void SetMeta(ItemValue value, int meta)
		{
			if (value == null)
			{
				return;
			}

			PropertyInfo property = typeof(ItemValue).GetProperty("Meta");
			if (property != null)
			{
				property.SetValue(value, meta, null);
				return;
			}

			FieldInfo field = typeof(ItemValue).GetField("Meta") ?? typeof(ItemValue).GetField("meta");
			if (field != null)
			{
				field.SetValue(value, meta);
			}
		}

		internal static int AmmoIndex(ItemValue value)
		{
			if (value == null)
			{
				return 0;
			}

			PropertyInfo property = typeof(ItemValue).GetProperty("SelectedAmmoTypeIndex");
			if (property != null)
			{
				return Convert.ToInt32(property.GetValue(value, null));
			}

			FieldInfo field = typeof(ItemValue).GetField("SelectedAmmoTypeIndex");
			if (field == null)
			{
				field = typeof(ItemValue).GetField("selectedAmmoTypeIndex");
			}

			return field != null ? Convert.ToInt32(field.GetValue(value)) : 0;
		}

		internal static void SetAmmoIndex(ItemValue value, int index)
		{
			if (value == null)
			{
				return;
			}

			byte ammo = (byte)index;
			PropertyInfo property = typeof(ItemValue).GetProperty("SelectedAmmoTypeIndex");
			if (property != null)
			{
				property.SetValue(value, ammo, null);
				return;
			}

			FieldInfo field = typeof(ItemValue).GetField("SelectedAmmoTypeIndex")
				?? typeof(ItemValue).GetField("selectedAmmoTypeIndex");
			if (field != null)
			{
				field.SetValue(value, ammo);
			}
		}
	}
}
