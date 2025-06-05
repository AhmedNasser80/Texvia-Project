using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texvia.Domain.Models
{
    public class Solution
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string SubName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<SolutionProviders> SolutionProviders { get; set; } = new HashSet<SolutionProviders>();
        public ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    }
}
