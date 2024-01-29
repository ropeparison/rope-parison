using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeParison.Security.Model;

namespace RopeParison.Security.Model
{
    public class Password
    {
        public int PasswordId { get; set; }

        public PasswordCategory PasswordCategory { get; set; }
        public string? PasswordString { get; set; }
    }
}
