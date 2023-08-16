using System;
using System.Runtime.Serialization;

namespace CST350_Milestone1.Models
{
    [Serializable()]
    public class Board
    {
        public string BoardId { get; set; }
        public int Size { get; set; }
        public Cell[,] Grid { get; set; }
        public int Difficulty { get; set; }

        public Board()
        {
            Size = 16;
            Difficulty = 20; // 20% of board is bombs to start
            Grid = new Cell[Size, Size];
            SetupLiveNeighbors();
        }
        public Board(int difficulty, int size = 16)
        {
            Size = size;
            // Difficulty adds a bit of an extra kick, for Moderate and Difficult settings
            // 10% of board is bombs for Easy
            //  With an added 4% or 20% percent added on top of that, depending
            Difficulty = 10 + (40 * (difficulty / 10));

            Grid = new Cell[Size, Size];
            SetupLiveNeighbors();
        }

        public void SetupLiveNeighbors()
        {
            int squareSpace = (int)Math.Pow(Size, 2);
            int totalAllowedBombs = (int)Math.Ceiling((decimal)squareSpace * ((decimal)Difficulty / 100));

            // Create 2D array to calculate which cells are live/dead
            Random rand = new Random();
            bool[] liveCells = new bool[squareSpace];

            // Setup and populate sorting set, used to randomize the liveCells array
            Double[] sortOrder = new Double[squareSpace];
            for (int idx = 0; idx < sortOrder.Length; idx++)
                sortOrder[idx] = rand.NextDouble();
            for (int idx = 0; idx < squareSpace; idx++)
            {
                // guarantee that all allowed bombs are set
                // in accord with the difficulty setting
                liveCells[idx] = idx < totalAllowedBombs;
            }

            // Randomize the liveCells placement through sortOrder
            Array.Sort(sortOrder, liveCells);

            // Now iter through 2D array and initialize all cells
            int liveCellSeedIdx = 0;
            for (int row = 0; row < Grid.GetLength(0); row++)
            {
                for (int col = 0; col < Grid.GetLength(1); col++)
                {
                    Grid[row, col] = new Cell(col, row, false, liveCells[liveCellSeedIdx], 0);
                    liveCellSeedIdx++;
                }
            }

            // Make a second pass over the Grid and calculate all live neighbors for each cell
            CalculateLiveNeighbors();
        }

        public void CalculateLiveNeighbors()
        {
            for (int row = 0; row < Grid.GetLength(0); row++)
            {
                for (int col = 0; col < Grid.GetLength(1); col++)
                {
                    CalculateLiveCellNeighbors(Grid[row, col]);
                }
            }
        }

        private void CalculateLiveCellNeighbors(Cell c)
        {
            // Set to 0 automatically, per rules, if cell itself is live
            if (c.Live)
            {
                c.LiveNeighbors = 9;
                return;
            }

            // test if any array index is going to cause out-of-bounds errs
            // If it would, just add a default (!Live && LiveNeighbors == 0)
            Cell def = new Cell(0, 0, false, false, 0);
            Cell left = (c.Column - 1 >= 0) ? Grid[c.Row, c.Column - 1] : def;
            Cell right = (c.Column + 1 < Size) ? Grid[c.Row, c.Column + 1] : def;
            Cell top = (c.Row - 1 >= 0) ? Grid[c.Row - 1, c.Column] : def;
            Cell bottom = (c.Row + 1 < Size) ? Grid[c.Row + 1, c.Column] : def;
            Cell topRight = (c.Row - 1 >= 0 && c.Column + 1 < Size) ? Grid[c.Row - 1, c.Column + 1] : def;
            Cell topLeft = (c.Row - 1 >= 0 && c.Column - 1 >= 0) ? Grid[c.Row - 1, c.Column - 1] : def;
            Cell bottomRight = (c.Row + 1 < Size && c.Column + 1 < Size) ? Grid[c.Row + 1, c.Column + 1] : def;
            Cell bottomLeft = (c.Row + 1 < Size && c.Column - 1 >= 0) ? Grid[c.Row + 1, c.Column - 1] : def;

            int liveNeighbors = 0;
            foreach (Cell neighbor in new Cell[] { left, right, top, bottom, topRight, topLeft, bottomRight, bottomLeft })
            {
                liveNeighbors += neighbor.Live ? 1 : 0;
            }
            c.LiveNeighbors = liveNeighbors;
        }

        private bool isSafeCell(int r, int c)
        {
            return (r >= 0 && r < Size) && (c >= 0 && c < Size) && !Grid[r, c].Live && !Grid[r, c].Visited;
        }

        public void floodFill(int r, int c)
        {
            if (!Grid[r, c].Visited && isSafeCell(r, c))
            {
                // Mark grid element as visited
                Grid[r, c].Visited = true;
                // Recursively check all compass directions
                if (isSafeCell(r - 1, c))
                {
                    if (Grid[r - 1, c].LiveNeighbors == 0) floodFill(r - 1, c); // N
                    else Grid[r - 1, c].Visited = true; // reach to next and flip it to visited
                }
                if (isSafeCell(r, c + 1))
                {
                    if (Grid[r, c + 1].LiveNeighbors == 0) floodFill(r, c + 1); // E
                    else Grid[r, c + 1].Visited = true;
                }
                if (isSafeCell(r + 1, c))
                {
                    if (Grid[r + 1, c].LiveNeighbors == 0) floodFill(r + 1, c); // S
                    else Grid[r + 1, c].Visited = true;
                }
                if (isSafeCell(r, c - 1))
                {
                    if (Grid[r, c - 1].LiveNeighbors == 0) floodFill(r, c - 1); // W
                    else Grid[r, c - 1].Visited = true;
                }
            }
            return;
        }

        public bool AllSafeTilesVisited()
        {
            int rows = Grid.GetLength(0);
            int cols = Grid.GetLength(1);
            bool someUnvisited = false;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (!Grid[row, col].Visited && !Grid[row, col].Live) someUnvisited = true;
                }
            }
            return !someUnvisited;
        }
    }



}
