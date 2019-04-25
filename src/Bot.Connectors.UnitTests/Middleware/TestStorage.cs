using Microsoft.Bot.Builder;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Middleware
{
    public class TestStorage : IStorage
    {
        private readonly ConcurrentDictionary<string, object> _dataStore;

        public TestStorage()
        {
            _dataStore = new ConcurrentDictionary<string, object>();
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            keys.ToList().ForEach(k =>
            {
                object value;
                _dataStore.TryRemove(k, out value);
            });

            return Task.CompletedTask;
        }

        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            var resultStore = new Dictionary<string, object>();
            keys.ToList().ForEach(k =>
            {
                object value;
                if (_dataStore.TryGetValue(k, out value))
                {
                    resultStore.Add(k, value);
                };
            });

            return Task.FromResult<IDictionary<string, object>>(resultStore);
        }

        public Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            changes.ToList().ForEach(c => _dataStore.AddOrUpdate(c.Key, c.Value, (key, value) => c.Value));
            return Task.CompletedTask;
        }
    }
}
