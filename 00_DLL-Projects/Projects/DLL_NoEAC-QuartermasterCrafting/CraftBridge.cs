using System;
using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace QuartermasterCrafting
{
	internal static class CraftBridge
	{
		private const float CacheSeconds = 0.4f;
		private const float PoolSeconds = 1f;
		private const float ResortSeconds = 2f;

		private static readonly List<Line> Cache = new List<Line>();
		private static readonly Dictionary<int, int> Nearby = new Dictionary<int, int>();
		private static readonly Dictionary<int, int> NearbyScratch = new Dictionary<int, int>();
		private static string poolSig = "";
		private static string poolRequested = "";
		private static float poolUntil;
		private static float poolSendUntil;
		private static int poolRequest;
		private static int waitingPool;
		private static bool poolKnown;
		private static bool listStale;
		private static float nextResort;
		private static string cacheSig = "";
		private static string requestedSig = "";
		private static float cacheUntil;
		private static float nextSend;
		private static int queryId;
		private static int waitingQuery;
		private static bool busy;
		private static bool hasClick;
		private static XUi lastXui;
		private static Click pending;

		public static bool TryReady(XUi xui, Recipe recipe, int count, out List<Line> lines)
		{
			lines = Cache;
			if (xui == null || RecipeLines.LeaveVanilla(recipe))
			{
				return false;
			}

			lastXui = xui;
			Vector3i origin;
			bool station;
			Resolve(xui, out origin, out station);
			string sig = Sig(recipe, count, origin, station);
			bool ready = cacheSig == sig && Cache.Count > 0;
			if (!ready || Time.unscaledTime > cacheUntil)
			{
				Ensure(xui, recipe, count, origin, station, sig);
			}

			return cacheSig == sig && Cache.Count > 0;
		}

		public static bool HandleClick(ItemActionEntryCraft action, XUiC_RecipeCraftCount counter, int tier)
		{
			XUiController controller = action.ItemController;
			XUi xui = controller?.xui;
			XUiC_RecipeEntry entry = controller as XUiC_RecipeEntry;
			Recipe recipe = entry?.Recipe;
			if (xui == null || recipe == null || RecipeLines.LeaveVanilla(recipe))
			{
				return true;
			}

			int count = counter != null ? counter.Count : 1;
			if (count < 1)
			{
				count = 1;
			}

			Vector3i origin;
			bool station;
			Resolve(xui, out origin, out station);
			if (VanillaReady(xui, recipe, count, station))
			{
				return true;
			}

			if (!CoversFromPool(xui, recipe, count))
			{
				return true;
			}

			if (busy)
			{
				return false;
			}

			XUiC_CraftingWindowGroup window = Showing(xui);
			if (window == null)
			{
				return true;
			}

			if (QueueFull(window))
			{
				WarnQueueFull(xui);
				return false;
			}

			pending = new Click
			{
				Xui = xui,
				Window = window,
				Recipe = recipe,
				Tier = tier,
				Count = count
			};
			hasClick = true;

			if (Storage.IsServer())
			{
				var lines = new List<Line>();
				int token;
				byte code = ServerPay.Pay(xui.playerUI.entityPlayer, recipe.GetName(), recipe.craftingArea, count, origin, station, lines, out token);
				Finish(code, token, lines);
				return false;
			}

			busy = true;
			NetPackageQuartermasterRequest.Send(RequestKind.Pay, 0, recipe, count, origin, station, 0);
			return false;
		}

		public static bool RefreshPool(XUi xui)
		{
			if (xui?.playerUI?.entityPlayer == null)
			{
				return false;
			}

			if (poolKnown && Time.unscaledTime <= poolUntil)
			{
				return false;
			}

			if (Showing(xui) == null)
			{
				return false;
			}

			Resolve(xui, out Vector3i origin, out bool station);
			if (!station)
			{
				origin = Storage.PlayerOrigin(xui.playerUI.entityPlayer);
			}

			if (Storage.IsServer())
			{
				Storage.CollectClosed(xui.playerUI.entityPlayer, origin, NearbyScratch);
				bool changed = ApplyPool("");
				if (changed)
				{
					listStale = true;
					Dirty();
				}

				poolUntil = Time.unscaledTime + PoolSeconds;
				return changed;
			}

			string sig = origin.x + "|" + origin.y + "|" + origin.z + "|" + station;
			if (poolRequested == sig && Time.unscaledTime < poolSendUntil)
			{
				return false;
			}

			poolRequest++;
			if (poolRequest == 0)
			{
				poolRequest++;
			}

			waitingPool = poolRequest;
			poolRequested = sig;
			poolSendUntil = Time.unscaledTime + PoolSeconds;
			NetPackageQuartermasterRequest.Send(RequestKind.Pool, poolRequest, null, 0, origin, station, 0);
			return false;
		}

		public static bool TakeListResort()
		{
			if (!listStale || Time.unscaledTime < nextResort)
			{
				return false;
			}

			listStale = false;
			nextResort = Time.unscaledTime + ResortSeconds;
			return true;
		}

		public static int ShownCraftCount(XUiController root)
		{
			int frame = Time.frameCount;
			if (countRoot != root || countFrame != frame)
			{
				countFrame = frame;
				countRoot = root;
				countControl = root?.GetChildByType<XUiC_RecipeCraftCount>();
			}

			int count = countControl != null ? countControl.Count : 1;
			return count < 1 ? 1 : count;
		}

		private static int countFrame = -1;
		private static XUiController countRoot;
		private static XUiC_RecipeCraftCount countControl;

		private static bool VanillaReady(XUi xui, Recipe recipe, int count, bool station)
		{
			EntityPlayer player = xui?.playerUI?.entityPlayer;
			if (player == null || recipe?.ingredients == null)
			{
				return false;
			}

			TileEntityWorkstation bench = null;
			if (station)
			{
				Resolve(xui, out Vector3i origin, out _);
				bench = GameManager.Instance?.World?.GetTileEntity(origin) as TileEntityWorkstation;
			}

			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				if (StackAccess.Value(ingredient) == null || StackAccess.TypeId(StackAccess.Value(ingredient)) == 0)
				{
					continue;
				}

				int need = RecipeLines.PerCraft(recipe, ingredient, player) * count;
				if (need < 1)
				{
					continue;
				}

				int have = station ? Storage.CountGrid(bench, StackAccess.Value(ingredient)) : Storage.CountInv(player, StackAccess.Value(ingredient));
				if (have < need)
				{
					return false;
				}
			}

			return true;
		}

		public static bool TryNearby(XUi xui, out Dictionary<int, int> counts)
		{
			counts = Nearby;
			return poolKnown && xui != null && Showing(xui) != null;
		}

		public static bool ReadHeld(XUi xui, Recipe recipe, ItemStack ingredient, int craftCount, out int held, out int total, out int need)
		{
			held = 0;
			total = 0;
			need = 0;
			if (xui?.playerUI?.entityPlayer == null || recipe == null || StackAccess.Value(ingredient) == null)
			{
				return false;
			}

			EntityPlayer player = xui.playerUI.entityPlayer;
			held = Storage.CountInv(player, StackAccess.Value(ingredient));
			Resolve(xui, out Vector3i origin, out bool station);
			if (station)
			{
				TileEntityWorkstation bench = GameManager.Instance?.World?.GetTileEntity(origin) as TileEntityWorkstation;
				held += Storage.CountGrid(bench, StackAccess.Value(ingredient));
			}

			int nearby = 0;
			if (TryNearby(xui, out Dictionary<int, int> pool))
			{
				pool.TryGetValue(StackAccess.TypeId(StackAccess.Value(ingredient)), out nearby);
			}

			if (craftCount < 1)
			{
				craftCount = 1;
			}

			int per = RecipeLines.PerCraft(recipe, ingredient, player);
			need = per > 0 ? per * craftCount : 0;
			total = held + nearby;
			return true;
		}

		public static bool CoversFromPool(XUi xui, Recipe recipe, int craftCount)
		{
			if (xui?.playerUI?.entityPlayer == null || RecipeLines.LeaveVanilla(recipe) || recipe?.ingredients == null)
			{
				return false;
			}

			if (!TryNearby(xui, out Dictionary<int, int> nearby))
			{
				return false;
			}

			if (craftCount < 1)
			{
				craftCount = 1;
			}

			EntityPlayer player = xui.playerUI.entityPlayer;
			Resolve(xui, out Vector3i origin, out bool station);
			TileEntityWorkstation bench = station ? GameManager.Instance?.World?.GetTileEntity(origin) as TileEntityWorkstation : null;
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack ingredient = recipe.ingredients[i];
				if (StackAccess.Value(ingredient) == null || StackAccess.TypeId(StackAccess.Value(ingredient)) == 0)
				{
					continue;
				}

				int per = RecipeLines.PerCraft(recipe, ingredient, player);
				int required = per * craftCount;
				if (required < 1)
				{
					continue;
				}

				int have = Storage.CountInv(player, StackAccess.Value(ingredient)) + Storage.CountGrid(bench, StackAccess.Value(ingredient));
				if (nearby.TryGetValue(StackAccess.TypeId(StackAccess.Value(ingredient)), out int extra))
				{
					have += extra;
				}

				if (have < required)
				{
					return false;
				}
			}

			return true;
		}

		public static void OnPool(int requestId, List<Line> lines)
		{
			if (requestId != waitingPool)
			{
				return;
			}

			NearbyScratch.Clear();
			if (lines != null)
			{
				for (int i = 0; i < lines.Count; i++)
				{
					if (lines[i].Type == 0 || lines[i].Nearby <= 0)
					{
						continue;
					}

					NearbyScratch[lines[i].Type] = lines[i].Nearby;
				}
			}

			if (ApplyPool(poolRequested))
			{
				listStale = true;
				Dirty();
			}

			poolUntil = Time.unscaledTime + PoolSeconds;
		}

		private static bool ApplyPool(string sig)
		{
			bool changed = !poolKnown || !SamePool();
			if (!changed)
			{
				return false;
			}

			Nearby.Clear();
			foreach (KeyValuePair<int, int> pair in NearbyScratch)
			{
				Nearby[pair.Key] = pair.Value;
			}

			poolSig = sig;
			poolKnown = true;
			return true;
		}

		private static bool SamePool()
		{
			if (Nearby.Count != NearbyScratch.Count)
			{
				return false;
			}

			foreach (KeyValuePair<int, int> pair in NearbyScratch)
			{
				if (!Nearby.TryGetValue(pair.Key, out int have) || have != pair.Value)
				{
					return false;
				}
			}

			return true;
		}

		public static void OnQuery(int requestId, List<Line> lines)
		{
			if (requestId != waitingQuery)
			{
				return;
			}

			Cache.Clear();
			Cache.AddRange(lines);
			cacheSig = requestedSig;
			cacheUntil = Time.unscaledTime + CacheSeconds;
			Dirty();
		}

		public static void OnPay(byte code, int token, List<Line> lines)
		{
			busy = false;
			Finish(code, token, lines);
		}

		public static Recipe RecipeFor(XUiC_IngredientEntry entry)
		{
			if (entry == null)
			{
				return null;
			}

			if (entry.Recipe != null)
			{
				return entry.Recipe;
			}

			XUiController cursor = entry.Parent;
			while (cursor != null)
			{
				XUiC_IngredientList list = cursor as XUiC_IngredientList;
				if (list != null && list.Recipe != null)
				{
					return list.Recipe;
				}

				cursor = cursor.Parent;
			}

			return null;
		}

		public static int IndexOf(XUiC_IngredientEntry entry)
		{
			XUiC_IngredientList list = null;
			XUiController cursor = entry?.Parent;
			while (cursor != null && list == null)
			{
				list = cursor as XUiC_IngredientList;
				cursor = cursor.Parent;
			}

			if (list != null)
			{
				XUiC_IngredientEntry[] rows = list.GetChildrenByType<XUiC_IngredientEntry>();
				for (int i = 0; i < rows.Length; i++)
				{
					if (rows[i] == entry)
					{
						return i;
					}
				}
			}

			return -1;
		}

		public static void Resolve(XUi xui, out Vector3i origin, out bool station)
		{
			station = false;
			EntityPlayerLocal player = xui.playerUI.entityPlayer;
			origin = new Vector3i(player.position);
			XUiC_CraftingWindowGroup window = Showing(xui);
			XUiC_WorkstationWindowGroup bench = window as XUiC_WorkstationWindowGroup;
			if (bench?.WorkstationData?.TileEntity == null)
			{
				return;
			}

			origin = bench.WorkstationData.TileEntity.ToWorldPos();
			station = true;
		}

		private static void Ensure(XUi xui, Recipe recipe, int count, Vector3i origin, bool station, string sig)
		{
			if (cacheSig == sig && Time.unscaledTime <= cacheUntil)
			{
				return;
			}

			if (Storage.IsServer())
			{
				ServerPay.Query(xui.playerUI.entityPlayer, recipe.GetName(), recipe.craftingArea, count, origin, station, Cache);
				cacheSig = sig;
				cacheUntil = Time.unscaledTime + CacheSeconds;
				return;
			}

			if (requestedSig == sig && Time.unscaledTime < nextSend)
			{
				return;
			}

			queryId++;
			if (queryId == 0)
			{
				queryId++;
			}

			waitingQuery = queryId;
			requestedSig = sig;
			nextSend = Time.unscaledTime + CacheSeconds;
			NetPackageQuartermasterRequest.Send(RequestKind.Query, queryId, recipe, count, origin, station, 0);
		}

		private static void Finish(byte code, int token, List<Line> lines)
		{
			if (!hasClick)
			{
				busy = false;
				return;
			}

			Click click = pending;
			hasClick = false;
			busy = false;
			if (click.Xui == null)
			{
				if (token != 0)
				{
					Refund(click, token);
				}

				return;
			}

			cacheUntil = 0f;
			poolUntil = 0f;
			if (code == PayCode.Open)
			{
				Tooltip(click.Xui, "agfQmStorageOpen", "Can't craft that yet. Those materials are in storage someone has open.");
				return;
			}

			if (code == PayCode.Missing)
			{
				Tooltip(click.Xui, "agfQmStorageGone", "Can't craft that. Those materials are no longer in storage.");
				return;
			}

			if (code == PayCode.Bad)
			{
				return;
			}

			if (code != PayCode.Paid && code != PayCode.NoNeed)
			{
				if (token != 0)
				{
					Refund(click, token);
				}

				return;
			}

			if (!SpendLocal(click, lines))
			{
				Tooltip(click.Xui, "agfQmStorageGone", "Can't craft that. Those materials are no longer in storage.");
				Refund(click, token);
				return;
			}

			if (!Queue(click, lines))
			{
				GiveBackLocal(click, lines);
				Refund(click, token);
				WarnQueueFull(click.Xui);
				return;
			}

			if (token != 0)
			{
				Ack(click, token);
			}

			Recipe recipe = click.Recipe;
			if (recipe == click.Xui.Recipes.TrackedRecipe)
			{
				click.Xui.Recipes.TrackedRecipe = null;
				click.Xui.Recipes.ResetToPreviousTracked(click.Xui.playerUI.entityPlayer);
			}

			Dirty(click.Window);
		}

		private static bool SpendLocal(Click click, List<Line> lines)
		{
			Recipe recipe = click.Recipe;
			if (recipe?.ingredients == null || lines == null || lines.Count != recipe.ingredients.Count)
			{
				return false;
			}

			var inv = new List<ItemStack>();
			XUiC_WorkstationInputGrid grid = click.Window.GetChildByType<XUiC_WorkstationInputGrid>();
			for (int i = 0; i < lines.Count; i++)
			{
				ItemValue item = StackAccess.Value(recipe.ingredients[i]);
				if (lines[i].Inv > 0)
				{
					if (click.Xui.PlayerInventory.GetItemCount(item) < lines[i].Inv)
					{
						return false;
					}

					inv.Add(new ItemStack(item.Clone(), lines[i].Inv));
				}

				if (lines[i].Grid > 0 && (grid == null || grid.GetItemCount(item) < lines[i].Grid))
				{
					return false;
				}
			}

			if (inv.Count > 0)
			{
				click.Xui.PlayerInventory.RemoveItems(inv, 1, null);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].Grid > 0)
				{
					grid.DecItem(StackAccess.Value(recipe.ingredients[i]), lines[i].Grid, null);
				}
			}

			return true;
		}

		private static void GiveBackLocal(Click click, List<Line> lines)
		{
			Recipe recipe = click.Recipe;
			if (recipe?.ingredients == null || lines == null)
			{
				return;
			}

			var inv = new List<ItemStack>();
			XUiC_WorkstationInputGrid grid = click.Window.GetChildByType<XUiC_WorkstationInputGrid>();
			for (int i = 0; i < lines.Count && i < recipe.ingredients.Count; i++)
			{
				ItemValue item = StackAccess.Value(recipe.ingredients[i]).Clone();
				if (lines[i].Inv > 0)
				{
					inv.Add(new ItemStack(item, lines[i].Inv));
				}

				if (lines[i].Grid > 0 && grid != null)
				{
					grid.AddToItemStackArray(new ItemStack(item.Clone(), lines[i].Grid));
				}
			}

			if (inv.Count > 0)
			{
				click.Xui.PlayerInventory.AddItems(inv.ToArray());
			}
		}

		private static bool Queue(Click click, List<Line> lines)
		{
			Recipe source = click.Recipe;
			XUi xui = click.Xui;
			var recipe = new Recipe
			{
				itemValueType = source.itemValueType,
				count = XUiM_Recipes.GetRecipeCraftOutputCount(xui, source),
				craftingArea = source.craftingArea,
				craftExpGain = source.craftExpGain,
				craftingTime = XUiM_Recipes.GetRecipeCraftTime(xui, source),
				craftingToolType = source.craftingToolType,
				craftingTier = click.Tier,
				tags = source.tags
			};
			var ingredients = new List<ItemStack>();
			int count = click.Count < 1 ? 1 : click.Count;
			for (int i = 0; i < source.ingredients.Count && i < lines.Count; i++)
			{
				int per = lines[i].Need / count;
				ingredients.Add(new ItemStack(StackAccess.Value(source.ingredients[i]).Clone(), per));
			}

			recipe.AddIngredients(ingredients);
			if (click.Window is XUiC_WorkstationWindowGroup bench)
			{
				bench.GetChildByType<XUiC_WorkstationFuelGrid>()?.TurnOn();
			}

			return click.Window.AddItemToQueue(recipe, click.Count);
		}

		private static void Ack(Click click, int token)
		{
			if (Storage.IsServer())
			{
				ServerPay.Ack(click.Xui.playerUI.entityPlayer.entityId, token);
				return;
			}

			NetPackageQuartermasterRequest.Send(RequestKind.Ack, 0, click.Recipe, click.Count, Vector3i.zero, false, token);
		}

		private static void Refund(Click click, int token)
		{
			if (token == 0)
			{
				return;
			}

			if (Storage.IsServer())
			{
				int entityId = click.Xui != null ? click.Xui.playerUI.entityPlayer.entityId : -1;
				if (entityId >= 0)
				{
					ServerPay.Refund(entityId, token);
				}

				return;
			}

			NetPackageQuartermasterRequest.Send(RequestKind.Refund, 0, click.Recipe, click.Count, Vector3i.zero, false, token);
		}

		private static int shownFrame = -1;
		private static XUi shownXui;
		private static XUiC_CraftingWindowGroup shownGroup;

		private static XUiC_CraftingWindowGroup Showing(XUi xui)
		{
			int frame = Time.frameCount;
			if (frame == shownFrame && shownXui == xui)
			{
				return shownGroup;
			}

			shownFrame = frame;
			shownXui = xui;
			shownGroup = null;
			if (xui == null)
			{
				return null;
			}

			List<XUiC_CraftingWindowGroup> groups = xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i].WindowGroup != null && groups[i].WindowGroup.isShowing)
				{
					shownGroup = groups[i];
					return shownGroup;
				}
			}

			return null;
		}

		private static bool QueueFull(XUiC_CraftingWindowGroup window)
		{
			if (window?.craftingQueue == null)
			{
				return false;
			}

			XUiC_RecipeStack[] slots = window.craftingQueue.GetRecipesToCraft();
			if (slots == null || slots.Length == 0)
			{
				return false;
			}

			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i] == null || slots[i].GetRecipe() == null)
				{
					return false;
				}
			}

			return true;
		}

		private static void Tooltip(XUi xui, string key, string fallback)
		{
			string text = Localization.Get(key);
			if (string.IsNullOrEmpty(text) || text == key)
			{
				text = fallback;
			}

			GameManager.ShowTooltip(xui.playerUI.entityPlayer, text);
			Manager.PlayInsidePlayerHead("ui_denied");
		}

		private static void WarnQueueFull(XUi xui)
		{
			GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("xuiCraftQueueFull"));
			Manager.PlayInsidePlayerHead("ui_denied");
		}

		private static void Dirty(XUiC_CraftingWindowGroup window = null)
		{
			if (window != null)
			{
				window.SetAllChildrenDirty();
				return;
			}

			XUi xui = pending.Xui ?? lastXui;
			if (xui == null)
			{
				return;
			}

			Showing(xui)?.SetAllChildrenDirty();
		}

		private static string Sig(Recipe recipe, int count, Vector3i origin, bool station)
		{
			return recipe.GetName() + "|" + (recipe.craftingArea ?? "") + "|" + count + "|" + origin.x + "|" + origin.y + "|" + origin.z + "|" + station;
		}

		private struct Click
		{
			public XUi Xui;
			public XUiC_CraftingWindowGroup Window;
			public Recipe Recipe;
			public int Tier;
			public int Count;
		}
	}

	internal static class RequestKind
	{
		public const byte Query = 1;
		public const byte Pay = 2;
		public const byte Ack = 3;
		public const byte Refund = 4;
		public const byte Pool = 5;
	}

	[Preserve]
	public abstract class NetPackageQuartermasterRequest : NetPackage
	{
		private byte kind;
		private int requestId;
		private string name = "";
		private string area = "";
		private short count;
		private int x;
		private int y;
		private int z;
		private bool station;
		private int token;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		public static void Send(byte kind, int requestId, Recipe recipe, int count, Vector3i origin, bool station, int token)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null)
			{
				return;
			}

			manager.SendToServer(PackageEmit.Take<NetPackageQuartermasterRequest>().Setup(
				kind,
				requestId,
				recipe != null ? recipe.GetName() : "",
				recipe != null ? recipe.craftingArea ?? "" : "",
				count,
				origin,
				station,
				token));
		}

		public NetPackageQuartermasterRequest Setup(byte kind, int requestId, string name, string area, int count, Vector3i origin, bool station, int token)
		{
			this.kind = kind;
			this.requestId = requestId;
			this.name = name ?? "";
			this.area = area ?? "";
			this.count = (short)(count > short.MaxValue ? short.MaxValue : count);
			x = origin.x;
			y = origin.y;
			z = origin.z;
			this.station = station;
			this.token = token;
			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			kind = reader.ReadByte();
			requestId = reader.ReadInt32();
			name = reader.ReadString();
			area = reader.ReadString();
			count = reader.ReadInt16();
			x = reader.ReadInt32();
			y = reader.ReadInt32();
			z = reader.ReadInt32();
			station = reader.ReadBoolean();
			token = reader.ReadInt32();
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			BinaryWriter raw = writer;
			raw.Write(kind);
			raw.Write(requestId);
			raw.Write(name ?? "");
			raw.Write(area ?? "");
			raw.Write(count);
			raw.Write(x);
			raw.Write(y);
			raw.Write(z);
			raw.Write(station);
			raw.Write(token);
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			if (world == null || Sender == null)
			{
				return;
			}

			EntityPlayer player = world.GetEntity(Sender.entityId) as EntityPlayer;
			if (player == null)
			{
				return;
			}

			Vector3i origin = new Vector3i(x, y, z);
			if (kind == RequestKind.Ack)
			{
				ServerPay.Ack(player.entityId, token);
				return;
			}

			if (kind == RequestKind.Refund)
			{
				ServerPay.Refund(player.entityId, token);
				return;
			}

			var lines = new List<Line>();
			if (kind == RequestKind.Pool)
			{
				ServerPay.Pool(player, origin, station, lines);
				Reply(player.entityId, RequestKind.Pool, 0, requestId, 0, lines);
				return;
			}
			if (kind == RequestKind.Query)
			{
				ServerPay.Query(player, name, area, count, origin, station, lines);
				Reply(player.entityId, RequestKind.Query, 0, requestId, 0, lines);
				return;
			}

			if (kind == RequestKind.Pay)
			{
				int payToken;
				byte code = ServerPay.Pay(player, name, area, count, origin, station, lines, out payToken);
				Reply(player.entityId, RequestKind.Pay, code, requestId, payToken, lines);
			}
		}

		// 3.2 NetPackage.GetLength. Not an override: 3.3 removed that method. PackageEmit adds the override on 3.2.
		public int GetLength()
		{
			return 64;
		}

		private static void Reply(int entityId, byte kind, byte code, int requestId, int token, List<Line> lines)
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null)
			{
				return;
			}

			manager.SendPackage(
				PackageEmit.Take<NetPackageQuartermasterReply>().Setup(kind, code, requestId, token, lines),
				false,
				entityId);
		}
	}

	[Preserve]
	public abstract class NetPackageQuartermasterReply : NetPackage
	{
		private byte kind;
		private byte code;
		private int requestId;
		private int token;
		private readonly List<Line> lines = new List<Line>();

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		internal NetPackageQuartermasterReply Setup(byte kind, byte code, int requestId, int token, List<Line> source)
		{
			this.kind = kind;
			this.code = code;
			this.requestId = requestId;
			this.token = token;
			lines.Clear();
			if (source != null)
			{
				lines.AddRange(source);
			}

			return this;
		}

		public override void read(PooledBinaryReader reader)
		{
			kind = reader.ReadByte();
			code = reader.ReadByte();
			requestId = reader.ReadInt32();
			token = reader.ReadInt32();
			lines.Clear();
			int count = reader.ReadInt16();
			for (int i = 0; i < count; i++)
			{
				lines.Add(new Line
				{
					Type = reader.ReadInt32(),
					Need = reader.ReadInt32(),
					Inv = reader.ReadInt32(),
					Grid = reader.ReadInt32(),
					Nearby = reader.ReadInt32(),
					Open = reader.ReadInt32()
				});
			}
		}

		public override void write(PooledBinaryWriter writer)
		{
			base.write(writer);
			BinaryWriter raw = writer;
			raw.Write(kind);
			raw.Write(code);
			raw.Write(requestId);
			raw.Write(token);
			raw.Write((short)lines.Count);
			for (int i = 0; i < lines.Count; i++)
			{
				Line line = lines[i];
				raw.Write(line.Type);
				raw.Write(line.Need);
				raw.Write(line.Inv);
				raw.Write(line.Grid);
				raw.Write(line.Nearby);
				raw.Write(line.Open);
			}
		}

		public override void ProcessPackage(World world, GameManager callbacks)
		{
			if (kind == RequestKind.Query)
			{
				CraftBridge.OnQuery(requestId, lines);
				return;
			}

			if (kind == RequestKind.Pool)
			{
				CraftBridge.OnPool(requestId, lines);
				return;
			}

			if (kind == RequestKind.Pay)
			{
				CraftBridge.OnPay(code, token, lines);
			}
		}

		// 3.2 NetPackage.GetLength. Not an override: 3.3 removed that method. PackageEmit adds the override on 3.2.
		public int GetLength()
		{
			return 32 + lines.Count * 24;
		}
	}
}
