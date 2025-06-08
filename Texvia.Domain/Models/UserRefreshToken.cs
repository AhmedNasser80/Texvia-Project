using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texvia.Domain.Models
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
    }
}
