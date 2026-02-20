namespace Common.Toolkit.ResultPattern;

/// <summary>
/// Unit type representing "no value". Use as Result&lt;Empty&gt; for void operations.
/// </summary>
public readonly struct Empty
{
    public static readonly Empty Value = default;
}