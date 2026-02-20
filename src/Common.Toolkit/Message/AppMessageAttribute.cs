namespace Common.Toolkit.Message;

/// <summary>
/// Defines a user-facing message for a string constant code.
/// Applied to const string fields in static code classes.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class AppMessageAttribute : Attribute
{
    public string Message { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public AppMessageAttribute(string message)
    {
        Message = message;
    }
}