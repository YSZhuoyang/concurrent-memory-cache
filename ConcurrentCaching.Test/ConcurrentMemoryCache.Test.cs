using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ConcurrentCaching.Test
{
    public class ConcurrentMemoryCacheTests
    {
        [Fact]
        public async void BenchmarkConcurrentFetchWithDifferentEntries()
        {
            var cacheEntryOptions = new MemoryCacheOptions();
            var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));
            var tasks = new List<Task>();

            var timer = Stopwatch.StartNew();

            for (var i = 0; i < 200000; i++)
            {
                tasks.Add(Task.Run(() => cache.GetOrCreateAsync<int>(Guid.NewGuid().ToString(), async entry =>
                {
                    await Task.Delay(400);
                    return i;
                })));
            }

            await Task.WhenAll(tasks);

            timer.Stop();

            Console.WriteLine(timer.Elapsed.TotalMilliseconds);

            Assert.True(timer.Elapsed.TotalMilliseconds < 10000);
        }

        [Fact]
        public async void FetchWithSingleEntry()
        {
            var cacheEntryOptions = new MemoryCacheOptions();
            var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));
            var results = new List<int>();
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => results.Add(cache.GetOrCreate<int>("key1", entry => i))));
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
                tasks.Add(Task.Run(() => cache.GetOrCreateAsync("key2", async entry =>
                {
                    await Task.Delay(100 - i * 5);
                    return i;
                })));
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
                    var res = cache.GetOrCreate($"key1-{i}", entry => i);
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
                    var res = await cache.GetOrCreateAsync($"key2-{i}", entry => Task.FromResult(i));
                    Assert.Equal(i, res);
                }));
            }
            await Task.WhenAll(tasks);
        }
    }
}
