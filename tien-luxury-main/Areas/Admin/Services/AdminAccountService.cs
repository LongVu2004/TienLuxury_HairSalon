using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Areas.Admin.Services
{
    public class AdminAccountService(DBContext hairSalonDbContext) : IAdminAccountService
    {
        private readonly DBContext _hairSalonDbContext = hairSalonDbContext;

        public AdminAccount FindAccountByID(ObjectId adminID)
            => _hairSalonDbContext.AdminAccounts.FirstOrDefault(account => account.ID == adminID);

        public ObjectId FindIdByUserName(string userName)
            => _hairSalonDbContext.AdminAccounts.FirstOrDefault(account => account.Username == userName).ID;

        public bool ChangePassword(ObjectId idAccountToChange, string newPassword, string oldPassword)
        {
            AdminAccount accountToChange = FindAccountByID(idAccountToChange);

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, accountToChange.Password))
            {
                return false;
            }

            accountToChange.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _hairSalonDbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_hairSalonDbContext.ChangeTracker.DebugView.LongView);

            _hairSalonDbContext.SaveChanges();
            return true;
        }
                
        public bool CheckAccount(AdminAccount checkedAccount)
        {
            AdminAccount accountToCheck = _hairSalonDbContext.AdminAccounts.FirstOrDefault(account => account.Username == checkedAccount.Username);

            if (accountToCheck == null)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(checkedAccount.Password, accountToCheck.Password) ? true : false; 
        }

        public void CreateAccount(AdminAccount newAdminAccount)
        {
            string hashPassword = BCrypt.Net.BCrypt.HashPassword(newAdminAccount.Password);
            newAdminAccount.Password = hashPassword;

            _hairSalonDbContext.AdminAccounts.Add(newAdminAccount);

            _hairSalonDbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_hairSalonDbContext.ChangeTracker.DebugView.LongView);

            _hairSalonDbContext.SaveChanges();
        }
    }
}
