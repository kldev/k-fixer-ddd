using Newtonsoft.Json;

namespace Common.Toolkit.Json;

public static class JsonUtil
{
    private static readonly JsonSerializerSettings ReadSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Populate
    };

    /// <summary>
    /// Deserializes a JSON string into an object of type T.
    /// Ignores null values, populates default values.
    /// </summary>
    public static T? Read<T>(string input)
    {
        return JsonConvert.DeserializeObject<T>(input, ReadSettings);
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    public static string Write(object data)
    {
        return JsonConvert.SerializeObject(data);
    }

    /// <summary>
    /// Serializes an object to a formatted (indented) JSON string.
    /// </summary>
    public static string WritePretty(object data)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }
}