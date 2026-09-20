public static class SelectedObjects
{
	private static NanoObject _selected_nano_object;

	public static void unselectNanoObject()
	{
		_selected_nano_object = null;
		PowerTabController.prev_selected_meta_id = null;
	}

	public static bool isNanoObjectSelected(NanoObject pNanoObject)
	{
		if (!isNanoObjectSet())
		{
			return false;
		}
		return pNanoObject == _selected_nano_object;
	}

	public static bool isNanoObjectSet()
	{
		return !_selected_nano_object.isRekt();
	}

	public static void setNanoObject(NanoObject pNanoObject)
	{
		_selected_nano_object = pNanoObject;
		SoundBox.click();
	}

	public static NanoObject getSelectedNanoObject()
	{
		return _selected_nano_object;
	}
}
