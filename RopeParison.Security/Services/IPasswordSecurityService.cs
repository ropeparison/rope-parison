using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeParison.Security.Services
{
    public interface IPasswordSecurityService
    {
        bool CheckPassword_Verify(string proposedPassword);
        bool CheckPassword_Edit(string proposedPassword);
        bool CheckPassword_Delete(string proposedPassword);
    }
}
