using Robust.Shared.Prototypes;

namespace Content.Shared.HeadquarterTask.Prototypes;

/// <summary>
/// Used to categorize headquarters tasks per faction/device.
/// </summary>
[Prototype]
public sealed partial class HeadquarterTaskGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
