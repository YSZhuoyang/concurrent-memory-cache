# concurrent-memory-cache

A simple wrapper over MemoryCache to prevent concurrent cache missing.

## Purpose

The Microsoft [MemoryCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-3.1) API provides the ability to cache locally, however it does not stop multiple threads concurrently calling `GetOrCreate()` method (at the moment this was written), which can lead to multiple cache missing and duplicated data fetching.

## How it works

It was implemented with the same idea used by [ConcurrentDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=netcore-3.1), which divides the Hashmap into a fixed number of segments (16 by default). Each of them is protected by a segment lock. Write operations that fall into the same segment are sequentialized, while write operations falling into different segments are handled concurrently. All read operations are lock-free.

## How to use

    var cacheEntryOptions = new MemoryCacheOptions(); // Setup e.g. max cache limit for LRU
    var cache = new ConcurrentMemoryCache(new MemoryCache(cacheEntryOptions));

    var item = cache.GetOrCreate<TItem>("<key>", entry =>
    {
        // Fetch data from elsewhere and return it ...
    });

    var item = await cache.GetOrCreateAsync<TItem>("<key>", async entry =>
    {
        // Fetch data from elsewhere and return it ...
    });
