using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ConcurrentCaching.Test
{
    public class ConcurrentMemoryCacheTests
    {
        [Fact]
        public async void FetchWithSingleEntry()
        {
            var cacheEntryOptions = new MemoryCacheOptions();
            var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));
            var results = new List<int>();
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => results.Add(cache.GetOrCreate<int>("key", entry => i))));
            }

            await Task.WhenAll(tasks);

            var cachedItem = results[0];
            Assert.All(results, res => Assert.Equal(cachedItem, res));
        }

        [Fact]
        public async void FetchAsyncWithSingleEntry()
        {
            var cacheEntryOptions = new MemoryCacheOptions();
            var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));
            var results = new List<int>();
            var tasks = new List<Task<int>>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => cache.GetOrCreateAsync("key", entry => Task.FromResult(1))));
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < tasks.Count; i++)
            {
                object res = await tasks[i];
                results.Add((int)res);
            }

            var cachedItem = results[0];
            Assert.All(results, res => Assert.Equal(cachedItem, res));
        }

        [Fact]
        public async void FetchWithMultipleEntries()
        {
            var cacheEntryOptions = new MemoryCacheOptions();
            var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var res = cache.GetOrCreate($"key {i}", entry => i);
                    Assert.Equal(i, res);
                }));
            }
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async void FetchAsyncWithMultipleEntries()
        {
            var cacheEntryOptions = new MemoryCacheOptions();
            var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var res = await cache.GetOrCreateAsync($"key {i}", entry => Task.FromResult(i));
                    Assert.Equal(i, res);
                }));
            }
            await Task.WhenAll(tasks);
        }
    }
}
