using System;
using Platform.XBL.Save.Storage.Blobs;
using Platform.XBL.Save.Storage.Files;

namespace Platform.XBL.Save.Storage;

public static class SaveStorageProviderExtensions
{
	public static ISaveStorageProvider Create(this SaveStorageProvider storageProvider)
	{
		return storageProvider switch
		{
			SaveStorageProvider.Blobs => new SaveStorageStorageProviderBlobs(), 
			SaveStorageProvider.Files => new SaveStorageStorageProviderFiles(), 
			_ => throw new ArgumentOutOfRangeException("storageProvider", storageProvider, string.Format("Unknown {0}: {1}", "SaveStorageProvider", storageProvider)), 
		};
	}
}
