﻿using MinesweeperApp.DatabaseServices;
using MinesweeperApp.Models;

namespace MinesweeperApp.BusinessServices
{
    /// <summary>
    /// This class service handles the logic for user login and validation.
    /// </summary>
    public class LoginBusinessService
    {
        //Database service object for users
        private IUserDAO userDAO;

        public LoginBusinessService(IUserDAO userDAO)
        {
            this.userDAO = userDAO;
        }

        /// <summary>
        /// This method takes in a user object and tries to validate their credentials vs. the database.
        /// </summary>
        /// <param name="user">A user object containing the login information to process.</param>
        /// <returns>Integer value of the logged in user. Will return -1 if the log in failed.</returns>
        public int ValidateLogin(User user)
        {
            if (userDAO.FindUserByUsernameAndPassword(user.Username, user.Password))
            {
                return userDAO.GetIdFromUsername(user.Username);
            }
            return -1;
        }
    }
}
