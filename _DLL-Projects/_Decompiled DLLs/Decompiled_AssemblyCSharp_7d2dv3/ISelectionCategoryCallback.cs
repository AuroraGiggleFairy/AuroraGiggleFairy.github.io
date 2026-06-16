public interface ISelectionCategoryCallback
{
	void OnSelectionCategoryVisibilityChanged(SelectionCategory _category, bool _visible);

	void OnSelectionCategoryBoxAdded(SelectionBox _box);

	void OnSelectionCategoryBoxRemoved(SelectionBox _box);

	void OnSelectionCategoryCleared(SelectionCategory _category);
}
