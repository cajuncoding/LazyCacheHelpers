# LazyCacheHelpers
Library for leveraging the power of Lazy<T> for caching at all layers of an application with support for both 
Sync & Async Lazy operations to maximize server utilization and performance!

It also supports changing the underlying Cache Repository with different implementations via 
ILazyCacheRepository implementation. But the default implementation using .Net MemoryCache is 
implemented (via default ICacheRepository implementation as LazyDotNetMemoryCacheRepository) 
to enable working with MemoryCache with greatly simplified support for self-populating (Lazy) initialization.
This implementation will work for the vast majority of medium or small projects; but this flexibility allows
for migrating to distributed caches and other cache storage mechanisms easier in the future.
    
The use of Lazy&lt;T&gt;, for loading/initializing of data, facilitates a self-populating cache (also known as 
a blocking cache), so that even if many requests, for the same cached data, are triggered at the exact same 
time, no more than one thread will ever perform the work, dramatically decreasing server utilization under high load.


To clarify, what this means is that if many requests for the cached data are submitted at or near the same time 
then only one thread executes the long running process while all other requests will immediately benefit from 
the loaded data once it is ready. For example, if the long running process takes 3 seconds to complete and 10 more requests come in after 2 seconds, then all
of the new requests will benefit from the performance of the self-populating/blocking cache after waiting for only 1 second!

The importance of the behavior becomes much more valuable as the load increases and espeically for processes
that can take exhorbitant amounts of time (10 seconds, 30 seconds, etc.)!

This class provides a completely ThreadSafe cache with Lazy loading/initialization capability in an easy to use
 implementation that can work at all levels of an application (classes, controllers, etc.).

#### LazyCacheHelpers.ConfigurationManagement
This is an extension package for the LazyCacheHelpers Library to provide easy to use helpers to read cache configuration
values from App.Config or Web.config (e.g. AppSettings) files using System.Configuration package.  These helpers
make working with configuration and many helpful scenarios much easier. It has built in support for enabling/disabling
configuration values (without deleting them) which also works in tandem with dynamic fallback from highly 
specific/specialized configuration values (e.g. method level) to generalized config values (e.g. class, or 
controller, or global) much easier to implement.

##### Breaking Change:
To improve compatibility the built in helpers for processing AppSettings configuration values for cache policy time-to-live values
has now been broken out into a separate package. This is a breaking change from v1.0.0.1; but can be easily fixed by renaming
prior references to `LazyCachePolicy` static helper to now use `LazyCachePolicyFromConfig` static helper. Otherwise the signatures
are identical.


## Release Notes:
### v1.0.0.1
 - Initial nuget release for .Net Framework.

### v1.0.2
- Refactored as .Net Standard v2.0 compatible Library for greater compatibility
- Removed dependency on System.ConfigurationManagement; Because of this there is a **breaking change** if the LazyCachePolicy helper overloads that dynamically ready from AppSettings were used. 
  - The helpers can be restored by adding LazyCacheHelpers.ConfigurationManagement extension package and renaming all calls to LazyCachePolicy static helper to now use LazyCachePolicyFromConfig static helper.
- Now fully supported as a .Net Standard 2.0 library (sans Configuration reader helpers) whereby you can specify the timespan directly for Cache Policy initialization.


## Nuget Package
To use behaviors in your project, add the [LazyCacheHelpers NuGet package](https://www.nuget.org/packages/LazyCacheHelpers/) to your project.

To use the powerful helpers for dynamically reading Cache Policy TTL values from AppSettings, add the [LazyCacheHelpers.ConfigurationManagement NuGet package](https://www.nuget.org/packages/LazyCacheHelpers.ConfigurationManager/) to your project.
 - Extension package for the LazyCacheHelpers Library to provide easy to use helpers to read cache configuration
values from App.Config or Web.config files using System.Configuration; making things like enabling/disabling and dynamic fallback from specialized to generalized config values much easier to implement.

## Usage:
It's as easy as . . .

**Synchronous Caching:**
```csharp
private static readonly TimeSpan CacheFiveMinutesTTL = TimeSpan.FromMinutes(5);

function ComplexData GetComplexData(string variable)
{
	return DefaultLazyCache.GetOrAddFromCache($"CacheKey::{variable}", 
		() => {
			//Do any work you want here, or call other services/helpers/etc...
			return BuildVeryComplexData(variable);
		},
		LazyCachePolicy.NewAbsoluteExpirationPolicy(CacheFiveMinutesTTL)
	);
}
```

**Aynchronous Caching:**
```csharp
private static readonly TimeSpan CacheFiveMinutesTTL = TimeSpan.FromMinutes(5);

function async Task<ComplexData> GetComplexDataAsync(string variable)
{
	return await DefaultLazyCache.GetOrAddFromCache($"CacheKey::{variable}", 
		async () => {
			//Do any Async work you want here, or call other services/helpers/etc...
			return await BuildVeryComplexDataAsync(variable);
		},
		LazyCachePolicy.NewAbsoluteExpirationPolicy(CacheFiveMinutesTTL)
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
