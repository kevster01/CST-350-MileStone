 using Microsoft.AspNetCore.Mvc;
using CST350_Milestone1.Models;




namespace CST350_Milestone1.Controllers
{
    public class GameController : Controller
    {
        Board gameBoard;
        public GameController()
        {
            gameBoard = new Board();
        }
        public IActionResult Index()
        {
            System.Diagnostics.Debug.WriteLine("Cells are instantiated: " + (gameBoard.Grid[0, 0] == null));
            return View("Game", gameBoard);
        }

        [HttpPost]
        public ActionResult UpdateCell(int row, int cell)
        {
            // TODO: Refactor to include right-click attempts later
            HasGameEnded(gameBoard, row, cell, false);
            return View("Game", gameBoard);
        }

        private bool HasGameEnded(Board board, int row, int col, bool flagAttempt)
        {
            // Check if mouse button was a right click
            if (flagAttempt)
            {
                board.Grid[row, col].Flagged = !board.Grid[row, col].Flagged;
                return false;
            }

            // Check if the button is already flagged. If so, do nothing
            if (board.Grid[row, col].Flagged)
            {
                return false;
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
                return true;
            }

            board.Grid[row, col].Visited = true;
            return false;
        }
    }

}
        
