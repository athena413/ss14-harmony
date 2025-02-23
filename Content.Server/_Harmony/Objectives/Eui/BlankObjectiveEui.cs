using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Objectives;

namespace Content.Server.Objectives;

public sealed class BlankObjectiveEui : BaseEui
{
    private readonly MetaDataSystem _metaData;
    private EntityUid _target;
    public BlankObjectiveEui(MetaDataSystem metadataSystem, EntityUid target)
    {
        _metaData = metadataSystem;
        _target = target;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is not ObjetiveSaveMessage s)
        {
            return;
        }

        _metaData.SetEntityName(_target, s.Name);
        _metaData.SetEntityDescription(_target, s.Description);
        Close();
    }
}
