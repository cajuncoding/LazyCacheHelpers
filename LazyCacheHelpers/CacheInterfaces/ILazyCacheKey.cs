using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Class to define a strongly typed implementation for generating valid Cache Keys.
    /// NOTE: Use of this class is not explicitly required, but it is recommended to provide improved tracing of code that generates cache keys,
    ///         and also provide greater code safety, readability and improve single responsibility.
    /// </summary>
    public interface ILazyCacheKey
    {
        String GenerateKey();
    }
}
