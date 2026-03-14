using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BiometricService
{
    public record CandidateTemplate(StudentMatchDto Student, byte[] FmdBytes);

    public class FingerprintTemplateStore
    {
        private readonly MongoService _mongoService;
        private readonly ILogger<FingerprintTemplateStore> _logger;
        private readonly SemaphoreSlim _reloadLock = new(1, 1);

        private List<CandidateTemplate> _cache = new();
        private DateTime _lastLoadedUtc = DateTime.MinValue;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

        public FingerprintTemplateStore(MongoService mongoService, ILogger<FingerprintTemplateStore> logger)
        {
            _mongoService = mongoService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CandidateTemplate>> GetCandidatesAsync(bool forceReload = false)
        {
            if (!forceReload && _cache.Count > 0 && DateTime.UtcNow - _lastLoadedUtc < _ttl)
                return _cache;

            await _reloadLock.WaitAsync();
            try
            {
                if (!forceReload && _cache.Count > 0 && DateTime.UtcNow - _lastLoadedUtc < _ttl)
                    return _cache;

                var students = await _mongoService.GetStudentsWithFingerprintsAsync();
                var list = new List<CandidateTemplate>(students.Count * 2);

                foreach (var s in students)
                {
                    if (string.IsNullOrWhiteSpace(s.Id))
                        continue;
                    var studentDto = new StudentMatchDto(s.Id);

                    foreach (var tmpl in s.GetSelectedFingerprintTemplates())
                    {
                        var normalized = TemplateUtils.NormalizeBase64(tmpl);
                        if (normalized == null) continue;
                        var bytes = TemplateUtils.TryDecodeBase64(normalized);
                        if (bytes == null || bytes.Length == 0) continue;
                        list.Add(new CandidateTemplate(studentDto, bytes));
                    }
                }

                _cache = list;
                _lastLoadedUtc = DateTime.UtcNow;
                _logger.LogInformation("Template cache loaded: students={Students}, templates={Templates}",
                    students.Count, list.Count);

                return _cache;
            }
            finally
            {
                _reloadLock.Release();
            }
        }

    }
}
