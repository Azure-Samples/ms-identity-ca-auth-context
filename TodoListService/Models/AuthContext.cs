using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListService.Models
{
    public class AuthContext
    {
        public string TenantId { get; set; }
        public string AuthContextType { get; set; }
        public string AuthContextValue { get; set; }
        public string Operation { get; set; }
    }
}
