using CST350_Milestone1.Models;
using Microsoft.AspNetCore.Mvc;
using CST350_Milestone1.Services;

namespace CST350_Milestone1.Controllers
{
    public class GameController : Controller
    {
        static List<CellModel> cells = new List<CellModel>();

        Random rand = new Random();
        static int GRID_SIZE = 6;
        static bool gameStarted = false;
        static bool gameOver = false;
        public IActionResult Index()
        {
            setUpBoard();

            return View("Index", cells);
        }

        /*
        public IActionResult RevealCells(int tileNumber)
        {
            if (!gameOver)
            {
                RevealCellAndNeighbors(tileNumber);
            }

            return View("Index", cells);
        }

        */
        public IActionResult NewGame()
        {
            cells.Clear();
            gameStarted = false;
            gameOver = false;
            setUpBoard();
            return View("Index", cells);
        }

        /*
       private void RevealCellAndNeighbors(int tileNumber)
       {
           if (cells[tileNumber].Visited || cells[tileNumber].Flagged)
           {
               return;
           }

           cells[tileNumber].Visited = true;

           if (cells[tileNumber].State > 0)
           {
               // Game over if a bomb is clicked
               gameOver = true;
               RevealAllCells();
           }
           else if (cells[tileNumber].Neighbors == 0)
           {
               int row = cells[tileNumber].Row;
               int col = cells[tileNumber].Col;

               int[] xmove = { -1, -1, -1, 0, 0, 1, 1, 1 };
               int[] ymove = { -1, -0, 1, -1, 1, -1, 0, 1 };

               for (int i = 0; i < 8; i++)
               {
                   int newRow = row + xmove[i];
                   int newCol = col + ymove[i];

                   if (IsValidCell(newRow, newCol))
                   {
                       int newTileNumber = newRow * GRID_SIZE + newCol;
                       RevealCellAndNeighbors(newTileNumber);
                   }
               }
           }
       }

       */
       

        private bool IsValidCell(int row, int col)
       {
           return row >= 0 && row < GRID_SIZE && col >= 0 && col < GRID_SIZE;
        }


        /*
           private void RevealAllCells()
           {
               foreach (var cell in cells)
               {
                   cell.Visited = true;
               }
           }
        */




        
           public IActionResult ShowOneCell(int tileNumber)
           {
               if (cells[tileNumber].Flagged == false)
               {
                   if (cells[tileNumber].State > 0)
                   {
                       gameOver = true;

                       cells[tileNumber].Visited = true;
                       Console.WriteLine("Game Over Fool!!!");
                   }
                   else
                   {

                       FloodFill(cells[tileNumber].Row, cells[tileNumber].Col);


                   }
                   cells.ElementAt(tileNumber).Visited = true;
               }
               RedirectToAction("Index", "Game", cells);
               return PartialView("showOneCell", cells.ElementAt(tileNumber));
           }



           public IActionResult RightClickOneCell(int tileNumber)
           {

               //Toggle
               cells.ElementAt(tileNumber).Flagged = cells.ElementAt(tileNumber).Flagged ? false : true;
               return PartialView("showOneCell", cells.ElementAt(tileNumber));
           }

           


        /*

       public IActionResult ShowOneCell(int tileNumber)
       {
           if (cells[tileNumber].Flagged == false)
           {
               if (cells[tileNumber].State > 0)
               {
                   gameOver = true;
                   cells[tileNumber].Visited = true;
                   Console.WriteLine("Game Over Fool!!!");
               }
               else
               {
                   RevealSurroundingEmptyCells(cells[tileNumber].Row, cells[tileNumber].Col);
               }
               cells[tileNumber].Visited = true;
           }

           return PartialView("showOneCell", cells.ElementAt(tileNumber));
       }

       public IActionResult RightClickOneCell(int tileNumber)
       {
           cells[tileNumber].Flagged = !cells[tileNumber].Flagged;
           return PartialView("showOneCell", cells.ElementAt(tileNumber));
       }

       private void RevealSurroundingEmptyCells(int row, int col)
       {
           if (!IsValidCell(row, col) || cells[row * GRID_SIZE + col].Visited || cells[row * GRID_SIZE + col].Flagged)
               return;

           cells[row * GRID_SIZE + col].Visited = true;

           if (cells[row * GRID_SIZE + col].State == 0)
           {
               int[] xmove = { -1, -1, -1, 0, 0, 1, 1, 1 };
               int[] ymove = { -1, -0, 1, -1, 1, -1, 0, 1 };

               for (int i = 0; i < 8; i++)
               {
                   int newRow = row + xmove[i];
                   int newCol = col + ymove[i];

                   RevealSurroundingEmptyCells(newRow, newCol);
               }
           }
       }

       */

        private void setUpBoard()
       {
           if (!gameStarted)
           {
               int x = GRID_SIZE;
               int y = x;
               int id = 0;
               double difficulty = .1;
               int bombCount = (int)((double)difficulty * (GRID_SIZE * GRID_SIZE));
               int randRow, randCol;
               CellModel currCell;

               for (int i = 0; i < x; i++)
               {
                   for (int j = 0; j < y; j++)
                   {
                       cells.Add(new CellModel(id, 0, i, j));
                       id++;
                   }
               }

               while (bombCount > 0)
               {
                   randRow = rand.Next(x + 1);
                   randCol = rand.Next(y + 1);

                   currCell = cells.Find(cell => cell.Row == randRow && cell.Col == randCol);

                   if (currCell != null)
                   {
                       currCell.State = 1;
                       bombCount--;
                   }
               }

               gameStarted = true;
           }
       }
      

        private int calculateLiveNeighbors(CellModel cell)
        {
            int x = cell.Row;
            int y = cell.Col;
            int size = (int)Math.Sqrt(GRID_SIZE);
            int liveNeghbors = 0;
            CellModel curCell;

            int[] xmove = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] ymove = { -1, -0, 1, -1, 1, -1, 0, 1 };

            if (cells.Find(cell => cell.Row == x && cell.Col == y).State > 0)
                liveNeghbors++;

            for (int i = 0; i < 8; i++)
            {
                curCell = cells.Find(cell => cell.Row == x + xmove[i] && cell.Col == y + ymove[i]);

                if (curCell != null && (x + xmove[i] >= 0 && y + ymove[i] >= 0 && curCell.State > 0))
                    liveNeghbors++;
            }

            return liveNeghbors;
        }



        public void FloodFill(int row, int col)
        {
            CellModel currCell = cells.Find(cell => cell.Row == row && cell.Col == col);

            if (currCell == null || currCell.State > 0 || currCell.Visited)
            {
                return;
            }
            else
            {

                currCell.Neighbors = calculateLiveNeighbors(currCell);
                currCell.Visited = true;

            }

            if (currCell.Neighbors == 0)
            {
                int[] xmove = { -1, -1, -1, 0, 0, 1, 1, 1 };
                int[] ymove = { -1, -0, 1, -1, 1, -1, 0, 1 };

                for (int i = 0; i < 8; i++)
                {
                    FloodFill(row + xmove[i], col + ymove[i]);


                }
            }

        }

        public IActionResult GameOver(string gameMessage)
        {
            return View(gameMessage);
        }



    }
}
