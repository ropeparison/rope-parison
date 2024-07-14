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

                return CheckMatch(proposedPassword, passwordAdd.PasswordString);
            }
        }

        public bool CheckPassword_Edit(string proposedPassword)
        {
            using (var db = _dbSecurityContextFactory.CreateDbContext())
            {
                Password passwordEdit = db.Passwords.Where(x => (int)x.PasswordCategory == (int)PasswordCategory.ApproveEdit).First();

                return CheckMatch(proposedPassword, passwordEdit.PasswordString);
            }
        }

        public bool CheckPassword_Delete(string proposedPassword)
        {
            using (var db = _dbSecurityContextFactory.CreateDbContext())
            {
                Password passwordDelete = db.Passwords.Where(x => (int)x.PasswordCategory == (int)PasswordCategory.Delete).First();

                return CheckMatch(proposedPassword, passwordDelete.PasswordString);
            }
        }

        public static bool CheckMatch(string? proposedPassword, string? password)
        {
            if (proposedPassword == null || password == null)
            {
                return false; 
            }

            return string.Equals(proposedPassword, password);
        }
    }

}
