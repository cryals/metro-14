using Content.Shared.HeadquarterTask.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.HeadquarterTask;

/// <summary>
/// Stores an active headquarters task instance.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct HeadquarterTaskData
{
    /// <summary>
    /// Prototype backing this task.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<HeadquarterTaskPrototype> Task { get; init; } = string.Empty;

    public HeadquarterTaskData(HeadquarterTaskPrototype task)
    {
        Task = task.ID;
    }
}
