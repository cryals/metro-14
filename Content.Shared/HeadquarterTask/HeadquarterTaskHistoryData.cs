using Content.Shared.HeadquarterTask.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.HeadquarterTask;

/// <summary>
/// Stores completed/closed headquarters tasks for a single task giver device.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct HeadquarterTaskHistoryData
{
    [DataField]
    public TaskResult Result { get; init; } = TaskResult.Completed;

    [DataField]
    public string? ActorName { get; init; } = default;

    [DataField]
    public TimeSpan Timestamp { get; init; } = TimeSpan.MinValue;

    [DataField(required: true)]
    public ProtoId<HeadquarterTaskPrototype> Task { get; init; } = string.Empty;

    public HeadquarterTaskHistoryData(HeadquarterTaskData task, TaskResult result, TimeSpan timestamp, string? actorName)
    {
        Task = task.Task;
        Result = result;
        ActorName = actorName;
        Timestamp = timestamp;
    }

    public enum TaskResult
    {
        Completed = 0,
        Closed = 1,
    }
}
