using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Texvia.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }  // الاسم الحقيقي للمستخدم
    }
}
