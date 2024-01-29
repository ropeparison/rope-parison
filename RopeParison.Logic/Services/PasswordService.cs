using RopeParison.Data;
using RopeParison.Data.Model;
using RopeParison.Data.Services;
using RopeParison.Protocol;

namespace RopeParison.Logic.Services
{

    public interface IPasswordService
    {
        bool CheckPassword_Verify(string proposedPassword);
        bool CheckPassword_Edit(string proposedPassword);
        bool CheckPassword_Delete(string proposedPassword);
    }

    public class PasswordService : IPasswordService
    {
        private readonly IPasswordSecurityService _passwordSecurityService;

        public PasswordService(IPasswordSecurityService passwordSecurityService)
        {
            _passwordSecurityService = passwordSecurityService;
        }
        //-------------------------------------------------------------------------

        public bool CheckPassword_Verify(string proposedPassword)
        {
            return _passwordSecurityService.CheckPassword_Verify(proposedPassword);
        }

        public bool CheckPassword_Edit(string proposedPassword)
        {
            return _passwordSecurityService.CheckPassword_Edit(proposedPassword);
        }

        public bool CheckPassword_Delete(string proposedPassword)
        {
            return _passwordSecurityService.CheckPassword_Delete(proposedPassword);
        }
    }
}
