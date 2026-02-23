using Content.Shared.HeadquarterTask;
using Content.Shared.HeadquarterTask.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.HeadquarterTask.Components;

/// <summary>
/// Phone-local task giver that stores and displays a unique task list per entity.
/// </summary>
[RegisterComponent]
[ComponentProtoName("HeadquarterTaskGiver")]
public sealed partial class HeadquarterTaskComponent : Component
{
    /// <summary>
    /// Maximum amount of active tasks shown by this console.
    /// </summary>
    [DataField]
    public int MaxBounties = 3;

    /// <summary>
    /// Active tasks for this specific console instance.
    /// </summary>
    [DataField]
    public List<HeadquarterTaskData> Tasks = new();

    /// <summary>
    /// Completed / closed tasks for this device during the current round.
    /// </summary>
    [DataField]
    public List<HeadquarterTaskHistoryData> History = new();

    /// <summary>
    /// Currently accepted task prototype for this device.
    /// </summary>
    [DataField]
    public ProtoId<HeadquarterTaskPrototype>? ActiveTask;

    /// <summary>
    /// Localized faction/station name used in completion report announcements.
    /// </summary>
    [DataField]
    public LocId FactionName = "headquarter-task-faction-unknown";

    /// <summary>
    /// Whether a completion report was already submitted for the active task.
    /// </summary>
    [DataField]
    public bool ActiveTaskReportSent;

    /// <summary>
    /// The group that tasks are pulled from.
    /// </summary>
    [DataField]
    public ProtoId<HeadquarterTaskGroupPrototype> Group = "HeadquarterTaskDefault";
}

[NetSerializable, Serializable]
public sealed class HeadquarterTaskConsoleState : BoundUserInterfaceState
{
    public List<HeadquarterTaskData> Tasks;
    public List<HeadquarterTaskHistoryData> History;
    public bool HasActiveTask;
    public HeadquarterTaskData ActiveTask;
    public bool ActiveTaskReportSent;
    public bool CanApproveActiveTaskReport;

    public HeadquarterTaskConsoleState(
        List<HeadquarterTaskData> tasks,
        List<HeadquarterTaskHistoryData> history,
        bool hasActiveTask,
        HeadquarterTaskData activeTask,
        bool activeTaskReportSent,
        bool canApproveActiveTaskReport)
    {
        Tasks = tasks;
        History = history;
        HasActiveTask = hasActiveTask;
        ActiveTask = activeTask;
        ActiveTaskReportSent = activeTaskReportSent;
        CanApproveActiveTaskReport = canApproveActiveTaskReport;
    }
}

[Serializable, NetSerializable]
public sealed class HeadquarterTaskAcceptMessage : BoundUserInterfaceMessage
{
    public string TaskId;

    public HeadquarterTaskAcceptMessage(string taskId)
    {
        TaskId = taskId;
    }
}

[Serializable, NetSerializable]
public sealed class HeadquarterTaskCancelMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class HeadquarterTaskSubmitReportMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public enum HeadquarterTaskUiKey : byte
{
    Key
}
