using TenderScanner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderScanner.Infrastracture
{
    public interface IParser<T>
        where T : class
    {
        Task<IEnumerable<T>> GetAsync();
    }
}
