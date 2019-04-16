# LazyCacheHelpers
Library for leveraging the power of Lazy<T> for caching at all layers of an application with support for both Sync & Async
Lazy operations to maximize server utilization and performance!

    
The use of Lazy<T> for loading/initializing of data facilitates a self-populating cache, so that the long running processes are never executed more than once.  And, even if they are triggered at the exact same time, no more than one thread will ever perform the work, dramatically decreasing server utilization under high load.

This class provides a completely ThreadSafe cache with Lazy loading/initialization capability in an easy to use implementation that can work at all levels of an application (classes, controllers, etc.). It also supports changing the underlying Cache Repository with different implementations via ILazyCacheRepository implementation.

NOTE: A default implementation using .Net MemoryCache is implemented (via default ICacheRepository implementation as LazyDotNetMemoryCacheRepository) to enable working with MemoryCache with greatly simplified support for self-populating (Lazy) initialization.
