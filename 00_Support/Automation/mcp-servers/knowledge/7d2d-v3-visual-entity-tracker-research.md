# 7D2D v3.0 Research: XML Visual Entity Tracking and Animal Tracker Interaction

Research date: 2026-07-18  
Primary mod reviewed: `01_Draft/AGF-NoEAC-VisualEntityTracker-v0.2.0`  
Vanilla XML reviewed: `C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config`  
Decompiled C# reviewed: `00_DLL-Projects/References/Decompiled-DLLs/Decompiled_AssemblyCSharp_7d2dv3`

## Current draft mod behavior

The draft mod is XML-only. It does not currently include a compiled DLL.

Files involved:

- `Config/entityclasses.xml`
  - Appends an `effect_group` to `playerMale` that adds `buffZAlert` on `onSelfEnteredGame` and `onSelfRespawn`.
  - Adds `bugswarm` tags to `animalInsectSwarm` and `animalBeeSwarm`.
- `Config/buffs.xml`
  - Adds hidden player buff `buffZAlert` with `update_rate=".45"`.
  - `buffZAlert` applies short marker buffs to nearby entities using `target="selfAOE"`, `range="25"`, and `target_tags` such as `zombie`, `mutant`, `alien`, `bandit`, `bear`, `chicken`, `deer`, `rabbit`, `wolf,coyote,dog`, `mountainlion`, `vulture`, `snake`, `boar`, and `bugswarm`.
  - Each marker buff lasts `1` second and uses `SetNavObject` on start/remove to add/remove a nav object class from the target entity.
- `Config/nav_objects.xml`
  - Defines custom nav object classes such as `ZAzombie`, `ZAbandit`, `ZAbear`, `ZAchicken`, etc.
  - All custom classes use `requirement_type="Tracking"` and compass-only settings with `max_distance="25"`.

In short: a hidden player buff repeatedly scans nearby entities and temporarily adds nav object classes to those entities. The compass then shows those nav objects if the local player passes the nav object's `Tracking` requirement.

## Vanilla systems involved

### `SetNavObject`

Decompiled file: `MinEventActionSetNavObject.cs`

`SetNavObject` is a vanilla min event action. It parses:

- `nav_object`
- `sprite`
- `text`
- `cvar_to_text`
- `add`

On execute, it calls `EntityAlive.AddNavObject(...)` when `add="true"`, or `EntityAlive.RemoveNavObject(...)` when `add="false"`.

### Entity nav object storage

Decompiled file: `Entity.cs`

Relevant behavior:

```csharp
public virtual void HandleNavObject()
{
    if (EntityClass.list[entityClass].NavObject != "")
    {
        NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[entityClass].NavObject, this);
    }
}

public void AddNavObject(string navObjectName, string overrideSprite, string overrideText)
{
    if (NavObject == null)
    {
        NavObjectManager.Instance.RegisterNavObject(navObjectName, this, overrideSprite).name = overrideText;
        return;
    }
    NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
    NavObject.name = overrideText;
    NavObject.AddNavObjectClass(navObjectClass);
}

public void RemoveNavObject(string navObjectName)
{
    NavObjectClass navObjectClass = NavObjectClass.GetNavObjectClass(navObjectName);
    if (NavObject != null && NavObject.RemoveNavObjectClass(navObjectClass))
    {
        NavObject = null;
    }
}
```

Important consequence: one entity has one `NavObject`, but that nav object can hold multiple `NavObjectClass` entries. Adding/removing marker classes repeatedly is supported by vanilla, but it does mean the mod is mutating nav object class lists on every tracked entity every update cycle.

### `requirement_type="Tracking"`

Decompiled files:

- `NavObjectClass.cs`
- `NavObject.cs`
- `MapObjectAnimal.cs`

`NavObjectClass.RequirementTypes` includes `Tracking`. For entity-tracked nav objects, `Tracking` validity is checked with:

```csharp
EffectManager.GetValue(PassiveEffects.Tracking, null, 0f, player, null, entity.EntityTags) > 0f
```

This means the local player must have a `Tracking` passive that matches at least one of the tracked entity's tags.

The draft mod provides this with:

```xml
<passive_effect name="Tracking" operation="base_set" value="1" tags="zombie,mutant,alien,bandit,animal"/>
```

Because this passive includes the broad `animal` tag, it can make any vanilla nav object with `requirement_type="Tracking"` valid for entities whose tags include `animal`, not only the mod's custom `ZA*` nav objects.

## Vanilla Animal Tracker behavior

Vanilla XML:

- `progression.xml`: `perkAnimalTracker`
- `buffs.xml`: `buffAnimalTracker`, `buffAnimalTrackerAcquired`
- `nav_objects.xml`: `animaltracking_*`
- `entityclasses.xml`: animal entity `NavObject` properties and tags

Key vanilla progression behavior:

- `perkAnimalTracker` gives `TrackDistance` values: `0,100,100,100,100,100` for levels `0..5`.
- Crouching triggers `buffAnimalTracker` only when `perkAnimalTracker > 0` and `buffAnimalTrackerAcquired` is not already active.
- `buffAnimalTracker` uses timers and `HasTrackedEntity` checks to determine whether a valid animal is in range.
- `buffAnimalTrackerAcquired` applies `Tracking` passives for rank-specific tags:
  - `perkAT01`
  - `perkAT02`
  - `perkAT03`
  - `perkAT04`
  - `perkAT05`

Vanilla note in `buffs.xml` says:

> `HasTrackedEntity tags (entity tags). This uses the "TrackDistance" passive for the distance check. Is not actually connected to the AnimalTracking passive. It simply returns TRUE if a tagged entity is in range.`

Vanilla animal nav objects are named like:

- `animaltracking_timid`
- `animaltracking_chicken`
- `animaltracking_rabbit`
- `animaltracking_doe`
- `animaltracking_stag`
- `animaltracking_hostile`
- `animaltracking_bear`
- `animaltracking_boar`
- `animaltracking_coyote`
- `animaltracking_direwolf`
- `animaltracking_mountainlion`
- `animaltracking_snake`
- `animaltracking_wolf`

These also use `requirement_type="Tracking"`.

## Likely interaction / bug with `perkAnimalTracker`

The draft mod's permanent hidden `buffZAlert` sets:

```xml
<passive_effect name="Tracking" operation="base_set" value="1" tags="zombie,mutant,alien,bandit,animal"/>
```

This likely interferes with vanilla Animal Tracker presentation because vanilla `animaltracking_*` nav objects also become valid when the local player has any matching `Tracking` passive for the target entity's tags.

Effectively, the mod can grant a broad always-on `Tracking` passive for `animal`, while vanilla expects animal tracking passives to be controlled by `perkAnimalTracker` rank and the crouch-initiated tracker buffs.

This does **not** appear to directly alter progression levels or skill points. The risk is functional bypass/visual leakage: animal nav objects may appear without the perk, or at times/ranges not intended by vanilla progression, depending on which animal already has vanilla animaltracking nav object classes and whether the mod's custom marker nav object classes are active.

## Cleaner XML-only approaches

### Best XML-only improvement: avoid broad vanilla tracking tags

To avoid interfering with vanilla animal tracking, use custom marker tags instead of broad tags like `animal` on the player's `Tracking` passive.

Example concept:

1. Add custom tags to the target entities or categories, such as:
   - `agfVetZombie`
   - `agfVetBandit`
   - `agfVetBear`
   - `agfVetChicken`
   - etc.
2. Change `buffZAlert`'s `Tracking` passive to only those custom tags:

```xml
<passive_effect name="Tracking" operation="base_set" value="1" tags="agfVetZombie,agfVetBandit,agfVetBear,agfVetChicken,..."/>
```

3. Keep the AOE `target_tags` scan on broad vanilla tags if desired, but ensure the final nav object `Tracking` requirement is satisfied only by custom tags on those entities.

This preserves XML-only/server-side behavior and prevents the mod from satisfying vanilla `animaltracking_*` classes with the broad `animal` tag.

### Lower-impact XML changes

- Increase `buffZAlert` `update_rate` from `.45` to something closer to `.75` or `1.0` if responsiveness remains acceptable.
- Increase marker buff durations slightly above the update rate, e.g. `duration="1.5"`, to reduce flicker if an update is delayed.
- Reduce the number of category-specific marker buffs if exact animal icon distinction is not required.
- Consider separate optional variants:
  - Accessibility full tracker: zombies + animals + bandits.
  - Hostiles only: zombies + bandits + hostile animals.
  - Animals off: avoids overlap with Animal Tracker entirely.

## DLL / Harmony considerations

A DLL is **not required** for the current feature set. Vanilla XML already supports:

- periodic player buffs,
- AOE target buff application,
- `SetNavObject`,
- custom `nav_object_class` definitions,
- `Tracking` passives and tag gating.

A DLL could be cleaner or more performant only if implementing a true client-side/local-player tracker that directly reads nearby entities and renders markers without applying/removing buffs on every entity. However, that would require EAC off and compiled maintenance. For this mod's accessibility goal and server-side XML distribution, staying XML-only is reasonable.

## Performance notes

The current XML approach has two main costs:

1. Every player with `buffZAlert` runs multiple `selfAOE` scans every `.45` seconds.
2. Every matching nearby entity gets a short marker buff that calls `SetNavObject` on start and remove.

This is likely acceptable at radius `25` for small to moderate player counts, but cost scales with player count, nearby entity count, and number of target categories. It is heavier than vanilla Animal Tracker, which is perk-gated and event/timer driven.

Recommended safer defaults:

- `range="25"` is reasonable.
- `update_rate=".75"` or `1.0` is less aggressive than `.45`.
- Marker `duration` should be longer than update rate to prevent flicker.
- Avoid broad `Tracking` tags that unlock vanilla systems unintentionally.

## Recommended next update for AGF-NoEAC-VisualEntityTracker

1. Keep the mod XML-only unless a future UI-specific feature truly requires a DLL.
2. Replace broad `Tracking` tags (`zombie,mutant,alien,bandit,animal`) with AGF-specific tracking tags.
3. Add those AGF-specific tags to vanilla entity classes/categories via XML patches.
4. Remove or avoid broad `animal` from the always-on player tracking passive to stop Animal Tracker bypass.
5. Tune update cadence for performance: consider `.75` or `1.0` update rate and `1.5` marker duration.
6. Consider user-facing variants/config notes for hostiles-only vs all-entities accessibility tracking.
