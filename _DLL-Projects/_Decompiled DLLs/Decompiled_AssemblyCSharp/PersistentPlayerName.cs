public class PersistentPlayerName
{
	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText playerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedDisplayName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nameSuffix;

	public AuthoredText AuthoredName => playerName;

	public string DisplayName
	{
		get
		{
			if (cachedDisplayName != null)
			{
				return cachedDisplayName + ((nameSuffix > 0) ? $"({nameSuffix})" : "");
			}
			if (!GeneratedTextManager.IsFiltered(playerName))
			{
				if (!GeneratedTextManager.IsFiltering(playerName))
				{
					GeneratedTextManager.PrefilterText(playerName);
				}
				return GeneratedTextManager.GetDisplayTextImmediately(playerName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.None) + ((nameSuffix > 0) ? $"({nameSuffix})" : "");
			}
			cachedDisplayName = GeneratedTextManager.GetDisplayTextImmediately(playerName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
			return cachedDisplayName + ((nameSuffix > 0) ? $"({nameSuffix})" : "");
		}
	}

	public string SafeDisplayName
	{
		get
		{
			if (!GeneratedTextManager.IsFiltered(playerName))
			{
				if (!GeneratedTextManager.IsFiltering(playerName))
				{
					GeneratedTextManager.PrefilterText(playerName);
				}
				string text = GeneratedTextManager.GetDisplayTextImmediately(playerName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
				if (!text.Equals("{...}") && nameSuffix > 0)
				{
					text += $"({nameSuffix})";
				}
				return text;
			}
			return DisplayName;
		}
	}

	public PersistentPlayerName(AuthoredText name)
	{
		playerName = name;
		GeneratedTextManager.PrefilterText(name, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
	}

	public void Update(string name, PlatformUserIdentifierAbs author)
	{
		cachedDisplayName = null;
		nameSuffix = 0;
		playerName.Update(name, author);
		GeneratedTextManager.PrefilterText(playerName, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
		GameManager.Instance.persistentPlayers.FixNameCollisions(name);
	}

	public void Update(AuthoredText name)
	{
		if (playerName != name)
		{
			cachedDisplayName = null;
			nameSuffix = 0;
			playerName = name;
			GeneratedTextManager.PrefilterText(playerName, GeneratedTextManager.TextFilteringMode.FilterOtherPlatforms);
			GameManager.Instance.persistentPlayers.FixNameCollisions(name.Text);
		}
	}

	public void SetCollisionSuffix(int suffix)
	{
		nameSuffix = suffix;
	}
}
