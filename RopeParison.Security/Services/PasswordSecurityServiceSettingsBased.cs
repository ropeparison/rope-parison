using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using RopeParison.Security;
using RopeParison.Security.Model;

namespace RopeParison.Security.Services
{
    public class PasswordSecurityServiceSettingsBased : IPasswordSecurityService
    {
        //This is developed to avoid paying for another db for a small amount of security info.
        
        //Actual live passwords are stored in the Azure App Settings.
        //Note that currently (24/07/14) double underscore has to be used rather than colon for Linux compatibility. E.g. Key = Security__Password__Verify, Value = ExampleVerifyPassword

        IConfiguration _configuration;

        public PasswordSecurityServiceSettingsBased(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CheckPassword_Verify(string proposedPassword)
        {
            string? password = _configuration["Security:Password:Verify"];

            return PasswordSecurityService.CheckMatch(proposedPassword, password);
        }

        public bool CheckPassword_Edit(string proposedPassword)
        {
            string? password = _configuration["Security:Password:ApproveEdit"];

            return PasswordSecurityService.CheckMatch(proposedPassword, password);
        }

        public bool CheckPassword_Delete(string proposedPassword)
        {
            string? password = _configuration["Security:Password:Delete"];

            return PasswordSecurityService.CheckMatch(proposedPassword, password);
        }
    }

}
