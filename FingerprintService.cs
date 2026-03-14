using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BiometricService
{
    public class FingerprintService
    {
        private readonly FingerprintTemplateStore _templateStore;
        private readonly ILogger<FingerprintService> _logger;
        private readonly uint _matchThreshold;

        public FingerprintService(
            FingerprintTemplateStore templateStore,
            ILogger<FingerprintService> logger,
            IConfiguration configuration)
        {
            _templateStore = templateStore;
            _logger = logger;
            _matchThreshold = configuration.GetValue<uint?>("Fingerprint:MatchThreshold") ?? 2000;
        }

        public async Task<StudentMatchDto?> VerifyStudentAsync(string incomingTemplateBase64)
        {
            if (string.IsNullOrWhiteSpace(incomingTemplateBase64))
                return null;

            var incomingNormalized = TemplateUtils.NormalizeBase64(incomingTemplateBase64);
            if (incomingNormalized == null)
                return null;

            var incomingBytes = TemplateUtils.TryDecodeBase64(incomingNormalized);
            if (incomingBytes == null || incomingBytes.Length == 0)
                return null;

            var candidates = await _templateStore.GetCandidatesAsync();
            if (candidates.Count == 0)
                return null;

            _logger.LogInformation("VerifyFingerprint: templates loaded = {Count}", candidates.Count);

            uint bestScore = uint.MaxValue;
            StudentMatchDto? bestStudent = null;

            foreach (var candidate in candidates)
            {
                if (!DpfjNative.TryCompare(incomingBytes, candidate.FmdBytes, out var score))
                    continue;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestStudent = candidate.Student;
                    if (bestScore == 0)
                        break;
                }
            }

            if (bestStudent != null && bestScore <= _matchThreshold)
            {
                _logger.LogInformation("Best match: {Id} score={Score}", bestStudent.Id, bestScore);
                return bestStudent;
            }

            _logger.LogInformation("No match under threshold. BestScore={Score}", bestScore);
            return null;
        }
    }
}
