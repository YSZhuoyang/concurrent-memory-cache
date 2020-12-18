# concurrent-memory-cache

A simple wrapper over MemoryCache to prevent concurrent cache missing.

## Purpose

The Microsoft [MemoryCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0) API provides the ability to cache locally, however it does not guarantee the [atomicity](https://github.com/dotnet/runtime/issues/36499) when multiple threads call `GetOrCreate()` with the same entry (at the moment this was written), which can lead to multiple cache missing and duplicated data fetching.

## How it works

It was implemented based on the same idea used by [ConcurrentDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=netcore-3.1), which divides the Hashmap into a fixed number of segments (16 by default). Each of them is protected by a segment lock. Write operations that fall into the same segment are sequentialized, while write operations falling into different segments are handled concurrently. All read operations are lock-free.

## How to use

1. Install package:

       dotnet add package ConcurrentCaching

2. Inject caching service:

       services.AddMemoryCache(options =>
       {
           //... Setup e.g. max cache limit for LRU
       });
       services.AddSingleton<IConcurrentMemoryCache, ConcurrentMemoryCache>();

3. Fetch data from cache:

       var item = cache.GetOrCreate<TItem>("<key>", entry =>
       {
           // Fetch data from elsewhere and return it ...
       });

       var item = await cache.GetOrCreateAsync<TItem>("<key>", async entry =>
       {
           // Fetch data from elsewhere and return it ...
       });
