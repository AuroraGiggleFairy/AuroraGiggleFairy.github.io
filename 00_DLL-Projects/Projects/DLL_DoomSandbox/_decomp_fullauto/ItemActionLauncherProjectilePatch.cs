using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using FullautoLauncher.Scripts.ProjectileManager;
using HarmonyLib;
using UnityEngine;
using XMLData.Item;

[HarmonyPatch]
internal class ItemActionLauncherProjectilePatch
{
	public static FieldInfo fldinfo_meta = AccessTools.Field(typeof(ItemValue), "Meta");

	public static MethodInfo mtdinfo_gbc = AccessTools.Method(typeof(ItemActionRanged), "GetBurstCount", new Type[1] { typeof(ItemActionData) }, (Type[])null);

	public static MethodInfo mtdinfo_gac = AccessTools.Method(typeof(AnimatorRangedReloadState), "GetAmmoCount", new Type[3]
	{
		typeof(EntityAlive),
		typeof(ItemValue),
		typeof(int)
	}, (Type[])null);

	public static MethodInfo mtdinfo_sta = AccessTools.Method(typeof(GameObject), "SetActive", new Type[1] { typeof(bool) }, (Type[])null);

	public static int getProjectileCount(ItemActionData _data)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		int num = 1;
		ItemInventoryData val = _data?.invData;
		if (val != null)
		{
			ItemValue itemValue = val.itemValue;
			ItemClass val2 = ((itemValue != null) ? itemValue.ItemClass : null);
			num = (int)EffectManager.GetValue((PassiveEffects)16, val.itemValue, (float)num, val.holdingEntity, (Recipe)null, (val2 != null) ? (val2.ItemTags | _data.ActionTags) : default(FastTags<Global>), true, true, true, true, true, 1, true, false);
		}
		return (num <= 0) ? 1 : num;
	}

	[HarmonyPatch(typeof(ItemActionLauncher), "StartHolding")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler_StartHolding_ItemActionLauncher(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		LocalBuilder localBuilder = generator.DeclareLocal(typeof(int));
		List<CodeInstruction> list2 = new List<CodeInstruction>
		{
			new CodeInstruction(OpCodes.Ldloc_S, (object)localBuilder),
			new CodeInstruction(OpCodes.Mul, (object)null)
		};
		for (int i = 0; i < list.Count; i++)
		{
			if (CodeInstructionExtensions.LoadsField(list[i], fldinfo_meta, false))
			{
				list.InsertRange(i + 1, list2);
				i += list2.Count;
			}
		}
		list.InsertRange(0, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[3]
		{
			new CodeInstruction(OpCodes.Ldarg_1, (object)null),
			CodeInstruction.Call(typeof(ItemActionLauncherProjectilePatch), "getProjectileCount", new Type[1] { typeof(ItemActionData) }, (Type[])null),
			new CodeInstruction(OpCodes.Stloc_S, (object)localBuilder)
		});
		return list;
	}

	[HarmonyPatch(typeof(ItemActionLauncher), "ItemActionEffects")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler_ItemActionEffects_ItemActionLauncher(IEnumerable<CodeInstruction> instructions)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (CodeInstructionExtensions.Calls(list[i], mtdinfo_gbc))
			{
				list.InsertRange(i + 1, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
				{
					new CodeInstruction(OpCodes.Ldarg_2, (object)null),
					CodeInstruction.Call(typeof(ItemActionLauncherProjectilePatch), "getProjectileCount", new Type[1] { typeof(ItemActionData) }, (Type[])null)
				});
				list.RemoveRange(i - 2, 3);
				break;
			}
		}
		return list;
	}

	[HarmonyPatch(typeof(ItemActionLauncher), "ConsumeAmmo")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler_ConsumeAmmo_ItemActionLauncher(IEnumerable<CodeInstruction> instructions)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		List<CodeInstruction> list = new List<CodeInstruction>();
		list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
		list.Add(new CodeInstruction(OpCodes.Ldarg_1, (object)null));
		list.Add(CodeInstruction.Call(typeof(ItemActionRanged), "ConsumeAmmo", new Type[1] { typeof(ItemActionData) }, (Type[])null));
		list.Add(new CodeInstruction(OpCodes.Ret, (object)null));
		return list;
	}

	[HarmonyPatch(typeof(ItemClass), "ExecuteAction")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler_ExecuteAction_ItemClass(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].opcode == OpCodes.Isinst && CodeInstructionExtensions.OperandIs(list[i], (MemberInfo)typeof(ItemActionLauncher)))
			{
				list.RemoveRange(i - 2, 4);
				break;
			}
		}
		return list;
	}

	[HarmonyPatch(typeof(AnimatorRangedReloadState), "OnStateEnter")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler_OnStateEnter_AnimatorRangedReloadState(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		LocalBuilder localBuilder = generator.DeclareLocal(typeof(int));
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (CodeInstructionExtensions.Calls(list[i], mtdinfo_gac))
			{
				list.InsertRange(i + 1, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[5]
				{
					new CodeInstruction(OpCodes.Ldloc_S, (object)6),
					CodeInstruction.Call(typeof(ItemActionLauncherProjectilePatch), "getProjectileCount", new Type[1] { typeof(ItemActionData) }, (Type[])null),
					new CodeInstruction(OpCodes.Stloc_S, (object)localBuilder),
					new CodeInstruction(OpCodes.Ldloc_S, (object)localBuilder),
					new CodeInstruction(OpCodes.Mul, (object)null)
				});
				count += 5;
				break;
			}
		}
		return list;
	}

	[HarmonyPatch(typeof(ItemActionLauncher), "instantiateProjectile")]
	[HarmonyPrefix]
	private static bool Prefix_instantiateProjectile_ItemActionLauncher(ref Vector3 _positionOffset)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_positionOffset = Vector3.zero;
		return true;
	}

	[HarmonyPatch(typeof(ItemActionLauncher), "instantiateProjectile")]
	[HarmonyPostfix]
	private static void Postfix_instantiateProjectile_ItemActionLauncher(Transform __result, ItemActionLauncher __instance)
	{
		if (((ItemAction)__instance).Properties.Contains("VisibleInMag") && !((ItemAction)__instance).Properties.GetBool("VisibleInMag"))
		{
			((Component)__result).gameObject.SetActive(false);
		}
	}

	[HarmonyPatch(typeof(GameManager), "FixedUpdate")]
	[HarmonyPostfix]
	private static void Postfix_FixedUpdate_GameManager()
	{
		CustomProjectileManager.FixedUpdate();
	}

	private static void ParseProjectileType(XElement _node)
	{
		string attribute = XmlExtensions.GetAttribute(_node, (XName)"name");
		if (string.IsNullOrEmpty(attribute))
		{
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(attribute, false);
		for (int i = 0; i < itemClass.Actions.Length; i++)
		{
			ItemAction obj = itemClass.Actions[i];
			ItemActionProjectile val = (ItemActionProjectile)(object)((obj is ItemActionProjectile) ? obj : null);
			if (val != null)
			{
				if (((ItemAction)val).Properties.Contains("CustomProjectileType"))
				{
					CustomProjectileManager.InitClass(itemClass, ((ItemAction)val).Properties.GetString("CustomProjectileType"));
				}
				else
				{
					CustomProjectileManager.InitClass(itemClass, "GameObject,FullautoLauncher");
				}
				break;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerMoveController), "Update")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler_Update_PlayerMoveController(IEnumerable<CodeInstruction> instructions)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		FieldInfo fieldInfo = AccessTools.Field(typeof(ProjectileMoveScript), "ProjectileID");
		MethodInfo methodInfo = AccessTools.Method(typeof(XUiM_PlayerInventory), "AddItem", new Type[1] { typeof(ItemStack) }, (Type[])null);
		for (int i = 0; i < list.Count; i++)
		{
			if (!CodeInstructionExtensions.StoresField(list[i], fieldInfo))
			{
				continue;
			}
			for (int num = i - 1; num >= 0; num--)
			{
				if (CodeInstructionExtensions.Calls(list[num], methodInfo))
				{
					list.RemoveRange(i + 2, 2);
					list.InsertRange(i + 2, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
					{
						new CodeInstruction(OpCodes.Ldloc_S, list[num - 1].operand),
						CodeInstruction.Call(typeof(ItemActionLauncherProjectilePatch), "DestroyOrPool", (Type[])null, (Type[])null)
					});
					break;
				}
			}
			break;
		}
		return list;
	}

	private static void DestroyOrPool(ProjectileMoveScript script, ItemStack stack)
	{
		IProjectileItemGroup projectileItemGroup = CustomProjectileManager.Get(((ItemData)stack.itemValue.ItemClass).Name);
		if (projectileItemGroup != null)
		{
			projectileItemGroup.PoolStickyTransform(((Component)script).transform);
		}
		else
		{
			Object.Destroy((Object)(object)((Component)script).gameObject);
		}
	}

	[HarmonyPatch(typeof(ItemClassesFromXml), "parseItem")]
	[HarmonyPostfix]
	private static void Postfix_parseItem_ItemClassesFromXml(XElement _node)
	{
		ParseProjectileType(_node);
	}

	[HarmonyPatch(typeof(GameManager), "SaveAndCleanupWorld")]
	[HarmonyPostfix]
	private static void Postfix_SaveAndCleanupWorld_GameManager()
	{
		CustomProjectileManager.Cleanup();
	}
}
