using MinesweeperApp.DatabaseServices;
using MinesweeperApp.Models;

namespace MinesweeperApp.BusinessServices
{
    /// <summary>
    /// This business service class is designed to manage processing logic for user registration.
    /// </summary>
    public class RegistrationBusinessService
    {
        //Database service object for users
        private IUserDAO userDAO;

        public RegistrationBusinessService(IUserDAO userDAO)
        {
            this.userDAO = userDAO;
        }

        /// <summary>
        /// user object is added them to the database. Does not create login status.
        /// </summary>
        /// <param name="user">The complete user object to add to the database.</param>
        /// <returns>true if the user was added, false otherwise.</returns>
        public bool RegisterUser(User user)
        {
            return userDAO.Add(user);
        }

        /// <summary>
        /// checks for a if same user name exists in the database.
        /// </summary>
        /// <param name="userName">The user name to match the database against.</param>
        /// <returns>true if a user has registered the given user name already, false otherwise</returns>
        public bool CheckUsernameAvailability(string userName)
        {
            if (userDAO.GetIdFromUsername(userName) == -1)
                return true;
            return false;
        }

        /// <summary>
        /// checks for a same email exists in the system.
        /// </summary>
        /// <param name="email">The email string to search the database for</param>
        /// <returns>true if another user has already registered with the given email, flase otherwise.</returns>
        public bool CheckEmailAvailability(string email)
        {
            if (userDAO.GetIdFromEmail(email) == -1)
                return true;
            return false;
        }
    }
}
