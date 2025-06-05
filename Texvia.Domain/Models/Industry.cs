using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texvia.Domain.Models
{
    public class Industry
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string SubName { get; set; } = null!;
        public string Description { get; set; } = null!; 
        
        public ICollection<Contact> Contacts = new HashSet<Contact>();
    }
}
