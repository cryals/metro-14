using Content.Client.HeadquarterTask.UI;
using Content.Shared.HeadquarterTask.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.HeadquarterTask.BUI;

[UsedImplicitly]
public sealed class HeadquarterTaskConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private HeadquarterTaskMenu? _menu;

    public HeadquarterTaskConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<HeadquarterTaskMenu>();
        _menu.OnAcceptTaskPressed += id => SendMessage(new HeadquarterTaskAcceptMessage(id));
        _menu.OnCancelTaskPressed += () => SendMessage(new HeadquarterTaskCancelMessage());
        _menu.OnSubmitReportPressed += () => SendMessage(new HeadquarterTaskSubmitReportMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not HeadquarterTaskConsoleState taskState)
            return;

        _menu?.UpdateEntries(
            taskState.Tasks,
            taskState.History,
            taskState.HasActiveTask,
            taskState.ActiveTask,
            taskState.ActiveTaskReportSent,
            taskState.CanApproveActiveTaskReport);
    }
}
