
namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// Grouping Interface to define a sincle Interface for simplified caching Facades that can generally group
    /// couple Cache Key generation with Cache Policy generation (1 param vs multiple)
    /// </summary>
    public interface ILazyCacheParams : ILazyCacheKey, ILazyCachePolicy
    {
    }
}
