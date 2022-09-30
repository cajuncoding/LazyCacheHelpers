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

This library provides a completely ThreadSafe cache with Lazy loading/initialization capability in an easy to use
 implementation that can work at all levels of an application (classes, controllers, etc.).

#### LazyCacheConfig Static Class
A set of static helpers that enable working with configuration in various helpful scenarios much easier. 
It provides mechanism to read Cache TTL values from configuration safely and easily. It helps when you want
to implement cache configurations that might fallback to general cache settings if very specific ones aren't defined, etc.

The key to using these is that you define (one time at app startup) the static configuration reader delegate function
that when given a cache key `string` it returns the a given cache TTL seconds (`string`; will be converted to `int` automatically).
This allows you to control where config values are read from but don't have to implement all of hte other helpful
looping for fallback values, etc. manually.

#### LazyCacheHelpers.ConfigurationManagement
This is an extension package for the LazyCacheHelpers Library to provide easy to use bootstrap method that will initialize
the LazyCacheConfig class to read cache configuration using ConfigurationManagement.AppSettings.

To use the ConfigurationManager you just have to run this, in you application startup, to initialize the config reader func/delegate:
`LazyCacheConfigurationManager.BootstrapConfigurationManager();`

#### Breaking Change for Reading Config values:
To improve compatibility the built in helpers for processing AppSettings configuration values and the LazyCacheConfig
ned to be initialized in your application startup process by defining the config reader func:
```csharp
LazyCacheConfig.BootstrapConfigValueReader(configKeyName => {
	
	... read your config value and return it as a string, or null if not found/undefined ...

});
```

OR use the ConfigurationManager Bootstrap helper above if you use AppSettings; it'll wire up a default reader for you (see below)!

## Release Notes:
### v1.2
- Add support for Clearing the Lazy Static In-memory Cache
  - *The wrapper for Lazy caching pattern for sync or async results based on the underlying ConcurrentDictionary<Lazy<T>>).*

### v1.1
 - Add support for Clearing the Cache and for getting the Cache Count; implemented for DefaultLazyCache as well as the Static In-memory caches.

### v1.0.4
- Restored LazyCacheConfig class capabilities for reading values dynamically from Configuration.
- Added support for Bootstrapping a Configuration Reader Func (Delegate) so that all reading of config values from the keys is completely dynamic now.
- Updated LazyCacheHelpers.ConfigurationManager library to now provide the Bootstrap method to initialize reading from AppSettings.

### v1.0.3
- Added new `LazyStaticInMemoryCache<>` class to make dyanimically caching data, that rarely or never changes, in memory 
in a high performance blocking (self-populating) cache using Lazy<> as the backing mechanism. This now works similar to
a normal `ConcurrentDictionary` but automatically wraps and unwraps all delegates in a Lazy<> to greatly simplify and enalbe
the use of this pattern more often and in more places.
   - It contains support for both Sync and Async value factories for those expensive I/O calls.
   - Examples use cases that benefit greatly from this are the often very expensive logic that loads data from 
   Reflection or reading values/configuration from Annotation Attributes; this helps mitigate the impact to runtime execution. 

### v1.0.2
- Refactored as .Net Standard v2.0 compatible Library for greater compatibility.
- Removed dependency on `System.ConfigurationManagement`; Because of this there is a **breaking change** in the LazyCachePolicy helper overloads that dynamically read values from AppSettings; but it is isolated to those helper overlaods only. 
  - The helpers can be restored by adding LazyCacheHelpers.ConfigurationManagement extension package and renaming all calls to LazyCachePolicy static helper to now use LazyCachePolicyFromConfig static helper.
- Now fully supported as a .Net Standard 2.0 library (sans Configuration reader helpers) whereby you can specify the timespan directly for Cache Policy initialization.

### v1.0.0.1
 - Initial nuget release for .Net Framework.

## Nuget Package
To use behaviors in your project, add the [LazyCacheHelpers NuGet package](https://www.nuget.org/packages/LazyCacheHelpers/) to your project.

To use the powerful helpers for dynamically reading Cache Policy TTL values from AppSettings, add the [LazyCacheHelpers.ConfigurationManagement NuGet package](https://www.nuget.org/packages/LazyCacheHelpers.ConfigurationManager/) to your project.
 - Extension package for the LazyCacheHelpers Library to provide easy to use helpers to read cache configuration
values from App.Config or Web.config files using System.Configuration; making things like enabling/disabling and dynamic fallback from specialized to generalized config values much easier to implement.

## Usage of LazyCache<> with Cache Policies for Data that changes:
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


## Usage of LazyStaticInMemoryCache<> for in-memory caching of data that rarely or never changegs:

The new `LazyStaticInMemoryCache<>` class makes it much easire to implement a lazy loading, blocking, in-memory cache of 
data that rarely or never changes. Enabling the use of caching patterns much more often with less code to maintain;
while also making the code easier to reason-about.  It also contains support for both Sync and Async value factories
for those expensive I/O processes that initialize data that rarely or never changes.

NOTE: It does support basic removal, but the `LazyStaticInMemoryCache<>` provides a patterm that is 
best used for data that never chagnes onces loaded/initialized.  In almost all cases for data that changes, the LazyCache<> above
with a cache expiration policy is the better pattern to use.

Key examples of the value of this is the, often expensive, loading of data from Reflection or reading values/configuration 
from Annotation Attributes; whereby this pattern migitages negative performance impacts at runtime.

```csharp
public class AttributeConfigReader
{
	[AttributeUsage(AttributeTargets.Class)]
	private class ConfigAttribute : Attribute
	{
		. . . implement your design time configuration properties . . . 
	}

	//By making the cache static it is now a global and thread-safe blocking cache; enabling only 
	//  one thread ever to complete the work, while any/all other threads/requests can benefit 
	//  immediatley when the work is completed!
	//NOTE: We are able to simply use the Class Type as the cache lookup Key.
	private static LazyStaticInMemoryCache<Type, Attribute> _lazyAttribConfigCache = new LazyStaticInMemoryCache<Type, Attribute>();

	public ConfigAttribute ReadConfigAttributeForClass<T>(T classWithConfig) where T : class
	{
		//Now using the static cache is very simple, following the same pattern as a ConcurrentDictionary<>
		// but without the need to apply the Lazy<> wrapper manually every time the pattern is implemented!
		//NOTE: The process in the value factory may implement any expensive processing including but not 
		//      limited to the use use Reflection to get access to values, an Attribute, or any 
		//      other expensive operation...
		//NOTE: Beacuse this is a Lazy loaded blocking cache you don't providee the value, you instead
		//NOTE: Beacuse this is a Lazy loaded blocking cache you don't providee the value, you instead
		//      provide a value factory method that will be executed to return and initialize the value.
		//      The key concept here is that the logic will only ever be executed at-most one time, no matter
		//      how many or how fast multiple (e.g. hundreds/thousands) threads/reqeuests come in for that same data!
		var cacheResult = _lazyAttribConfigCache.GetOrAdd(typeof(T), (key) =>
		{
			//NOTE: If an Exception occurs then the result will not be cached, only value values
			//      will be cached (e.g. a safe response of null will be cached).
			var configAttribute = GetConfigAttributeInternal();
			return configAttribute;
		});

		return cacheResult;
	}

	//Helper method to make the above code more readable, but as an internal private method
	// is not used outside the cache initialization/loading process.
	//NOTE: This is the expensive work that we would not want to run on every call to get a Classes
	//      configuration due to the Reflection invocations at runtime.
	private ConfigAttribute GetConfigAttributeInternal(Type classType)
	{
		var configAttribute = Attribute.GetCustomAttribute(classType, typeof(ConfigAttribute)) as ConfigAttribute;
		return configAttribute;
	}
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
