using CST350_Milestone1.Models;
using Microsoft.AspNetCore.Mvc;
using CST350_Milestone1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CST350_Milestone1.Controllers
{
    public class GameBoardController : Controller
    {
        static Board gameBoard;
        const int GRID_SIZE = 12; // 144 when squared

        public IActionResult Index()
        {
            gameBoard = new Board(10, GRID_SIZE);

            return View(gameBoard);
        }




        //Change the button to a flag
        public IActionResult FlagButton(int buttonRow, int buttonCol)
        {
            Cell clicked = gameBoard.Grid[buttonRow, buttonCol];
            clicked.Flagged = !clicked.Flagged;

            return PartialView(clicked);
        }

        public IActionResult Result(int buttonRow, int buttonCol)
        {
            Cell clicked = gameBoard.Grid[buttonRow, buttonCol];

            if (clicked.Flagged)
            {
                // Toggle the flag status if the cell was flagged
                clicked.Flagged = false;
            }
            else if (!clicked.Visited)
            {
                // Check if user hit a bomb
                if (clicked.Live)
                {
                    // Reveal all bombs
                    foreach (Cell cell in gameBoard.Grid)
                    {
                        // Set all live or previously-visited tiles to Visited so UI exposes all hidden mines
                        cell.Visited = cell.Visited || cell.Live;
                    }
                    return PartialView("_GameOver", gameBoard); // You might need to create a partial view named "_GameOver"
                }
                else
                {
                    clicked.Visited = true;
                }
            }

            bool allVisited = true;
            foreach (Cell cell in gameBoard.Grid)
            {
                if (!cell.Visited && !cell.Live)
                {
                    allVisited = false;
                    break;
                }
            }

            if (allVisited)
            {
                return PartialView("_GameWon", gameBoard); // You might need to create a partial view named "_GameWon"
            }

            return PartialView("_GameBoard", gameBoard); // You might need to create a partial view named "_GameBoard"
        }

        // Result: 0 for no, 1 for loss, 2 for win
        private int HasGameEnded(Board board, int row, int col, bool flagAttempt)
        {
            // Check if mouse button was a right click
            if (flagAttempt)
            {
                board.Grid[row, col].Flagged = !board.Grid[row, col].Flagged;
                return 0;
            }

            // Check if the button is already flagged. If so, do nothing
            if (board.Grid[row, col].Flagged)
            {
                return 0;
            }

            // Check if user hit a bomb
            if (board.Grid[row, col].Live)
            {
                // Reveal all bombs
                for (int r = 0; r < board.Grid.GetLength(0); r++)
                {
                    for (int c = 0; c < board.Grid.GetLength(1); c++)
                    {
                        // Set all live or previously-visited tiles to Visited so UI exposes all hidden mines
                        board.Grid[r, c].Visited = board.Grid[r, c].Visited || board.Grid[r, c].Live;
                    }
                }
                return 1;
            }

            board.Grid[row, col].Visited = true;
            return 1;
        }
    }
}
