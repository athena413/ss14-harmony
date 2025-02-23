using Content.Client.Eui;
using Content.Client._Harmony.Objectives;
using Content.Shared.Eui;
using Content.Shared.Objectives;

namespace Content.Server.Objectives;

public sealed class BlankObjectiveEui : BaseEui
{
    private BlankObjectiveUi _blankObjectiveUi;
    public BlankObjectiveEui()
    {
        _blankObjectiveUi = new BlankObjectiveUi();
        _blankObjectiveUi.OnClose += () => SendMessage(new CloseEuiMessage());
        _blankObjectiveUi.SubmitButton.OnPressed += _ => SendMessage(new ObjetiveSaveMessage("Custom objective", _blankObjectiveUi.SetObjective()));
    }

    public override void Opened()
    {
        _blankObjectiveUi.OpenCentered();
    }
}
