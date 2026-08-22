using System.Collections.Generic;

namespace DoomInfighting;

public static class Demons
{
	private const string DemonTag = "demon";

	private const string NoRevengeTag = "noRevenge";

	private const string AlwaysRevengeTag = "alwaysRevenge";

	private const string SpeciesTags = "speciesBruiser";

	private const string NotMissileTags = "hitscan,skullfly";

	private const string NoSplashTag = "noSplash";

	private static readonly HashSet<string> NoSplashClasses = new HashSet<string> { "demonCyber" };

	private static FastTags<TagGroup.Global> _demon;

	private static FastTags<TagGroup.Global> _noRevenge;

	private static FastTags<TagGroup.Global> _alwaysRevenge;

	private static FastTags<TagGroup.Global> _species;

	private static FastTags<TagGroup.Global> _notMissile;

	private static FastTags<TagGroup.Global> _noSplash;

	private static bool _parsed;

	public static bool Is(EntityAlive entity)
	{
		return Test(entity, 0);
	}

	public static bool NoRevenge(EntityAlive entity)
	{
		return Test(entity, 1);
	}

	public static bool AlwaysRevenge(EntityAlive entity)
	{
		return Test(entity, 2);
	}

	public static bool NotMissile(EntityAlive entity)
	{
		return Test(entity, 3);
	}

	public static bool SplashImmune(EntityAlive entity)
	{
		EntityClass cls = ClassOf(entity);
		if (cls == null)
		{
			return false;
		}
		Parse();
		if (!cls.Tags.Test_AnySet(_noSplash))
		{
			return NoSplashClasses.Contains(cls.entityClassName);
		}
		return true;
	}

	public static bool SameSpecies(EntityAlive a, EntityAlive b)
	{
		if (a.entityClass == b.entityClass)
		{
			return true;
		}
		EntityClass ca = ClassOf(a);
		EntityClass cb = ClassOf(b);
		if (ca == null || cb == null)
		{
			return false;
		}
		Parse();
		FastTags<TagGroup.Global> mine = ca.Tags & _species;
		if (!mine.IsEmpty)
		{
			return mine.Test_AnySet(cb.Tags & _species);
		}
		return false;
	}

	private static bool Test(EntityAlive entity, int which)
	{
		EntityClass cls = ClassOf(entity);
		if (cls == null)
		{
			return false;
		}
		Parse();
		return which switch
		{
			0 => cls.Tags.Test_AnySet(_demon), 
			1 => cls.Tags.Test_AnySet(_noRevenge), 
			2 => cls.Tags.Test_AnySet(_alwaysRevenge), 
			_ => cls.Tags.Test_AnySet(_notMissile), 
		};
	}

	private static void Parse()
	{
		if (!_parsed)
		{
			_demon = FastTags<TagGroup.Global>.Parse("demon");
			_noRevenge = FastTags<TagGroup.Global>.Parse("noRevenge");
			_alwaysRevenge = FastTags<TagGroup.Global>.Parse("alwaysRevenge");
			_species = FastTags<TagGroup.Global>.Parse("speciesBruiser");
			_notMissile = FastTags<TagGroup.Global>.Parse("hitscan,skullfly");
			_noSplash = FastTags<TagGroup.Global>.Parse("noSplash");
			_parsed = true;
		}
	}

	private static EntityClass ClassOf(EntityAlive entity)
	{
		if (!(entity == null))
		{
			return entity.EntityClass;
		}
		return null;
	}
}
