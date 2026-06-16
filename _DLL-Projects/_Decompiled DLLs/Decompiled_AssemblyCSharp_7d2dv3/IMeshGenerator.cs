public interface IMeshGenerator
{
	bool IsLayerEmpty(int _layerIdx);

	bool IsLayerEmpty(int _startLayerIdx, int _endLayerIdx);
}
