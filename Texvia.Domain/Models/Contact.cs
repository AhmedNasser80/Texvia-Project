using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Texvia.Domain.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Mobile { get; set; }
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Region { get; set; }
        public string? Message { get; set; }

        [ForeignKey("Industry")]
        public int IndustryId { get; set; }
        public Industry Industry { get; set; }

        [ForeignKey("Solution")]
        public int SolutionId { get; set; }
        public Solution Solution { get; set; }
    }
}
