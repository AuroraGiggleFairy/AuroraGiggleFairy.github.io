using System.IO;

namespace Platform.Shared;

public class SaveGameIOProviderFixedRoot : SaveGameIOProvider
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string m_rootPath;

	public SaveGameIOProviderFixedRoot(string rootPath)
	{
		m_rootPath = GameIO.GetNormalizedPath(rootPath);
		Directory.CreateDirectory(m_rootPath);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPath(SaveDataManagedPath path)
	{
		if (m_rootPath != null)
		{
			return GameIO.GetNormalizedPath(Path.Combine(m_rootPath, path.PathRelativeToRoot));
		}
		return path.GetOriginalPath();
	}
}
