using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BiometricService
{
    [BsonIgnoreExtraElements] // ignore other fields in MongoDB
    public record Student(
        [property: BsonId]
        [property: BsonRepresentation(BsonType.ObjectId)]
        string? Id,
        [property: BsonElement("fingerprints")] Dictionary<string, string>? Fingerprints // base64 templates
    )
    {
        public IEnumerable<string> GetSelectedFingerprintTemplates()
        {
            if (Fingerprints == null)
                yield break;

            if (Fingerprints.TryGetValue("f1", out var f1) && !string.IsNullOrWhiteSpace(f1))
                yield return f1;
            else if (Fingerprints.TryGetValue("f$1", out var f1a) && !string.IsNullOrWhiteSpace(f1a))
                yield return f1a;

            if (Fingerprints.TryGetValue("f6", out var f6) && !string.IsNullOrWhiteSpace(f6))
                yield return f6;
            else if (Fingerprints.TryGetValue("f$6", out var f6a) && !string.IsNullOrWhiteSpace(f6a))
                yield return f6a;
        }
    }
}
