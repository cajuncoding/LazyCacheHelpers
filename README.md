# LazyCacheHelpers
Library for leveraging the power of Lazy<T> for caching at all layers of an application with support for both Sync & Async
Lazy operations to maximize server utilization and performance!

    
The use of Lazy&lt;T&gt; for loading/initializing of data facilitates a self-populating cache, so that the long running processes are never executed more than once.  And, even if they are triggered at the exact same time, no more than one thread will ever perform the work, dramatically decreasing server utilization under high load.

This class provides a completely ThreadSafe cache with Lazy loading/initialization capability in an easy to use implementation that can work at all levels of an application (classes, controllers, etc.). It also supports changing the underlying Cache Repository with different implementations via ILazyCacheRepository implementation.

NOTE: A default implementation using .Net MemoryCache is implemented (via default ICacheRepository implementation as LazyDotNetMemoryCacheRepository) to enable working with MemoryCache with greatly simplified support for self-populating (Lazy) initialization.

## Nuget Package
To use behaviors in your project, add the [LazyCacheHelpers NuGet package](https://www.nuget.org/packages/LazyCacheHelpers/) to your project.

## Usage:
It's as easy as . . .

**Synchronous Caching:**
```
private static readonly string _cacheTTLConfigKey = "CacheTTL.LongRunningProcess";

function ComplexData GetComplexData(string variable)
{
	return DefaultLazyCache.GetOrAddFromCache($"CacheKey::{variable}", 
		() => {
			return BuildVeryComplexData(variable);
		},
		LazyCachePolicy.NewAbsoluteExpirationPolicy(_cacheTTLConfigKey)
	);
}
```

**Aynchronous Caching:**
```
private static readonly string _cacheTTLConfigKey = "CacheTTL.LongRunningProcess";

function async Task<ComplexData> GetComplexDataAsync(string variable)
{
	return await DefaultLazyCache.GetOrAddFromCache($"CacheKey::{variable}", 
		async () => {
			return await BuildVeryComplexDataAsync(variable);
		},
		LazyCachePolicy.NewAbsoluteExpirationPolicy(_cacheTTLConfigKey)
	);
}
```



```
/*
MIT License

Copyright (c) 2018

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
```
