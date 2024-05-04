namespace Marsey.Game.Patches.Marseyports.Attributes;

/// <summary>
/// Target any engine version
/// </summary>
/// <remarks>Incompatible with BackportTargetEngine, BackportTargetEngineBefore, and BackportTargetEngineAfter</remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BackportTargetEngineAny : Attribute
{
    public BackportTargetEngineAny()
    {
        Type targetType = GetType();
        if (IsDefined(targetType, typeof(BackportTargetEngine)) ||
            IsDefined(targetType, typeof(BackportTargetEngineBefore)) ||
            IsDefined(targetType, typeof(BackportTargetEngineAfter)))
            throw new InvalidOperationException("Cannot apply BackportTargetAnyEngine with BackportTargetEngine, BackportTargetEngineBefore, or BackportTargetEngineAfter attributes.");
    }
}

/// <summary>
/// Target specific engine version
/// </summary>
/// <remarks>Mutually exclusive with BackportTargetEngineUntil and BackportTargetEngineAfter</remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BackportTargetEngine : Attribute
{
    public Version Ver { get; }

    public BackportTargetEngine(string ver)
    {
        Ver = new Version(ver);

        Type targetType = GetType();
        if (IsDefined(targetType, typeof(BackportTargetEngineBefore)) || IsDefined(targetType, typeof(BackportTargetEngineAfter)))
            throw new InvalidOperationException("Cannot apply BackportTargetEngine with BackportTargetEngineBefore or BackportTargetEngineAfter attributes.");
    }
}

/// <summary>
/// Target engine versions below and including this value
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BackportTargetEngineBefore : Attribute
{
    public Version Ver { get; }

    public BackportTargetEngineBefore(string ver)
    {
        Ver = new Version(ver);

        Type targetType = GetType();
        if (GetCustomAttribute(targetType, typeof(BackportTargetEngineAfter)) is BackportTargetEngineAfter afterAttr && afterAttr.Ver >= Ver)
            throw new InvalidOperationException("The version in BackportTargetEngineBefore must be greater than the version in BackportTargetEngineAfter.");
    }
}

/// <summary>
/// Target engine versions above and including this value
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BackportTargetEngineAfter : Attribute
{
    public Version Ver { get; }

    public BackportTargetEngineAfter(string ver)
    {
        Ver = new Version(ver);

        Type targetType = GetType();
        if (GetCustomAttribute(targetType, typeof(BackportTargetEngineBefore)) is BackportTargetEngineBefore untilAttr && untilAttr.Ver <= Ver)
            throw new InvalidOperationException("The version in BackportTargetEngineAfter must be less than the version in BackportTargetEngineBefore.");
    }
}

/// <summary>
/// Target specific content packs with ID matching this
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BackportTargetFork : Attribute
{
    public string ForkID { get; }

    public BackportTargetFork(string fork)
    {
        ForkID = fork;
    }
}
