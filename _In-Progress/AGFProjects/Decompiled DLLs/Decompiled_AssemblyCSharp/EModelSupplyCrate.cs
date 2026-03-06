using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelSupplyCrate : EModelBase
{
	public Transform parachute;

	public override void Init(World _world, Entity _entity)
	{
		base.Init(_world, _entity);
		parachute = base.transform.FindInChilds("parachute_supplies");
	}

	public override void SetSkinTexture(string _texture)
	{
		base.SetSkinTexture(_texture);
		if (_texture == null || _texture.Length == 0)
		{
			return;
		}
		for (int i = 0; i < modelTransformParent.childCount; i++)
		{
			Transform child = modelTransformParent.GetChild(i);
			if (child != parachute)
			{
				child.GetComponent<Renderer>().material.mainTexture = DataLoader.LoadAsset<Texture2D>(DataLoader.IsInResources(_texture) ? ("Entities/" + _texture) : _texture);
				break;
			}
		}
	}
}
