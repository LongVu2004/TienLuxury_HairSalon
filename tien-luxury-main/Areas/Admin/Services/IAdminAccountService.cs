using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Areas.Admin.Services
{
    public interface IAdminAccountService
    {
        public bool ChangePassword(ObjectId idAccountToChange, string newPassword, string oldPassword);

        public bool CheckAccount(AdminAccount adminAccount);

        public void CreateAccount(AdminAccount newAdminAccount);

        public AdminAccount FindAccountByID(ObjectId adminID);

        public ObjectId FindIdByUserName(string userName);
    }

}
