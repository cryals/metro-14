using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Shared.HeadquarterTask;
using Content.Shared.HeadquarterTask.Components;
using Content.Shared.HeadquarterTask.Prototypes;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.HeadquarterTask.Systems;

/// <summary>
/// Generates and maintains per-entity headquarters task lists.
/// </summary>
public sealed class HeadquarterTaskSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadquarterTaskComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HeadquarterTaskComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<HeadquarterTaskComponent, HeadquarterTaskAcceptMessage>(OnAcceptTaskMessage);
        SubscribeLocalEvent<HeadquarterTaskComponent, HeadquarterTaskCancelMessage>(OnCancelTaskMessage);
        SubscribeLocalEvent<HeadquarterTaskComponent, HeadquarterTaskSubmitReportMessage>(OnSubmitReportMessage);
    }

    private void OnMapInit(EntityUid uid, HeadquarterTaskComponent component, MapInitEvent args)
    {
        FillTaskDatabase(uid, component);
    }

    private void OnUiOpened(EntityUid uid, HeadquarterTaskComponent component, BoundUIOpenedEvent args)
    {
        FillTaskDatabase(uid, component, args.Actor);
    }

    private void OnAcceptTaskMessage(EntityUid uid, HeadquarterTaskComponent component, HeadquarterTaskAcceptMessage args)
    {
        if (component.ActiveTask != null)
            return;

        if (!TryGetTaskDataByPrototypeId(component, args.TaskId, out var task))
            return;

        component.ActiveTask = task.Value.Task;
        component.ActiveTaskReportSent = false;
        UpdateTaskUi(uid, component, args.Actor);
    }

    private void OnCancelTaskMessage(EntityUid uid, HeadquarterTaskComponent component, HeadquarterTaskCancelMessage args)
    {
        if (component.ActiveTask == null)
            return;

        component.ActiveTask = null;
        component.ActiveTaskReportSent = false;
        UpdateTaskUi(uid, component, args.Actor);
    }

    private void OnSubmitReportMessage(EntityUid uid, HeadquarterTaskComponent component, HeadquarterTaskSubmitReportMessage args)
    {
        if (component.ActiveTask == null)
            return;

        var isAdminReviewer = _adminManager.IsAdmin(args.Actor);

        if (component.ActiveTaskReportSent)
        {
            if (!isAdminReviewer)
            {
                UpdateTaskUi(uid, component, args.Actor);
                return;
            }

            if (!TryCompleteActiveTask(component))
                return;

            FillTaskDatabase(uid, component, args.Actor);
            return;
        }

        if (!_protoMan.TryIndex(component.ActiveTask.Value, out HeadquarterTaskPrototype? taskProto))
            return;

        component.ActiveTaskReportSent = true;

        var actorName = args.Actor.Valid ? MetaData(args.Actor).EntityName : "Unknown";
        var factionName = Loc.GetString(component.FactionName);
        var taskName = Loc.GetString(taskProto.Name);
        var message = $"{actorName} отправил задание {taskName} от фракции {factionName}";
        _chatManager.SendAdminAnnouncement(message);

        UpdateTaskUi(uid, component, args.Actor);
    }

    /// <summary>
    /// Fills the device task list up to its configured limit.
    /// </summary>
    public void FillTaskDatabase(EntityUid uid, HeadquarterTaskComponent? component = null, EntityUid? actor = null)
    {
        if (!Resolve(uid, ref component))
            return;

        while (component.Tasks.Count < component.MaxBounties)
        {
            if (!TryAddTask(uid, component))
                break;
        }

        UpdateTaskUi(uid, component, actor);
    }

    public void RerollTaskDatabase(Entity<HeadquarterTaskComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Tasks.Clear();
        entity.Comp.ActiveTask = null;
        entity.Comp.ActiveTaskReportSent = false;
        FillTaskDatabase(entity.Owner, entity.Comp);
    }

    [PublicAPI]
    public bool TryAddTask(EntityUid uid, HeadquarterTaskComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var allTasks = _protoMan.EnumeratePrototypes<HeadquarterTaskPrototype>()
            .Where(p => p.Group == component.Group)
            .ToList();

        if (allTasks.Count == 0)
            return false;

        var filteredTasks = new List<HeadquarterTaskPrototype>();
        foreach (var proto in allTasks)
        {
            if (component.Tasks.Any(t => t.Task == proto.ID))
                continue;
            if (component.History.Any(h => h.Task == proto.ID))
                continue;

            filteredTasks.Add(proto);
        }

        if (filteredTasks.Count == 0)
            return false;

        var task = _random.Pick(filteredTasks);
        return TryAddTask(uid, task, component);
    }

    [PublicAPI]
    public bool TryAddTask(EntityUid uid, string taskId, HeadquarterTaskComponent? component = null)
    {
        if (!_protoMan.TryIndex<HeadquarterTaskPrototype>(taskId, out var task))
            return false;

        return TryAddTask(uid, task, component);
    }

    public bool TryAddTask(EntityUid uid, HeadquarterTaskPrototype task, HeadquarterTaskComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Tasks.Count >= component.MaxBounties)
            return false;

        var newTask = new HeadquarterTaskData(task);
        if (component.Tasks.Any(t => t.Task == newTask.Task))
        {
            Log.Error("Failed to add headquarters task {Task} because it already existed in this device list!", newTask.Task);
            return false;
        }

        component.Tasks.Add(newTask);
        return true;
    }

    public void UpdateTaskUi(EntityUid uid, HeadquarterTaskComponent? component = null, EntityUid? actor = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_uiSystem.HasUi(uid, HeadquarterTaskUiKey.Key))
            return;

        var canApproveActiveTaskReport = actor is { Valid: true } viewer && _adminManager.IsAdmin(viewer);
        var hasActiveTask = TryGetActiveTaskData(component, out var activeTask);
        if (!hasActiveTask)
        {
            component.ActiveTask = null;
            component.ActiveTaskReportSent = false;
        }

        _uiSystem.SetUiState(uid, HeadquarterTaskUiKey.Key,
            new HeadquarterTaskConsoleState(
                component.Tasks,
                component.History,
                hasActiveTask,
                activeTask ?? default,
                component.ActiveTaskReportSent,
                canApproveActiveTaskReport));
    }

    private bool TryCompleteActiveTask(HeadquarterTaskComponent component)
    {
        if (!TryGetActiveTaskData(component, out var activeTask))
            return false;

        component.History.Add(new HeadquarterTaskHistoryData(
            activeTask.Value,
            HeadquarterTaskHistoryData.TaskResult.Completed,
            _timing.CurTime,
            null));

        component.Tasks.RemoveAll(task => task.Task == activeTask.Value.Task);
        component.ActiveTask = null;
        component.ActiveTaskReportSent = false;
        return true;
    }

    private bool TryGetTaskDataByPrototypeId(
        HeadquarterTaskComponent component,
        string taskId,
        [NotNullWhen(true)] out HeadquarterTaskData? task)
    {
        task = null;

        foreach (var taskData in component.Tasks)
        {
            if (taskData.Task != taskId)
                continue;

            task = taskData;
            return true;
        }

        return false;
    }

    private bool TryGetActiveTaskData(
        HeadquarterTaskComponent component,
        [NotNullWhen(true)] out HeadquarterTaskData? task)
    {
        task = null;
        if (component.ActiveTask == null)
            return false;

        foreach (var taskData in component.Tasks)
        {
            if (taskData.Task != component.ActiveTask.Value)
                continue;

            task = taskData;
            return true;
        }

        return false;
    }
}
