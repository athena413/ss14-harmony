using Content.Shared._Goobstation.Bingle.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Bingle.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedBinglePitSystem))]
[AutoGenerateComponentState]
public sealed partial class BinglePitComponent : Component
{
    public const string PitContainerName = "pit";

    [ViewVariables]
    public Container Pit = default!;

    [DataField, AutoNetworkedField]
    public int Points;

    [DataField]
    public int PointsPerObject = 1;

    [DataField]
    public int AliveBonus = 4;

    [DataField]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");

    /// <summary>
    /// The current level that the bingle pit is at.
    /// This is also the level that the bingle pit will start at.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentLevel;

    // This is probably not well-balanced at all, and will definitely need to be changed a lot.
    /// <summary>
    /// The list of levels that the bingle pit can reach.
    /// The lower in the list a level is, the later it happens.
    /// </summary>
    [DataField]
    public List<BinglePitLevel> Levels =
    [
        new(0, false, 1, 1f),
        new(10, true, 1, 2f),
    ];

    /// <summary>
    /// The time between something touching the pit and being inserted.
    /// </summary>
    [DataField]
    public TimeSpan FallIntoPitTime = TimeSpan.FromSeconds(1.8f);
}

/// <summary>
/// Defines the values for a single level of the bingle pit.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public sealed partial class BinglePitLevel
{
    /// <summary>
    /// The amount of points required before advancing to this level from the previous one.
    /// </summary>
    [DataField]
    public int PointsRequired;

    /// <summary>
    /// Should the pit be able to eat living beings at this level.
    /// </summary>
    [DataField]
    public bool CanEatLiving;

    /// <summary>
    /// The amount of points required in between each bingle.
    /// </summary>
    [DataField]
    public int PointsPerBingle;

    /// <summary>
    /// The size of the pit at that level.
    /// </summary>
    [DataField]
    public float Size;

    public BinglePitLevel(int pointsRequired, bool canEatLiving, int pointsPerBingle, float size)
    {
        PointsRequired = pointsRequired;
        CanEatLiving = canEatLiving;
        PointsPerBingle = pointsPerBingle;
        Size = size;
    }
}
