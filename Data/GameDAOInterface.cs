using CST350_Milestone1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CST350_Milestone1.Data
{
    public interface GameDAOInterface
    {
            public Game FindById(int id);
            public bool SaveGame(Game game);

            // NOT NEEDED - public Game ResumeGame(int id);
       
    }
}
