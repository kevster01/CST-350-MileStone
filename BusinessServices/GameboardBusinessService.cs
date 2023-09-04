﻿using MinesweeperApp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MinesweeperApp.BusinessServices
{
    /// <summary>
    /// This business service class handles in-game logic for a Minesweeper board.
    /// </summary>
    public class GameboardBusinessService
    {
        //The game board object of the current game
        public Board GameBoard { get; set; }

        //callback method signature for revealing a single Cell
        public delegate void RevealCell(int id);

        //Constant grid size value for creating a new game 
        private const int GRID_SIZE = 10;

        //Local array of values to determine mine count on each new board.
        private float[] DifficultySettings = { 0.12f, 0.14f, 0.16f };

        /// <summary>
        /// A single dimensional List of Cell objects that represent the current board.
        /// </summary>
        public List<Cell> Grid
        {
            get
            {
                List<Cell> cells = new List<Cell>();
                for (int i = 0; i < GameBoard.Size; i++)
                {
                    for (int j = 0; j < GameBoard.Size; j++)
                    {
                        cells.Add(GameBoard.Grid[i, j]);
                    }
                }
                return cells;
            }
        }


        public GameboardBusinessService()
        {
            GameBoard = new Board();
        }

        /// <summary>
        /// Initiates a new game and randomizes the mines on the grid based on the difficulty.
        /// </summary>
        /// <param name="difficulty">A string representing the difficulty level (Easy, Medium, Hard).</param>
        public void NewGame(string difficulty)
        {
            //create a new board
            GameBoard.Difficulty = getDifficultyFromString(difficulty);
            GameBoard.Size = GRID_SIZE;
            GameBoard.TimeStarted = GameBoard.CurrentStartTime = DateTime.Now;
            UpdatePlayTime();
            
            //clears the existing grid to a certain size
            clearGrid(GameBoard.Size);

            //create the mines on the board
            setupLiveNeighbors();

            //determine all the neighbor counts on the new board
            calculateLiveNeighbors();
        }

        /// <summary>
        /// This method toggles the flagged status of a given cell
        /// </summary>
        /// <param name="cellId">The given cell Id to toggle the flag of.</param>
        public void ToggleFlag(int cellId)
        {
            //grab the current cell's position
            var row = cellId / GameBoard.Size;
            var col = cellId % GameBoard.Size;

            //check bounds first
            if (withinBounds(row, col))
            {
                //toggle flag
                GameBoard.Grid[row, col].Flagged = !GameBoard.Grid[row, col].Flagged;
            }
        }

        /// <summary>
        /// This method checks the entire board for all visits to determine endgame status
        /// </summary>
        /// <returns>true if endgame status was achieved, false otherwise.</returns>
        public bool CheckForWin()
        {
            //counter to track the number of cells checked.
            int counter = 0;

            //go through each cell to check its status
            foreach (var cell in GameBoard.Grid)
            {
                //If the cell was visited or the cell was flagged and a mine add one to the counter.
                if (cell.Visited || (cell.Flagged && cell.Mine))
                {
                    counter++;
                }
            }

            //If the counter reached all cells in the grid we have won.
            if (counter == (GameBoard.Size * GameBoard.Size))
                return true;
            else
                return false;
        }

        /// <summary>
        /// This method visits a cell on the grid. Will recursivly visit neighbors when appropriate. (ignores flagged cells)
        /// </summary>
        /// <param name="cellId">The Id of the cell to visit initially.</param>
        /// <param name="revealCell">A callback method of type RevealCell to help with revealing logic of each cell visited in cases of recursion.</param>
        /// <returns>It will return true if the cell was a mine otherwise false.</returns>
        public bool MakeMove(int cellId, RevealCell revealCell = null)
        {
            //grab the current cell's position
            var row = cellId / GameBoard.Size;
            var col = cellId % GameBoard.Size;

            //check that we are inside the grid still
            if (!withinBounds(row, col))
            {
                return false;
            }

            //check for flag or a previous visit
            if (GameBoard.Grid[row, col].Flagged || GameBoard.Grid[row, col].Visited)
            {
                return false;
            }

            //visit the cell and call reveal logic on it
            GameBoard.Grid[row, col].Visited = true;
            if(revealCell != null)
                revealCell((row * GameBoard.Size) + col);

            //check for no neighbors
            if (GameBoard.Grid[row, col].LiveNeighbors == 0)
            {
                //recursively fill out all connected no neighbor cells
                floodFill(row, col, revealCell);
            }

            return GameBoard.Grid[row, col].Mine;
        }

        /// <summary>
        /// This method helps with revealing all cells on the grid in cases of a loss.
        /// </summary>
        /// <param name="revealCell">A callback method to allow outside revealing cell logic.</param>
        public void RevealAll(RevealCell revealCell)
        {
            //loop through all of the cells
            foreach (var cell in GameBoard.Grid)
            {
                if (!cell.Visited)
                {
                    //reveal the cell and callback to revealing logic
                    cell.Visited = true;
                    revealCell(cell.Id);
                }
            }
        }

        /// <summary>
        /// This method updates the current timer with the current sessions start time and the time at the moment of the call.
        /// </summary>
        public void UpdatePlayTime()
        {
            //Update the play time
            GameBoard.TimePlayed = GameBoard.TimePlayed + (DateTime.Now - GameBoard.CurrentStartTime);
            GameBoard.CurrentStartTime = DateTime.Now;
        }

        /// <summary>
        /// This method takes the current GameBoard and serializes it into a string.
        /// </summary>
        /// <returns>The complete GameBoard serialized into a string with each property on a single line.</returns>
        public string SerializeGameBoard()
        {
            StringWriter sw = new StringWriter();

            sw.WriteLine(GameBoard.Id);
            sw.WriteLine(GameBoard.Size);
            sw.WriteLine(GameBoard.Difficulty);
            sw.WriteLine(GameBoard.NumberOfMines);
            sw.WriteLine(GameBoard.TimeStarted);
            sw.WriteLine(GameBoard.CurrentStartTime);
            sw.WriteLine(GameBoard.TimePlayed);
            sw.Write(Board.SerializeGridToString(GameBoard.Grid, GameBoard.Size, GameBoard.Size));

            return sw.ToString();
        }

        /// <summary>
        /// This method takes in a string and attempts to parse a GameBoard from each line of the string.
        /// </summary>
        /// <param name="str">A potential Serialized GameBoard.</param>
        public void DeserializeGameBoard(string str)
        {
            StringReader sr = new StringReader(str);

            //try to parse data out of the string
            try
            {
                GameBoard.Id = int.Parse(sr.ReadLine());
                GameBoard.Size = int.Parse(sr.ReadLine());
                GameBoard.Difficulty = int.Parse(sr.ReadLine());
                GameBoard.NumberOfMines = int.Parse(sr.ReadLine());
                GameBoard.TimeStarted = DateTime.Parse(sr.ReadLine());
                GameBoard.CurrentStartTime = DateTime.Parse(sr.ReadLine());
                GameBoard.TimePlayed = TimeSpan.Parse(sr.ReadLine());
                GameBoard.Grid = Board.DeserializeGridFromString(sr.ReadToEnd());
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error while deserializing game board! \n" + ex.Message);
            }
        }

        /// <summary>
        /// This method clears the given board grid and sizes the new one based on the input size
        /// </summary>
        /// <param name="size">The new grids size.</param>
        private void clearGrid(int size)
        {
            // Clears the given board grid and sizes the new one based on the input size
            GameBoard.Grid = new Cell[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    //Id for each cell will be (row * width + col)
                    GameBoard.Grid[i, j] = new Cell((i * size) + j);
                }
            }
        }

        /// <summary>
        /// This method sets up the game board with mines based on the current difficulty
        /// </summary>
        private void setupLiveNeighbors()
        {
            // Sets up the game board with mines based on the current difficulty
            var rand = new Random();

            // Determine the number of mines based on difficulty
            GameBoard.NumberOfMines = (int)((GameBoard.Size * GameBoard.Size) * DifficultySettings[GameBoard.Difficulty - 1]);

            for (int i = 0; i < GameBoard.NumberOfMines; i++)
            {
                //randomly place mine
                var row = rand.Next(0, GameBoard.Size);
                var col = rand.Next(0, GameBoard.Size);

                //check for previously placed mine
                if (GameBoard.Grid[row, col].Mine)
                {
                    //go back and try to place it again
                    i--;
                }
                else
                {
                    //place the new mine
                    GameBoard.Grid[row, col].Mine = true;
                }
            }
        }

        private void calculateLiveNeighbors()
        {
            // Calculates the number of live neighbors for each cell on the board
            foreach (var cell in GameBoard.Grid)
            {
                //loop through each cell surrounding the current cell in a 3x3 Grid
                for (int m = -1; m < 2; m++)
                {
                    for (int n = -1; n < 2; n++)
                    {
                        //grab the current neighbor's position
                        var row = (cell.Id / GameBoard.Size) + m;
                        var col = (cell.Id % GameBoard.Size) + n;

                        //check if we are off the board
                        if (!withinBounds(row, col))
                        {
                            continue;
                        }

                        //check for mine
                        if (GameBoard.Grid[row, col].Mine)
                        {
                            //increment the touching mine count
                            cell.LiveNeighbors++;
                        }
                    }
                }
            }
        }

        private bool withinBounds(int row, int col)
        {
            // Checks if the given position is within the bounds of the current size grid
            return row >= 0 && col >= 0 && row < GameBoard.Size && col < GameBoard.Size;
        }

        // Recursive function to find all adjacent zero live neighbor cells and set them to visited
        private void floodFill(int row, int col, RevealCell revealCell = null)
        {
            //loop through all of the 3 x 3 neighbors of the current cell
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    //check within bounds
                    if (!withinBounds(row + i, col + j))
                    {
                        continue;
                    }

                    //check if we have been to this cell already or if it is flagged
                    if (GameBoard.Grid[row + i, col + j].Visited || GameBoard.Grid[row + i, col + j].Flagged)
                    {
                        continue;
                    }

                    //visit the cell then callback to any reveal logic
                    GameBoard.Grid[row + i, col + j].Visited = true;
                    if(revealCell != null)
                        revealCell(((row + i) * GameBoard.Size) + col + j);

                    //check for continued recursion
                    if (GameBoard.Grid[row + i, col + j].LiveNeighbors == 0)
                    {
                        floodFill(row + i, col + j, revealCell);
                    }
                }
            }
        }

        // Gets the difficulty level based on the string name of each level; defaults to 2 if no difficulty was parsed

        private int getDifficultyFromString(string difficutly)
        {
            //default level is medium
            int difficutlyLevel = 2;

            if (difficutly == "Easy")
            {
                difficutlyLevel = 1;
            }
            else if (difficutly == "Medium")
            {
                difficutlyLevel = 2;
            }
            else if (difficutly == "Hard")
            {
                difficutlyLevel = 3;
            }
            return difficutlyLevel;
        }
    }
}
