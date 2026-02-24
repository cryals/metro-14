using Content.Client.UserInterface.Controls;
using Content.Shared.HeadquarterTask;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.HeadquarterTask.UI;

public sealed partial class HeadquarterTaskMenu : FancyWindow
{
    private readonly Control _taskListUi;
    private readonly Control _activeTaskUi;
    private readonly TabContainer _masterTabContainer;
    private readonly BoxContainer _taskEntriesContainer;
    private readonly BoxContainer _taskHistoryContainer;
    private readonly BoxContainer _activeTaskEntryContainer;
    private readonly Label _noHistoryLabel;
    private readonly Button _cancelTaskButton;
    private readonly Button _submitReportButton;
    private bool _activeTaskReportSent;

    public Action<string>? OnAcceptTaskPressed;
    public Action? OnCancelTaskPressed;
    public Action? OnSubmitReportPressed;

    public HeadquarterTaskMenu()
    {
        RobustXamlLoader.Load(this);

        _taskListUi = this.FindControl<Control>("TaskListUI");
        _activeTaskUi = this.FindControl<Control>("ActiveTaskUI");
        _masterTabContainer = this.FindControl<TabContainer>("MasterTabContainer");
        _taskEntriesContainer = this.FindControl<BoxContainer>("TaskEntriesContainer");
        _taskHistoryContainer = this.FindControl<BoxContainer>("TaskHistoryContainer");
        _activeTaskEntryContainer = this.FindControl<BoxContainer>("ActiveTaskEntryContainer");
        _noHistoryLabel = this.FindControl<Label>("NoHistoryLabel");
        _cancelTaskButton = this.FindControl<Button>("CancelTaskButton");
        _submitReportButton = this.FindControl<Button>("SubmitReportButton");

        _cancelTaskButton.OnPressed += _ => OnCancelTaskPressed?.Invoke();
        _submitReportButton.OnPressed += _ =>
        {
            if (!_activeTaskReportSent)
            {
                // Show pending status immediately while the server processes the report submission.
                _submitReportButton.Text = Loc.GetString("headquarter-task-console-submit-report-pending-button");
                _submitReportButton.Disabled = true;
                _cancelTaskButton.Disabled = true;
            }

            OnSubmitReportPressed?.Invoke();
        };

        _masterTabContainer.SetTabTitle(0, Loc.GetString("headquarter-task-console-tab-available-label"));
        _masterTabContainer.SetTabTitle(1, Loc.GetString("headquarter-task-console-tab-history-label"));
    }

    public void UpdateEntries(
        List<HeadquarterTaskData> tasks,
        List<HeadquarterTaskHistoryData> history,
        bool hasActiveTask,
        HeadquarterTaskData activeTask,
        bool activeTaskReportSent,
        bool canApproveActiveTaskReport)
    {
        _activeTaskReportSent = activeTaskReportSent;
        _taskListUi.Visible = !hasActiveTask;
        _activeTaskUi.Visible = hasActiveTask;

        if (hasActiveTask)
        {
            UpdateActiveTask(activeTask, activeTaskReportSent, canApproveActiveTaskReport);
            return;
        }

        UpdateTaskList(tasks, history);
    }

    private void UpdateTaskList(List<HeadquarterTaskData> tasks, List<HeadquarterTaskHistoryData> history)
    {
        _taskEntriesContainer.Children.Clear();
        foreach (var task in tasks)
        {
            var entry = new HeadquarterTaskEntry(task);
            entry.OnAcceptTaskButtonPressed += () => OnAcceptTaskPressed?.Invoke(task.Task.ToString());
            _taskEntriesContainer.AddChild(entry);
        }

        _taskEntriesContainer.AddChild(new Control
        {
            MinHeight = 10
        });

        _taskHistoryContainer.Children.Clear();
        if (history.Count == 0)
        {
            _noHistoryLabel.Visible = true;
            return;
        }

        _noHistoryLabel.Visible = false;

        // Show the history in reverse, so the newest entry is listed first.
        for (var i = history.Count - 1; i >= 0; i--)
        {
            _taskHistoryContainer.AddChild(new HeadquarterTaskHistoryEntry(history[i]));
        }
    }

    private void UpdateActiveTask(HeadquarterTaskData activeTask, bool activeTaskReportSent, bool canApproveActiveTaskReport)
    {
        _activeTaskEntryContainer.Children.Clear();
        _activeTaskEntryContainer.AddChild(new HeadquarterTaskEntry(activeTask, false));
        _activeTaskEntryContainer.AddChild(new Control
        {
            MinHeight = 10
        });

        if (!activeTaskReportSent)
        {
            _submitReportButton.Text = Loc.GetString("headquarter-task-console-submit-report-button");
            _cancelTaskButton.Disabled = false;
            _submitReportButton.Disabled = false;
            return;
        }

        if (canApproveActiveTaskReport)
        {
            _submitReportButton.Text = Loc.GetString("headquarter-task-console-approve-task-button");
            _cancelTaskButton.Disabled = false;
            _submitReportButton.Disabled = false;
            return;
        }

        _submitReportButton.Text = Loc.GetString("headquarter-task-console-submit-report-pending-button");
        _cancelTaskButton.Disabled = true;
        _submitReportButton.Disabled = true;
    }
}
