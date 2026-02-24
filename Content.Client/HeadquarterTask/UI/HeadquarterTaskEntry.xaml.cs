using Content.Client.Message;
using Content.Shared.HeadquarterTask;
using Content.Shared.HeadquarterTask.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.HeadquarterTask.UI;

public sealed partial class HeadquarterTaskEntry : BoxContainer
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private readonly RichTextLabel _manifestLabel;
    private readonly RichTextLabel _descriptionLabel;
    private readonly Button _acceptTaskButton;

    public Action? OnAcceptTaskButtonPressed;

    public HeadquarterTaskEntry(HeadquarterTaskData task, bool showAcceptButton = true)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _manifestLabel = this.FindControl<RichTextLabel>("ManifestLabel");
        _descriptionLabel = this.FindControl<RichTextLabel>("DescriptionLabel");
        _acceptTaskButton = this.FindControl<Button>("AcceptTaskButton");
        _acceptTaskButton.Visible = showAcceptButton;
        _acceptTaskButton.Disabled = false;
        _acceptTaskButton.OnPressed += _ => OnAcceptTaskButtonPressed?.Invoke();

        if (!_prototype.Resolve<HeadquarterTaskPrototype>(task.Task, out var taskPrototype))
            return;

        _manifestLabel.SetMarkup(Loc.GetString("headquarter-task-console-manifest-label", ("item", Loc.GetString(taskPrototype.Name))));
        _descriptionLabel.SetMarkup(Loc.GetString("headquarter-task-console-description-label", ("description", Loc.GetString(taskPrototype.Description))));
    }
}
