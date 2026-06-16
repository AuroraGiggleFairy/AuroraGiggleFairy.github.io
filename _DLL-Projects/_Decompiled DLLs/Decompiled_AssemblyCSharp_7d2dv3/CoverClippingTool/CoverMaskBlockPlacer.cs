using System;
using UnityEngine;

namespace CoverClippingTool;

public class CoverMaskBlockPlacer : MonoBehaviour
{
	public static readonly string PropCoverMask = "CoverMask";

	public static readonly string PropCoverMaskMulti = "CoverMaskMulti";

	public static readonly string PropCoverOffset = "CoverOffset";

	[SerializeField]
	public SelectedBlockInfo selectedModel = new SelectedBlockInfo(0);

	[SerializeField]
	public Vector3i selectedMultiBlock;

	[SerializeField]
	public bool editMultiBlock;

	[SerializeField]
	public string searchFilter;

	public void UpdateSelectedMultiBlock(string value)
	{
		editMultiBlock = !value.Equals("All", StringComparison.OrdinalIgnoreCase);
		selectedMultiBlock = (editMultiBlock ? StringParsers.ParseVector3i(value) : Vector3i.zero);
	}
}
