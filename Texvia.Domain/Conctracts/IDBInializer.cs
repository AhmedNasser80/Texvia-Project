using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texvia.Domain.Conctracts
{
    public interface IDBInializer
    {
        public Task InializeAsync();
    }
}
