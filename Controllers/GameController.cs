﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinesweeperApp.BusinessServices;
using MinesweeperApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace MinesweeperApp.Controllers
{
    /// <summary>
    /// This class controller supports routing while playing a game of minesweeper.
    /// </summary>
    [CustomAuthorization(LogOutRequired = false)]
    public class GameController : Controller
    {

        //Business service object to handle game logic and board updates.
        private GameboardBusinessService gbs;

        //Business service object to handle loading and saving games.
        private SavingLoadingService sls;

        /// <summary>
        /// This enum holds the different states that a game can be in
        /// </summary>
        public enum GameState
        {
            None = -1,
            Won = 0,
            Lost = 1,
            Playing = 2
        }

        //Internal list of cells that need to be visually updated after a move has been made
        private List<int> updatedCells;

        public GameController(GameboardBusinessService gbs, SavingLoadingService sls)
        {
            this.gbs = gbs;
            this.sls = sls;
        }

        /// <summary>
        /// Default routing for this controller.
        /// </summary>
        /// <returns>A view containing the initial set up requirements for a game.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Routing option for creating a board from the given difficulty level.
        /// </summary>
        /// <param name="options">This will be a difficulty selection of (Easy, Medium, Hard)</param>
        /// <returns>A view of the newly created board ready to play.</returns>
        [HttpPost]
        public IActionResult CreateBoard(string options)
        {
            //set up the new board
            gbs.NewGame(options);

            //set the game state to playing
            HttpContext.Session.SetInt32("gameState", (int)GameState.Playing);

            //create a persistent game board
            HttpContext.Session.SetString("board", gbs.SerializeGameBoard());

            //set view bag variables for the view
            ViewBag.Width = gbs.GameBoard.Size;
            ViewBag.TimeStarted = gbs.GameBoard.TimeStarted;
            ViewBag.TimePlayed = gbs.GameBoard.TimePlayed.ToString(@"dd\.hh\:mm\:ss");

            return View("GameBoard", gbs.Grid);
        }

        /// <summary>
        /// Default routing option for left clicking. This method is default only and is depreciated. 
        /// </summary>
        /// <param name="buttonNumber">The number of the button that was pressed. This will correspond to the Cell's position and Id.</param>
        /// <returns>A new view containing the updated game board.</returns>
        public IActionResult HandleButtonClick(string buttonNumber)
        {
            //will simply reload the gameboard on a click
            ViewBag.Width = gbs.GameBoard.Size;
            return View("GameBoard", gbs.Grid);
        }

        /// <summary>
        /// This method receives left click information and produces a list of cells that were affected by the click.
        /// </summary>
        /// <param name="buttonNumber">The number of the button that was pressed. This will correspond to the Cell's position and Id.</param>
        /// <returns>A Json object list of each cell which has had it's state altered.</returns>
        public JsonResult HandleButtonLeftClick(string buttonNumber)
        {
            //Parse the Id back to Integer
            int buttonId = int.Parse(buttonNumber);

            //reset the list of altered cells
            updatedCells = new List<int>();

            //Check for a good game state
            if ((GameState)CheckGrid() == GameState.Playing)
            {
                //get the persistent game board
                gbs.DeserializeGameBoard(HttpContext.Session.GetString("board"));

                //make the move on the game board
                if (gbs.MakeMove(buttonId, updateCell))
                {
                    //A mine was hit so set the proper flag and reveal all the cells
                    HttpContext.Session.SetInt32("gameState", (int)GameState.Lost);
                    gbs.RevealAll(updateCell);
                }
                else if (gbs.CheckForWin())
                {
                    //A win was detected so set the proper game state.
                    HttpContext.Session.SetInt32("gameState", (int)GameState.Won);
                }

                //update persistent game board
                HttpContext.Session.SetString("board", gbs.SerializeGameBoard());
            }

            return  Json(updatedCells);
        }

        /// <summary>
        /// This method handles right click information and produces a Json list of cells that were affected by the click
        /// </summary>
        /// <param name="buttonNumber">The number of the button that was pressed. This will correspond to the Cell's position and Id.</param>
        /// <returns>A Json object list of each cell which has had it's state altered.</returns>
        public JsonResult HandleButtonRightClick(string buttonNumber)
        {
            //Parse the Id back to Integer
            int id = int.Parse(buttonNumber);

            //reset the list of altered cells
            updatedCells = new List<int>();

            //check for a good game state
            if ((GameState)CheckGrid() == GameState.Playing)
            {
                //get the persistent game board
                gbs.DeserializeGameBoard(HttpContext.Session.GetString("board"));

                //toggle a flag on the cell the user clicked
                gbs.ToggleFlag(id);

                //check for a win condition and set the state if it was achieved
                if (gbs.CheckForWin())
                    //A win was detected so set the proper game state.
                    HttpContext.Session.SetInt32("gameState", (int)GameState.Won);

                //add the altered cell to the update list
                updatedCells.Add(id);

                //create a persistent game board
                HttpContext.Session.SetString("board", gbs.SerializeGameBoard());
            }

            return Json(updatedCells);
        }

        /// <summary>
        /// This routing path saves the current board to the database. It will not create new saves for the same board so if the board has been saved previously it will update the existing state.
        /// </summary>
        /// <returns>A view containing the current grid that the user is playing</returns>
        public IActionResult SaveGame()
        {
            //if we are still playing we can save the game
            if ((GameState)CheckGrid() == GameState.Playing)
            {
                //get the persistent game board
                gbs.DeserializeGameBoard(HttpContext.Session.GetString("board"));

                //update the play time
                gbs.UpdatePlayTime();

                //process the save through the business service
                sls.SaveGame((int)HttpContext.Session.GetInt32("userId"), gbs.GameBoard);
            }

            //reload the game to return to the existing game board.
            return LoadGame(gbs.GameBoard.Id);
        }

        /// <summary>
        /// This routing path loads the given board and display the grid so the user can continue playing.
        /// </summary>
        /// <param name="boardId">The Id of the board to load from the database.</param>
        /// <returns>A view of the grid so that the user can continue to play.</returns>
        public IActionResult LoadGame(int boardId)
        {
            //Grab the game board from the loading business service
            gbs.GameBoard = sls.LoadGame(boardId);

            //set the state to for continued playing
            HttpContext.Session.SetInt32("gameState", (int)GameState.Playing);

            //create persistent game board
            HttpContext.Session.SetString("board", gbs.SerializeGameBoard());

            //reset the view bag parameters
            ViewBag.Width = gbs.GameBoard.Size;
            ViewBag.TimeStarted = gbs.GameBoard.TimeStarted;
            ViewBag.TimePlayed = gbs.GameBoard.TimePlayed.ToString(@"dd\.hh\:mm\:ss");

            return View("GameBoard", gbs.Grid);
        }

        /// <summary>
        /// This routing option removes the given board from the database
        /// </summary>
        /// <param name="boardId">The Id of the board to remove from the database.</param>
        /// <returns>A view of all the remaining games owned by the currently logged in user</returns>
        public IActionResult DeleteGame(int boardId)
        {
            sls.DeleteSaveGame((int)HttpContext.Session.GetInt32("userId"), boardId);

            return SavedGameList();
        }

        /// <summary>
        /// This routing path loads a list of saved games owned by the logged in user.
        /// </summary>
        /// <returns>A view list containing all the game saves for the currently logged in user.</returns>
        public IActionResult SavedGameList()
        {
            //Grab the list of saves for this user.
            List<Board> boardList = sls.GetGameList((int)HttpContext.Session.GetInt32("userId"));

            //convert the list into publicly viewable DTO objects
            IEnumerable<BoardDTO> list = from b in boardList
                                                     select
                                                     new BoardDTO(b.Id, b.Difficulty, b.TimeStarted, b.TimePlayed)
                                                     ;

            return View(list);
        }

        /// <summary>
        /// This Routing option is a partial load of a a single cell and is used to load only a piece of the entire grid.
        /// </summary>
        /// <param name="buttonNumber">The button on the grid that this partial load should focus on.</param>
        /// <returns>A partial view of the button being updated.</returns>
        public IActionResult UpdateOneCell(string buttonNumber)
        {
            //Parse the Id from the button number
            int id = int.Parse(buttonNumber);

            //get the persistent game board
            gbs.DeserializeGameBoard(HttpContext.Session.GetString("board"));

            return PartialView("SingleButton", gbs.Grid.ElementAt(id));
        }

        /// <summary>
        /// This routing option takes the user to a win screen,
        /// </summary>
        /// <returns>A view to notify the user of a completed game.</returns>
        public IActionResult Winner()
        {
            return View("Winner");
        }

        /// <summary>
        /// This routing option takes the user to a loss screen
        /// </summary>
        /// <returns>A view informing the user of a loss condition.</returns>
        public IActionResult EndGame()
        {
            return View("EndGame");
        }

        /// <summary>
        /// This method is for notifying other systems of the current running game state.
        /// </summary>
        /// <returns>The game state of the current board.</returns>
        public int CheckGrid()
        {
            return HttpContext.Session.GetInt32("gameState") == null ? -1 : (int)HttpContext.Session.GetInt32("gameState");
        }

        /// <summary>
        /// This method updates and returns the current running play time timer
        /// </summary>
        /// <returns>A string representing the current play time.</returns>
        public string UpdateTimer()
        {
            //get the persistent game board
            gbs.DeserializeGameBoard(HttpContext.Session.GetString("board"));

            gbs.UpdatePlayTime();

            //update persistent game board
            HttpContext.Session.SetString("board", gbs.SerializeGameBoard());
            
            return gbs.GameBoard.TimePlayed.ToString(@"dd\.hh\:mm\:ss");
        }

        /// <summary>
        /// This method is a helper function to load new cells into the altered list. It checks for previous entries so there are no duplicates.
        /// </summary>
        /// <param name="id">The Id of the cell to check for and add.</param>
        public void updateCell(int id)
        {
            if(!updatedCells.Contains(id))
                updatedCells.Add(id);
        }

    }
}
