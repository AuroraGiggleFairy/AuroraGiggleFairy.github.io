public class ItemWorldData
{
	public IGameManager gameManager;

	public WorldBase world;

	public EntityItem entityItem;

	public int belongsEntityId;

	public ItemWorldData(IGameManager _gm, ItemValue _itemValue, EntityItem _entityItem, int _belongsEntityId)
	{
		gameManager = _gm;
		world = _entityItem.world;
		entityItem = _entityItem;
		belongsEntityId = _belongsEntityId;
	}
}
