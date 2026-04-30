using UnityEngine;

public class GUIWindowEditBlockSpawnEntity : GUIWindow
{
	public static string ID = typeof(GUIWindowEditBlockSpawnEntity).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUICompList compEntitiesToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dXZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dYm;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dYp;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedEntityClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	public GUIWindowEditBlockSpawnEntity(GameManager _gm)
		: base(ID, 580, 280, _bDrawBackground: true)
	{
	}

	public void SetBlockValue(Vector3i _blockPos, BlockValue _bv)
	{
		blockPos = _blockPos;
		blockValue = _bv;
		compEntitiesToSpawn = new GUICompList(new Rect(0f, 0f, 350f, 200f));
		if (_bv.Block is BlockSpawnEntity { spawnClasses: var spawnClasses } blockSpawnEntity)
		{
			foreach (string line in spawnClasses)
			{
				compEntitiesToSpawn.AddLine(line);
			}
			selectedEntityClass = blockSpawnEntity.spawnClasses[_bv.meta];
			compEntitiesToSpawn.SelectEntry(selectedEntityClass);
		}
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		GUILayout.Space(20f);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label("Select entity to spawn:", GUILayout.Width(180f));
		GUILayout.Space(5f);
		compEntitiesToSpawn.OnGUILayout();
		selectedEntityClass = compEntitiesToSpawn.SelectedEntry;
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		if (GUILayoutButton("Ok"))
		{
			blockValue.meta = (byte)compEntitiesToSpawn.SelectedItemIndex;
			GameManager.Instance.World.SetBlockRPC(0, blockPos, blockValue);
			windowManager.Close(this);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.EndVertical();
	}
}
