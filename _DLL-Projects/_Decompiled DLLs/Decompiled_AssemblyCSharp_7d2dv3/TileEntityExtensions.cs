public static class TileEntityExtensions
{
	public static T GetSelfOrFeature<T>(this ITileEntity _te) where T : class
	{
		_te.TryGetSelfOrFeature<T>(out var _typedTe);
		return _typedTe;
	}

	public static bool TryGetSelfOrFeature<T>(this ITileEntity _te, out T _typedTe) where T : class
	{
		if (_te == null)
		{
			_typedTe = null;
			return false;
		}
		if (_te is T val)
		{
			_typedTe = val;
			return true;
		}
		if (_te is TileEntityComposite tileEntityComposite)
		{
			_typedTe = tileEntityComposite.GetFeature<T>();
			return _typedTe != null;
		}
		if (_te is ITileEntityFeature tileEntityFeature)
		{
			_typedTe = tileEntityFeature.Parent.GetFeature<T>();
			return _typedTe != null;
		}
		_typedTe = null;
		return false;
	}
}
