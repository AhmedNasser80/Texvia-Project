using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texvia.Domain.Models
{
    public class SolutionProviders
    {
        public int Id { get; set; }

        [ForeignKey("Solution")]
        public int SolutionId { get; set; }
        public Solution Solution { get; set; } = null!;
        public string ProvideName { get; set; } = null!;
    }
}
