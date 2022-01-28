using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ConcurrentCaching
{
    public interface IConcurrentMemoryCache
    {
        TItem GetOrCreate<TItem>(string key, Func<ICacheEntry, TItem> factory);

        Task<TItem> GetOrCreateAsync<TItem>(string key, Func<ICacheEntry, Task<TItem>> factory);
    }

    public class ConcurrentMemoryCache : IConcurrentMemoryCache
    {
        private readonly object[] segments;
        private readonly IMemoryCache _localCache;

        public ConcurrentMemoryCache(IMemoryCache localCache, int numSegments = 16)
        {
            _localCache = localCache;

            segments = new object[numSegments];
            for (int i = 0; i < numSegments; i++) segments[i] = new object();
        }

        public Task<TItem> GetOrCreateAsync<TItem>(string key, Func<ICacheEntry, Task<TItem>> factory)
        {
            return (Task<TItem>)GetOrCreate(key, (Func<ICacheEntry, object>)(key => factory(key)));
        }

        public TItem GetOrCreate<TItem>(string key, Func<ICacheEntry, TItem> factory)
        {
            return (TItem)GetOrCreate(key, (Func<ICacheEntry, object>)(key => factory(key)));
        }

        private object GetOrCreate(string key, Func<ICacheEntry, object> factory)
        {
            if (_localCache.TryGetValue(key, out var value)) return value;

            int segmentNo = getSegmentNo(key);
            lock (segments[segmentNo])
            {
                return _localCache.GetOrCreate(key, factory);
            }
        }

        /// <summary>
        /// Get rid of the sign bit and obtain segment no.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int getSegmentNo(string key)
        {
            return (key.GetHashCode() & 0x7fffffff) % segments.Length;
        }
    }
}
