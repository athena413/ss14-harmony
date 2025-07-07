using Content.Shared._Goobstation.Bingle.EntitySystems;
using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Bingle.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedBinglePitSystem))]
[AutoGenerateComponentState]
public sealed partial class BingleComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Upgraded;

    [DataField]
    public ProtoId<PolymorphPrototype> UpgradePolymorph = "BinglePolymorph";
}
