using System.Text.Json.Serialization;

namespace BiometricService
{
    public record StudentMatchDto(
        [property: JsonPropertyName("_id")] string Id
    );
}
