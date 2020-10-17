using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ConcurrentCaching
{
    public interface IConcurrentMemoryCache
    {
        object GetOrCreateItem(string key, Func<ICacheEntry, object> factory);
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

        public object GetOrCreateItem(string key, Func<ICacheEntry, object> factory)
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

    public static class ConcurrentMemoryCacheExtension
    {
        public static TItem GetOrCreate<TItem>(this IConcurrentMemoryCache cache, string key, Func<ICacheEntry, TItem> factory)
        {
            return (TItem)cache.GetOrCreateItem(key, key => factory(key));
        }

        public static Task<TItem> GetOrCreateAsync<TItem>(this IConcurrentMemoryCache cache, string key, Func<ICacheEntry, Task<TItem>> factory)
        {
            return (Task<TItem>)cache.GetOrCreateItem(key, key => factory(key));
        }
    }
}
