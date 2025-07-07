using Content.Shared._Goobstation.Bingle.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Bingle.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedBinglePitSystem))]
[AutoGenerateComponentState]
public sealed partial class BingleSpawnerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Pit = EntityUid.Invalid;
}
