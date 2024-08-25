using System.Text.Json;

namespace monument.api.client
{
    public partial class MonumentApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
        {
            settings.PropertyNameCaseInsensitive = true;
            settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            settings.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            settings.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        }

    }
}
