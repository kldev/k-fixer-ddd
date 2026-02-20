using System.Text.Json;
using System.Text.Json.Serialization;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Extensions;


public static class HttpExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Reads the HTTP response content and deserializes it to T.
    /// Optionally logs status code and raw content to xUnit test output.
    /// </summary>
    public static async Task<T?> ReadWithJson<T>(
        this HttpResponseMessage response,
        ITestOutputHelper? output = null)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (output != null)
        {
            output.WriteLine($"Status: {response.StatusCode}");
            output.WriteLine($"Content: {content}");
        }

        if (string.IsNullOrWhiteSpace(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
}