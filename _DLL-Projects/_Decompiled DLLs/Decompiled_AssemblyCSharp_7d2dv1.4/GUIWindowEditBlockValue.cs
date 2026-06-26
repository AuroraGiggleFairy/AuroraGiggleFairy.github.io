using UnityEngine;

public class GUIWindowEditBlockValue : GUIWindow
{
	public static string ID = typeof(GUIWindowEditBlockValue).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFace blockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameManager gameManager;

	public GUIWindowEditBlockValue(GameManager _gm)
		: base(ID, 220, 140, _bDrawBackground: true)
	{
		gameManager = _gm;
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		GUILayout.Space(20f);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		blockValue.hasdecal = GUILayout.Toggle(blockValue.hasdecal, " Decal on", GUILayout.Width(80f));
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label("Texture idx:", GUILayout.Width(80f));
		blockValue.decaltex = (byte)int.Parse(GUILayout.TextField(blockValue.decaltex.ToString(), GUILayout.Width(40f)));
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		if (GUILayoutButton("Ok"))
		{
			if (blockValue.hasdecal)
			{
				blockValue.decalface = blockFace;
			}
			else
			{
				blockValue.decalface = BlockFace.Top;
			}
			gameManager.World.SetBlockRPC(blockPos, blockValue);
			windowManager.Close(this);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.EndVertical();
	}

	public void SetBlock(Vector3i _blockPos, BlockFace _blockFace)
	{
		blockFace = _blockFace;
		blockPos = _blockPos;
		blockValue = gameManager.World.GetBlock(_blockPos);
	}
}
