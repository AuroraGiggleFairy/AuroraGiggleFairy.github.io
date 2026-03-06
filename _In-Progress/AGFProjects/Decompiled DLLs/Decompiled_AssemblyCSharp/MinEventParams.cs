using Challenges;
using UnityEngine;

public class MinEventParams
{
	public static MinEventParams CachedEventParam = new MinEventParams();

	public MinEffectController.SourceParentType ParentType;

	public ITileEntity TileEntity;

	public EntityAlive Self;

	public EntityAlive Instigator;

	public EntityAlive Other;

	public EntityAlive[] Others;

	public ItemValue ItemValue;

	public ItemActionData ItemActionData;

	public ItemInventoryData ItemInventoryData;

	public Vector3 StartPosition;

	public Vector3 Position;

	public Transform Transform;

	public BuffValue Buff;

	public BlockValue BlockValue;

	public PrefabInstance POI;

	public Bounds Area;

	public BiomeDefinition Biome;

	public FastTags<TagGroup.Global> Tags;

	public DamageResponse DamageResponse;

	public ProgressionValue ProgressionValue;

	public Challenge Challenge;

	public int Seed;

	public bool IsLocal;

	public static void CopyTo(MinEventParams _source, MinEventParams _destination)
	{
		_destination.TileEntity = _source.TileEntity;
		_destination.Self = _source.Self;
		_destination.Instigator = _source.Instigator;
		_destination.Other = _source.Other;
		_destination.Others = _source.Others;
		_destination.ItemValue = _source.ItemValue;
		_destination.ItemActionData = _source.ItemActionData;
		_destination.ItemInventoryData = _source.ItemInventoryData;
		_destination.StartPosition = _source.StartPosition;
		_destination.Position = _source.Position;
		_destination.Transform = _source.Transform;
		_destination.Buff = _source.Buff;
		_destination.BlockValue = _source.BlockValue;
		_destination.POI = _source.POI;
		_destination.Area = _source.Area;
		_destination.Biome = _source.Biome;
		_destination.Tags = _source.Tags;
		_destination.DamageResponse = _source.DamageResponse;
		_destination.ProgressionValue = _source.ProgressionValue;
		_destination.Seed = _source.Seed;
	}
}
