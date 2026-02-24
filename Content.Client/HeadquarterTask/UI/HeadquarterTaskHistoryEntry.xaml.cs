using Content.Client.Message;
using Content.Shared.HeadquarterTask;
using Content.Shared.HeadquarterTask.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.HeadquarterTask.UI;

public sealed partial class HeadquarterTaskHistoryEntry : BoxContainer
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private readonly RichTextLabel _manifestLabel;
    private readonly RichTextLabel _timestampLabel;
    private readonly RichTextLabel _noticeLabel;

    public HeadquarterTaskHistoryEntry(HeadquarterTaskHistoryData task)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _manifestLabel = this.FindControl<RichTextLabel>("ManifestLabel");
        _timestampLabel = this.FindControl<RichTextLabel>("TimestampLabel");
        _noticeLabel = this.FindControl<RichTextLabel>("NoticeLabel");

        if (!_prototype.Resolve<HeadquarterTaskPrototype>(task.Task, out var taskPrototype))
            return;

        _manifestLabel.SetMarkup(Loc.GetString("headquarter-task-console-manifest-label", ("item", Loc.GetString(taskPrototype.Name))));
        _timestampLabel.SetMarkup(task.Timestamp.ToString(@"hh\:mm\:ss"));

        if (task.Result == HeadquarterTaskHistoryData.TaskResult.Completed)
        {
            _noticeLabel.SetMarkup(Loc.GetString("headquarter-task-console-history-notice-completed-label"));
            return;
        }

        _noticeLabel.SetMarkup(Loc.GetString("headquarter-task-console-history-notice-closed-label"));
    }
}
