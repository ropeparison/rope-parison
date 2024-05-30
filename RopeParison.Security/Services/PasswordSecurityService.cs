using Microsoft.EntityFrameworkCore;
using RopeParison.Security;
using RopeParison.Security.Model;

namespace RopeParison.Security.Services
{
    public class PasswordSecurityService : IPasswordSecurityService
    {
        private IDbContextFactory<SecurityContext> _dbSecurityContextFactory;

        public PasswordSecurityService(IDbContextFactory<SecurityContext> dbSecurityContextFactory)
        {
            _dbSecurityContextFactory = dbSecurityContextFactory;
        }

        public bool CheckPassword_Verify(string proposedPassword)
        {
            using (var db = _dbSecurityContextFactory.CreateDbContext())
            {
                Password passwordAdd = db.Passwords.Where(x => (int)x.PasswordCategory == (int)PasswordCategory.Verify).First();

                var match = false;

                if (proposedPassword == passwordAdd.PasswordString)
                {
                    match = true;
                }
                
                return match;
            }
        }

        public bool CheckPassword_Edit(string proposedPassword)
        {
            using (var db = _dbSecurityContextFactory.CreateDbContext())
            {
                Password passwordEdit = db.Passwords.Where(x => (int)x.PasswordCategory == (int)PasswordCategory.ApproveEdit).First();

                var match = false;

                if (proposedPassword == passwordEdit.PasswordString)
                {
                    match = true;
                }

                return match;
            }
        }

        public bool CheckPassword_Delete(string proposedPassword)
        {
            using (var db = _dbSecurityContextFactory.CreateDbContext())
            {
                Password passwordEdit = db.Passwords.Where(x => (int)x.PasswordCategory == (int)PasswordCategory.Delete).First();

                var match = false;

                if (proposedPassword == passwordEdit.PasswordString)
                {
                    match = true;
                }

                return match;
            }
        }
    }

}
