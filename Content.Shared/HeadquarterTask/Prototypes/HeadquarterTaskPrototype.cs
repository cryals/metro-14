using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.HeadquarterTask.Prototypes;

/// <summary>
/// Lightweight headquarters task prototype independent from cargo bounty mechanics.
/// </summary>
[Prototype]
public sealed partial class HeadquarterTaskPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Short player-facing task title.
    /// </summary>
    [DataField(required: true)]
    public LocId Name = string.Empty;

    /// <summary>
    /// Task description shown in the UI.
    /// </summary>
    [DataField]
    public LocId Description = string.Empty;

    /// <summary>
    /// Device/faction task pool group.
    /// </summary>
    [DataField]
    public ProtoId<HeadquarterTaskGroupPrototype> Group = "HeadquarterTaskDefault";
}
