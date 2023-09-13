using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using System;

// creates a class for all tiles to use. contains attributes that will affect game flow and sprite.
public class Cell
{
    // the default values that a tile generates with.
    public int posX = 0;
    public int posY = 0;
    public int Adjacency = 0;
    public bool IsExploded = false;
    public bool IsMined = false;
    public bool IsFlagged = false;
    public bool IsUnknown = true;
    public bool IsEmpty = true;
    public bool isHighlighted = false;
    public bool gameOver = false; // used to show mines when the game is lost
}
 
public class Grid : MonoBehaviour
{
    // declares all objects and variables that affect game as a whole.
    
    // these are the game objects.
    [SerializeField] private Camera camera;
    [SerializeField] public Tilemap tilemap;
    [SerializeField] public AudioSource audioSource;

    // these are the sound effects.
    [SerializeField] public AudioClip audioExplosion;
    [SerializeField] public AudioClip audioFlag;
    [SerializeField] public AudioClip audioFanfare;
    [SerializeField] public AudioClip audioSweep;

    // these are the UI elements.
    [SerializeField] private TMP_Text stopwatch;
    [SerializeField] private RawImage stopwatchIcon;
    [SerializeField] private string stopwatchText;
    [SerializeField] private TMP_Text flagCount;
    [SerializeField] private TMP_Text endMinesFlagged;
    
    // tutorial dialogue
    [SerializeField] private TMP_Text tutorialDialogue1;
    [SerializeField] private TMP_Text tutorialDialogue2;
    [SerializeField] private TMP_Text tutorialDialogue3;
    [SerializeField] private TMP_Text tutorialDialogue4;
    [SerializeField] private TMP_Text tutorialDialogue5;
    [SerializeField] private Image tutorialDialogueBacking;
    [SerializeField] private GameObject tutorialEndScreen;

    // end screen assets
    [SerializeField] private TMP_Text yourTime;
    [SerializeField] public Image bronzeMedal;
    [SerializeField] public Image silverMedal;
    [SerializeField] public Image goldMedal;

    // these are the variables that affect game flow and difficulty.
    [SerializeField] public static bool showLossEndScreen = false; // off by default, changes to true after delay finishes.
    [SerializeField] public static bool showWinEndScreen = false;
    [SerializeField] public static bool gameOver = false; // off by default, runs a function when changed to true.
    [SerializeField] public static bool gameWon = false; 
    [SerializeField] public static int difficulty; // affects the size of the board and the times to beat on time trial mode.
    [SerializeField, Min(0)] public static int gridSize = 16; // the size of one dimension of the grid (tiles).
    [SerializeField] private int gridOffset; // ensures that the game board is in the center of the screen, regardless of size.
    [SerializeField] private float tempRandom; // a spare float slot to allow random numbers to generate in.
    [SerializeField] public static int mineChance = 25; // the % chance that any given tile will be a mine on generation.
    [SerializeField] private int numFlags = 0; // the number of flags that a player has.
    [SerializeField] private int numMines = 0; // the number of mines generated on the board.
    [SerializeField] private int minesFlagged = 0; // the number of mines flagged by the player.
    [SerializeField] public bool stopwatchRunning = false; // toggles the stopwatch on and off.
    [SerializeField] private float timeElapsed; // the time used for the stopwatch.
    [SerializeField] private int tutorialPhase = 1; // determines which cells to highlight in the tutorial.
    [SerializeField] private int tutorialProgress = 0;

    // these are all of the unique sprites that a game of Minesweeper uses.
    [SerializeField] private Tile tileEmpty;
    [SerializeField] private Tile tileMine;
    [SerializeField] private Tile tileFlag;
    [SerializeField] private Tile tileUnknown;
    [SerializeField] private Tile tileHighlightedUnknown;
    [SerializeField] private Tile tileHighlightedFlag;
    [SerializeField] private Tile tileExploded;
    [SerializeField] private Tile tileNum1;
    [SerializeField] private Tile tileNum2;
    [SerializeField] private Tile tileNum3;
    [SerializeField] private Tile tileNum4;
    [SerializeField] private Tile tileNum5;
    [SerializeField] private Tile tileNum6;
    [SerializeField] private Tile tileNum7;
    [SerializeField] private Tile tileNum8;
    
    // creates a dictionary that will house the location of each new tile created.
    private readonly Dictionary<Vector3Int, Cell> cellRegistry = new();

    // this function determines which sprite a tile will be using, based on the value of the attributes the tile has.
    private Tile GetTileForCellType(Cell cell)
    {
        if (!cell.IsUnknown && !cell.IsMined) // switch case for the adjacency tiles
        {
            switch (cell.Adjacency){
                case 0:
                    return tileEmpty;
                case 1:
                    return tileNum1;
                case 2:
                    return tileNum2;
                case 3:
                    return tileNum3;
                case 4:
                    return tileNum4;
                case 5:
                    return tileNum5;
                case 6:
                    return tileNum6;
                case 7:
                    return tileNum7;
                case 8:
                    return tileNum8;
                default:
                    Debug.LogError("Adjacency is lower than 0, or greater than 8"); // should never occur, but just in case
                    return null;
            }
        }
        
        else if (cell.gameOver && !cell.IsExploded && cell.IsMined) // if a mine has been clicked, the tile is NOT the mine clicked, and the tile is mined:
        {
            return tileMine;
        }
        else if (cell.isHighlighted && cell.IsUnknown && !cell.IsFlagged){ // if the tile is unknown and not flagged and is highlighted (tutorial):
            return tileHighlightedUnknown;
        }
        else if (cell.IsUnknown && !cell.IsFlagged) // if the tile is unknown, and not flagged:
        {
            return tileUnknown;
        }
        
        else if (cell.IsExploded) // if the cell has a mine in it and is clicked:
        {
            return tileExploded;
        }
        
        else if (cell.IsFlagged && cell.IsUnknown) // if the tile is unknown and is flagged:
        {
            return tileFlag;
        }
        else
        {
            // if no path can be found for the tile, logs it into the debug window.
            Debug.LogError("logic error; cannot find sprite that matches attributes");
            return null;
        }
    }    

    // the function for generating the grid of tiles.
    private void createBoard()
    {
        // these two layered loops are multiplicative to generate the grid column by column.
        for (int x = 0; x < gridSize; x++){
            for (int y = 0; y < gridSize; y++){

                // initiates the cell to go in this space as class Cell.
                Cell cell = new Cell();
                
                // the grid offset variables move the tile so that the grid is in the center of the camera.
                gridOffset = ((gridSize / 2) * -1);
                Vector3Int position = new Vector3Int(x + gridOffset, y + gridOffset, 0);
                
                // decides whether a tile will be mined or not.
                tempRandom = UnityEngine.Random.Range(1,100); // rolls a random number from 1 to 100
                if(mineChance >= tempRandom){ // if it is equal or less than the mine chance:
                    cell.IsMined = true;
                    cell.IsEmpty = false;
                    numFlags = numFlags + 1;
                    numMines = numFlags; // updates the number of mines (and the number of flags the player has at the beginning)
                }
                
                // adds the cell to the dictionary, and generates the cell onto the tilemap.
                cellRegistry.Add(position, cell);
                tilemap.SetTile(position, GetTileForCellType(cell));
            }
        }
    }

    private void createTutorial(){
        // same nested loop as the regular game's creation
        for(int x = 0; x < gridSize; x++){
            for(int y = 0; y < gridSize; y++){
                Cell cell = new Cell();
                gridOffset = ((gridSize / 2) * -1);
                Vector3Int position = new Vector3Int(x + gridOffset, y + gridOffset, 0);

                // specified positions for mines and the first highlighted cell.
                Vector3Int highlightPos = new Vector3Int(0,0,0);
                Vector3Int minePos1 = new Vector3Int(-1,2,0);
                Vector3Int minePos2 = new Vector3Int(1,-2,0);
                Vector3Int minePos3 = new Vector3Int(2,-2,0);

                // if the current position is where a mine needs to be, place a mine there.
                if(position == minePos1 | position == minePos2 | position == minePos3){
                    cell.IsMined = true;
                    numFlags += 1;
                    numMines += 1;
                }
                // if the position is where the first highlighted cell should be, highlight that cell.
                if(position == highlightPos){
                    cell.isHighlighted = true;
                }

                // log the cell in the dictionary and update its sprite on the tilemap.
                cellRegistry.Add(position, cell);
                
                tilemap.SetTile(position, GetTileForCellType(cell));
            }
        }

        // hide the stopwatch and show the tutorial backing and the first bit of dialogue.
        tutorialDialogueBacking.enabled = true;
        stopwatchIcon.enabled = false;
        stopwatch.enabled = false;
        tutorialDialogue1.enabled = true;
    }
    
    private void CheckMine(Cell checkcell, Cell cell) // an optimization function for checking if a cell has a mine in a given adjacent cell.
    {
        if (checkcell.IsMined){
            cell.Adjacency = cell.Adjacency + 1;
            return;
        }
    }

    // this function is called when the start button is clicked on the menu.
    public void StartGame()
    {
        // resets/hides game assets/variables
        tutorialPhase = 1;
        tutorialProgress = 0;
        tutorialDialogueBacking.enabled = false;
        tutorialDialogue1.enabled = false;
        tutorialDialogue2.enabled = false;
        tutorialDialogue3.enabled = false;
        tutorialDialogue4.enabled = false;
        tutorialDialogue5.enabled = false;
        tutorialEndScreen.SetActive(false);
        stopwatch.enabled = true;
        stopwatchIcon.enabled = true;
        stopwatch.text = "00:00";
        numMines = 0;
        numFlags = 0;
        minesFlagged = 0;
        timeElapsed = 0;
        
        // decides the value of certain game variables dependent on the difficulty of the game.
        switch(difficulty){
            case 0: // tutorial
                gridSize = 5;
                Camera.main.orthographicSize = 3f;
                createTutorial();
                return;
            case 1: // easy mode
                gridSize = 10;
                mineChance = 10;
                Camera.main.orthographicSize = 3;
                createBoard();
                return;
            case 2: // medium mode
                gridSize = 16;
                mineChance = 15;
                Camera.main.orthographicSize = 4.5f;
                createBoard();
                return;
            case 3: // hard mode
                gridSize = 24;
                mineChance = 20;
                Camera.main.orthographicSize = 6.5f;
                createBoard();
                return;
            case 4: // expert mode
                gridSize = 32;
                mineChance = 25;
                Camera.main.orthographicSize = 9;
                createBoard();
                return;
            case 5: // classic mode
                if(gridSize < 16){
                    Camera.main.orthographicSize = 4.5f;
                }
                else if(gridSize < 26){
                    Camera.main.orthographicSize = 6.5f;
                }
                else{
                    Camera.main.orthographicSize = 8.5f;
                }
                createBoard();
                return;
        }
    }

    private void GameOver()
    {
        stopwatchRunning = false; // stops the stopwatch
        endMinesFlagged.text = "Mines Flagged: "+minesFlagged.ToString()+"/"+numMines.ToString(); // generates the string for the final mines flagged text
        // recursive loop similar to that in createBoard()
        for (int x = 0; x < gridSize; x++){
            for (int y = 0; y < gridSize; y++){
                // updates a position variable to find each tile's position.
                Vector3Int position = new Vector3Int(x + gridOffset, y + gridOffset, 0);
                if (cellRegistry.TryGetValue(position, out Cell checkcell)){ // if it finds the cell in the position:
                    checkcell.gameOver = true; // changes attribute gameOver to true
                    tilemap.SetTile(position,GetTileForCellType(checkcell)); // update tile sprite
                }
            }
        }
        showLossEndScreen = true; // shows the end screen for a loss
        tutorialDialogueBacking.enabled = false; // hides the backing and dialogue if you fail the tutorial (somehow)
        tutorialDialogue4.enabled = false;
        tutorialDialogue5.enabled = false;
        cellRegistry.Clear(); // wipes down the dictionary to replay
    }

    public void GameWin() // function is called when all mines have been flagged
    {
        audioSource.PlayOneShot(audioFanfare, Buttons.volume);
        stopwatchRunning = false; // stops the stopwatch
        showWinEndScreen = true; // displays the end screen for a win
        cellRegistry.Clear(); // wipes down the dictionary to replay
        yourTime.text = "Your time: " + stopwatchText.ToString(); // generates the string for the final time display to show

        // decides which medal to display (if at all).
        if((difficulty == 1 & (timeElapsed <= 180 & timeElapsed > 120)) // difficulty is easy, time is between 03:00 and 02:01
        | (difficulty == 2 & (timeElapsed <= 435 & timeElapsed > 290)) // difficulty is medium, time is between 07:15 and 04:51
        | (difficulty == 3 & (timeElapsed <= 865 & timeElapsed > 580)) // difficulty is hard, time is between 14:25 and 09:41
        | (difficulty == 4 & (timeElapsed <= 1440 & timeElapsed > 960))){ // difficulty is expert, time is between 24:00 and 16:01
            bronzeMedal.enabled = true; // bronze medal awarded
        }
        if((difficulty == 1 & (timeElapsed <= 120 & timeElapsed > 60)) // difficulty is easy, time is between 02:00 and 01:01
        | (difficulty == 2 & (timeElapsed <= 290 & timeElapsed > 145)) // difficulty is medium, time is between 04:50 and 02:26
        | (difficulty == 3 & (timeElapsed <= 580 & timeElapsed > 290)) // difficulty is hard, time is between 09:40 and 04:51
        | (difficulty == 4 & (timeElapsed <= 960 & timeElapsed > 480))){ // difficulty is expert, time is between 16:00 and 08:01
            silverMedal.enabled = true; // silver medal awarded
        }
        if((difficulty == 1 & timeElapsed <= 60) // difficulty is easy, time is 01:00 or below
        | (difficulty == 2 & timeElapsed <= 145) // difficulty is medium, time is 02:25 or below
        | (difficulty == 3 & timeElapsed <= 290) // difficulty is hard, time is 04:50 or below
        | (difficulty == 4 & timeElapsed <= 480)){ // difficulty is expert, time is 08:00 or below
            goldMedal.enabled = true; // gold medal awarded
        }
        // displays no medal if none of the above conditions are met
    }

    public void TutorialWin()
    {
        // similar to GameWin, but without the time check, displaying the tutorial end dialogue instead.
        audioSource.PlayOneShot(audioFanfare, Buttons.volume);
        stopwatchRunning = false;
        tutorialEndScreen.SetActive(true);
        tutorialDialogue5.enabled = false;
        tutorialDialogueBacking.enabled = false;
        cellRegistry.Clear(); // wipes down the dictionary to replay
    }

    // updates which cells to highlight dependent on the phase of the tutorial
    private void UpdateTutorial(Vector3Int tilePosition){
        switch(tutorialPhase){
            case 2: // if the center cell has been swept
            tutorialDialogue1.enabled = false;
            tutorialDialogue2.enabled = true;
                HighlightCell(new Vector3Int(1,0,0));
                HighlightCell(new Vector3Int(-1,0,0));
                HighlightCell(new Vector3Int(0,1,0));
                HighlightCell(new Vector3Int(0,-1,0));
                HighlightCell(new Vector3Int(1,1,0));
                HighlightCell(new Vector3Int(1,-1,0));
                HighlightCell(new Vector3Int(-1,1,0));
                HighlightCell(new Vector3Int(-1,-1,0));
                return;
            case 3: // the surrounding tiles have been swept
                tutorialDialogue2.enabled = false;
                tutorialDialogue3.enabled = true;
                HighlightCell(new Vector3Int(-2,0,0));
                HighlightCell(new Vector3Int(-2,1,0));
                HighlightCell(new Vector3Int(-2,-1,0));
                HighlightCell(new Vector3Int(0,-2,0));
                HighlightCell(new Vector3Int(-1,-2,0));
                HighlightCell(new Vector3Int(-2,-2,0));
                HighlightCell(new Vector3Int(0,2,0));
                HighlightCell(new Vector3Int(1,2,0));
                HighlightCell(new Vector3Int(2,2,0));
                HighlightCell(new Vector3Int(2,1,0));
                HighlightCell(new Vector3Int(2,0,0));
                HighlightCell(new Vector3Int(2,-1,0));
                return;
            case 4: // all tiles surrounding empty tiles have been swept
                tutorialDialogue3.enabled = false;
                tutorialDialogue4.enabled = true;
                HighlightCell(new Vector3Int(-1,2,0));
                return;
            case 5: // top left mine has been flagged
                DeHighlightCell(new Vector3Int(-1,2,0));
                tutorialDialogue4.enabled = false;
                tutorialDialogue5.enabled = true;
                HighlightCell(new Vector3Int(1,-2,0));
                HighlightCell(new Vector3Int(2,-2,0));
                return;
            default: // should never be called
                return;
        }
    }

    // highlights a given cell.
    private void HighlightCell(Vector3Int highlightPos){
        if(cellRegistry.TryGetValue(highlightPos, out Cell highlightCell)){
            highlightCell.isHighlighted = true;
            tilemap.SetTile(highlightPos, GetTileForCellType(highlightCell));
        }
    }

    // removes highlight from a cell.
    private void DeHighlightCell(Vector3Int highlightPos){
        if(cellRegistry.TryGetValue(highlightPos, out Cell highlightCell)){
            highlightCell.isHighlighted = false;
            tilemap.SetTile(highlightPos, GetTileForCellType(highlightCell));
        }
    }

    // this function is called every frame.
    private void Update()
    {   
        // declares the position variables to be used by the click functions.
        Vector2 clickPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int tilePosition = tilemap.WorldToCell(clickPosition);
        Vector3Int up = tilePosition + new Vector3Int(0,1,0);
        Vector3Int down = tilePosition + new Vector3Int(0,-1,0);     
        Vector3Int left = tilePosition + new Vector3Int(-1,0,0);     
        Vector3Int right = tilePosition + new Vector3Int(1,0,0);     
        Vector3Int upLeft = tilePosition + new Vector3Int(-1,1,0);     
        Vector3Int upRight = tilePosition + new Vector3Int(1,1,0);     
        Vector3Int downLeft = tilePosition + new Vector3Int(-1,-1,0);     
        Vector3Int downRight = tilePosition + new Vector3Int(1,-1,0);
        
        // works out what to display on the stopwatch and displays it if the stopwatch is running
        if(stopwatchRunning){
            timeElapsed = timeElapsed + Time.deltaTime; // the time elapsed in seconds.milliseconds
            TimeSpan t = TimeSpan.FromSeconds(timeElapsed); // splitting the time into different parts
            stopwatchText = string.Format("{0:D2}:{1:D2}",t.Minutes, t.Seconds); // concatenates the time into minutes:seconds
            stopwatch.text = stopwatchText; // the text displays this concatenated string
        }

        // Updating the UI text
        flagCount.text = numFlags.ToString();

        // if the player left-clicks a tile (clearing):
        if(Input.GetMouseButtonDown(0))
        {
            // check that the cell found is in the dictionary created:
            if (!cellRegistry.TryGetValue(tilePosition, out Cell cell)){
                return;
            }
 
            // make sure that the cell is not null (which it should never be).
            if (cell == null){
                Debug.LogError($"cell clicked is null");
                return;
            }

            // if this is the first tile clicked, turns the stopwatch on.
            stopwatchRunning = true;

            // if the player clicks a non-highlighted cell whilst playing the tutorial:
            if (difficulty == 0 & !cell.isHighlighted){
                return;
            }

            // now that the cell is located, change its attributes:
            if (!cell.IsFlagged && cell.IsUnknown){
                audioSource.PlayOneShot(audioSweep, Buttons.volume);
                cell.IsUnknown = false;
                if(cell.IsMined){ // if the tile is mined:
                    cell.IsExploded = true;
                    audioSource.PlayOneShot(audioExplosion, Buttons.volume);
                    gameOver = true;
                    GameOver();
                    enabled = false;
                }
                
                // checks for adjacency. each direction (the first parameter in TryGetValue()) is a unique Vector3Int declared above.
                else{
                    if (cellRegistry.TryGetValue(up, out Cell checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(down, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(left, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(right, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(upLeft, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(upRight, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(downLeft, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    if (cellRegistry.TryGetValue(downRight, out checkcell)){
                        CheckMine(checkcell, cell);
                    }
                    // update the progress of the tutorial if the cell is highlighted.
                    if(cell.isHighlighted){
                        tutorialProgress += 1;
                        if(tutorialProgress == 1){
                        tutorialPhase = 2;
                        }
                        if(tutorialProgress == 9){
                            tutorialPhase = 3;
                        }
                        if(tutorialProgress == 21){
                            tutorialPhase = 4;
                        }
                        // update the tutorial with new highlights if the phase has updated.
                        UpdateTutorial(tilePosition);
                    }
                }
            }
            else{
                return;
            }

            // update the sprite of the cell to the correct sprite for this logic path.
            Debug.Log($"cell attributes: adjacency = {cell.Adjacency}, exploded = {cell.IsExploded}, mined = {cell.IsMined}, unknown = {cell.IsUnknown}, flagged = {cell.IsFlagged}, empty = {cell.IsEmpty}");
            Debug.Log($"cell {tilePosition} has been updated.");
            tilemap.SetTile(tilePosition, GetTileForCellType(cell));
        }


        // if the player right-clicks a tile (flagging):
        if (Input.GetMouseButtonDown(1))
        {
            // check that the cell found is in the dictionary created:
            if (!cellRegistry.TryGetValue(tilePosition, out Cell rcCell)){
                // this should never be called, but to wrap all logic paths up, log that the cell is unidentifiable.
                Debug.LogError($"no cell found for click position {tilePosition}.");
                return;
            }
 
            // make sure that the cell is not null (which it should never be).
            if (rcCell == null){
                Debug.LogError($"cell clicked is null");
                return;
            }

            // if the player clicks a non-highlighted cell whilst playing the tutorial:
            if (difficulty == 0 & !rcCell.isHighlighted){
                return;
            }

            // now that the cell is located, change its attributes
            if(rcCell.IsFlagged){ // if the cell is already flagged:
                rcCell.IsFlagged = false; // remove the flag
                numFlags = numFlags + 1; // return the flag to the counter
                Debug.Log($"Flag Retrieved! Remaining flags: {numFlags}"); // log that the flag has been retrieved
                if(rcCell.IsMined){ // if the tile was mined:
                    minesFlagged = minesFlagged - 1; // remove that mines from the count of mines flagged
                }
            }
            
            else if(rcCell.IsUnknown && numFlags != 0){ // if the tile is not flagged and you are not out of flags:
                audioSource.PlayOneShot(audioFlag, Buttons.volume);
                rcCell.IsFlagged = true; // update the tile to be flagged
                numFlags = numFlags - 1; // remove that flag from the counter
                if(rcCell.IsMined){ // if the tile is mined:
                    Debug.Log($"Mined Tile Flagged! Remaining flags: {numFlags}"); // log that a mined tile has been flagged
                    minesFlagged = minesFlagged + 1;
                }
                if(rcCell.isHighlighted & tilePosition == new Vector3Int(-1,2,0)){
                    tutorialPhase = 5;
                    UpdateTutorial(tilePosition);
                }
                else{ // if the tile is not mined:
                    Debug.Log($"Empty Tile Flagged! Remaining flags: {numFlags}"); // log that an empty tile has been flagged
                }
            }
            
            
            else{ // if there are no flags available:
                Debug.Log($"No flags available! Remaining flags: {numFlags}"); // log that the player has run out of flags
                return;
            }

            // update the sprite of the cell to the correct sprite for this logic path. returns all of the attributes of the clicked tile.
            Debug.Log($"cell attributes: adjacency = {rcCell.Adjacency}, exploded = {rcCell.IsExploded}, mined = {rcCell.IsMined}, unknown = {rcCell.IsUnknown}, flagged = {rcCell.IsFlagged}, empty = {rcCell.IsEmpty}");
            tilemap.SetTile(tilePosition, GetTileForCellType(rcCell));
            if (minesFlagged == numMines & difficulty != 0){ // if all mines have been flagged:
                flagCount.text = "0";
                GameWin();
                enabled = false;
            }
            else if(minesFlagged == numMines & difficulty == 0){ // if all mines have been flagged (tutorial):
                flagCount.text = "0";
                TutorialWin();
                enabled = false;
            }
        }
    }       
}