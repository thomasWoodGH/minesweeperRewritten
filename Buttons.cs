using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class Buttons : MonoBehaviour
{    
    // affects entire game
    [SerializeField] public static float volume = 1;
    [SerializeField] private static int currentMenu = 0;
    
    // background for all menus
    [SerializeField] private Image background;

    // title card buttons
    [SerializeField] private GameObject playButton;
    
    // buttons that start the game
    [SerializeField] private GameObject ttStartGameButton;   
    [SerializeField] private GameObject classicStartGameButton;
    [SerializeField] private GameObject tutorialStartGameButton;

    // buttons that adjust game aspects
    [SerializeField] private GameObject easyButton;
    [SerializeField] private GameObject mediumButton;
    [SerializeField] private GameObject hardButton;
    [SerializeField] private GameObject expertButton;
    [SerializeField] private Slider gridSizeSlider;
    [SerializeField] private Slider mineChanceSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text gridSizeValue;
    [SerializeField] private TMP_Text mineChanceValue;
    [SerializeField] private TMP_Text volumeValue;
    [SerializeField] private double volumePercentage;

    // buttons that select menus
    [SerializeField] private GameObject classicModeButton;
    [SerializeField] private GameObject timeTrialModeButton;
    [SerializeField] private GameObject tutorialButton;
    [SerializeField] private GameObject settingsButton;

    // back/exit buttons
    [SerializeField] private GameObject ttBackButton;
    [SerializeField] private GameObject exitButton;

    // object folders to hide/show
    [SerializeField] private GameObject titleCardMenu;
    [SerializeField] private GameObject classicMenu;
    [SerializeField] private GameObject timeTrialMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject tutorialMenu;
    [SerializeField] private GameObject winEndScreen;
    [SerializeField] private GameObject lossEndScreen;
    [SerializeField] private GameObject quitConfirmScreen;
    [SerializeField] private Tilemap tilemap;

    // difficulty backings to hide/show
    [SerializeField] private Image easySelection;
    [SerializeField] private Image mediumSelection;
    [SerializeField] private Image hardSelection;
    [SerializeField] private Image expertSelection;

    // game assets/affectors
    [SerializeField] public Grid gridScript;   
    [SerializeField] private TMP_Text stopwatch;
    [SerializeField] private TMP_Text flagCount;    
    [SerializeField] private RawImage stopwatchIcon;
    [SerializeField] private Image flagIcon;
    [SerializeField] public Image bronzeMedal;
    [SerializeField] public Image silverMedal;
    [SerializeField] public Image goldMedal;
    [SerializeField] public AudioSource audioSource;

    // menu click sound effect
    [SerializeField] public AudioClip audioClick;
    [SerializeField] public AudioClip audioAwake;

    // this button is for the dev build only
    [SerializeField] private GameObject gameWinButton;

    // this function is called on start-up
    private void Start(){
        gridScript=GameObject.FindGameObjectWithTag("GridTag").GetComponent<Grid>(); // grabs the Grid script from the object that has the tag specified (game canvas)
        lossEndScreen.SetActive(false); // hides the win and loss screens
        winEndScreen.SetActive(false);
        quitConfirmScreen.SetActive(false); // hides the quit confirm screen
        goldMedal.enabled = false; // hides the medals
        silverMedal.enabled = false;
        bronzeMedal.enabled = false;
    }

    public void OnSliderVolumeChanged(){
        audioSource.PlayOneShot(audioClick, volume);
    }
    // initial play button. moves to the main menu.
    public void OnClickPlay(){
        audioSource.PlayOneShot(audioClick, volume);
        currentMenu = 1;
    }

    // time trial mode button on main menu. moves to time trial menu.
    public void OnClickTimeTrial(){
        audioSource.PlayOneShot(audioClick, volume);
        currentMenu = 2;
    }

    // classic mode button on the main menu. sets the difficulty to the integer 
    // associated and moves to the classic mode menu.
    public void OnClickClassicMode(){
        audioSource.PlayOneShot(audioClick, volume);
        Grid.difficulty = 5;
        currentMenu = 3;
    }

    // settings button on the main menu. moves to settings menu.
    public void OnClickSettings(){
        audioSource.PlayOneShot(audioClick, volume);
        currentMenu = 4;
    }

    // tutorial button on the main menu. moves to the tutorial menu.
    public void OnClickTutorial(){
        audioSource.PlayOneShot(audioClick, volume);
        Grid.difficulty = 0;
        currentMenu = 5;
    }

    // any given back button. returns to the main menu, resets bools for the game's progression, 
    // hides objects on the end screens and clears the tilemap.
    public void OnClickBack(){
        audioSource.PlayOneShot(audioClick, volume);
        tilemap.ClearAllTiles();
        Grid.gameWon = false;
        Grid.gameOver = false;
        Grid.showLossEndScreen = false;
        Grid.showWinEndScreen = false;
        goldMedal.enabled = false;
        silverMedal.enabled = false;
        bronzeMedal.enabled = false;
        winEndScreen.SetActive(false);
        lossEndScreen.SetActive(false);
        currentMenu = 1;
    }

    // easy mode button on the time trial menu. changes the difficulty to easy.
    public void OnClickEasy(){
        audioSource.PlayOneShot(audioClick, volume);
        Grid.difficulty = 1;
    }

    // medium mode button on the time trial menu. changes the difficulty to medium.
    public void OnClickMedium(){
        audioSource.PlayOneShot(audioClick, volume);
        Grid.difficulty = 2;
    }

    // hard mode button on the time trial menu. changes the difficulty to hard.
    public void OnClickHard(){
        audioSource.PlayOneShot(audioClick, volume);
        Grid.difficulty = 3;
    }

    // expert mode button on the time trial menu. changes the difficulty to expert.
    public void OnClickExpert(){
        audioSource.PlayOneShot(audioClick, volume);
        Grid.difficulty = 4;
    }

    // start game button on the classic mode menu. sets the game variables to the ones specified in the sliders, toggles on the Update loop in the Grid script,
    // moves to the game menu and calls the game start function.
    public void OnClickClassicStart(){
        audioSource.PlayOneShot(audioClick, volume);
        currentMenu = 6;
        Grid.mineChance = (int)mineChanceSlider.value;
        Grid.gridSize = (int)gridSizeSlider.value;
        gridScript.enabled = true;
        gridScript.StartGame();
    }

    // start game button on the time trial mode menu. does nothing if the difficulty is not between 1 and 4 (difficulty not selected). moves to the game menu,
    // toggles on the update loop in the Grid script, and calls the start game function.
    public void OnClickTTStart(){
        audioSource.PlayOneShot(audioClick, volume);
        if(Grid.difficulty < 1 | Grid.difficulty > 4){
            return;
        }
        currentMenu = 6;
        gridScript.enabled = true;
        gridScript.StartGame();
    }

    // start game button on the tutorial menu. moves to the game menu, toggles on the update loop in the Grid script, and calls the start game function.
    public void OnClickTutorialStart(){
        audioSource.PlayOneShot(audioClick, volume);
        currentMenu = 6;
        gridScript.enabled = true;
        gridScript.StartGame();
    }

    // exit button on the main menu and title card.
    public void OnClickExit(){
        audioSource.PlayOneShot(audioClick, volume);
        quitConfirmScreen.SetActive(true);
    }

    // no button on the quit confirm screen. closes the screen.
    public void OnClickNo(){
        audioSource.PlayOneShot(audioClick, volume);
        quitConfirmScreen.SetActive(false);
    }

    // yes button on the quit confirm screen. kills the program.
    public void ExitGame(){
        audioSource.PlayOneShot(audioClick, volume);
        Application.Quit();
    }

    private void Update(){

        // Updates the volume and text on the Settings Menu.
        volume = (volumeSlider.value / 100); // changes the volume from a percentage to a decimal.
        volumeValue.text = volumeSlider.value.ToString() + "%"; 

        // Updates the text stating your selected values in the Classic Mode Menu.
        gridSizeValue.text = gridSizeSlider.value.ToString();
        mineChanceValue.text = $"{mineChanceSlider.value.ToString()}%";

        // decides which menu UI to hide and show.
        switch(currentMenu){
            case 0: // title card
                // active menu
                titleCardMenu.SetActive(true);
                
                timeTrialMenu.SetActive(false);
                gameScreen.SetActive(false);
                mainMenu.SetActive(false);
                tutorialMenu.SetActive(false);
                classicMenu.SetActive(false);
                lossEndScreen.SetActive(false);
                settingsMenu.SetActive(false);
                easySelection.enabled = false;
                mediumSelection.enabled = false;
                hardSelection.enabled = false;
                expertSelection.enabled = false;
                break;

            case 1: // main menu
                // active menu
                mainMenu.SetActive(true);

                timeTrialMenu.SetActive(false);
                gameScreen.SetActive(false);
                titleCardMenu.SetActive(false);
                tutorialMenu.SetActive(false);
                classicMenu.SetActive(false);
                lossEndScreen.SetActive(false);
                settingsMenu.SetActive(false);
                easySelection.enabled = false;
                mediumSelection.enabled = false;
                hardSelection.enabled = false;
                expertSelection.enabled = false;
                break;

            case 2: // time trial menu
                // active menu
                timeTrialMenu.SetActive(true);
                
                titleCardMenu.SetActive(false);
                gameScreen.SetActive(false);
                mainMenu.SetActive(false);
                tutorialMenu.SetActive(false);
                classicMenu.SetActive(false);
                lossEndScreen.SetActive(false);
                settingsMenu.SetActive(false);

                // decides which difficulty selection to show
                switch(Grid.difficulty){
                    case 0: // tutorial
                        easySelection.enabled = false;
                        mediumSelection.enabled = false;
                        hardSelection.enabled = false;
                        expertSelection.enabled = false;
                        return;
                    case 1: // easy
                        easySelection.enabled = true;
                        mediumSelection.enabled = false;
                        hardSelection.enabled = false;
                        expertSelection.enabled = false;
                        return;
                    case 2: // medium
                        easySelection.enabled = false;
                        mediumSelection.enabled = true;
                        hardSelection.enabled = false;
                        expertSelection.enabled = false;
                        return;
                    case 3: // hard
                        easySelection.enabled = false;
                        mediumSelection.enabled = false;
                        hardSelection.enabled = true;
                        expertSelection.enabled = false;
                        return;
                    case 4: // expert
                        easySelection.enabled = false;
                        mediumSelection.enabled = false;
                        hardSelection.enabled = false;
                        expertSelection.enabled = true;
                        return;
                    case 5: // classic mode
                        easySelection.enabled = false;
                        mediumSelection.enabled = false;
                        hardSelection.enabled = false;
                        expertSelection.enabled = false;
                        return;
                    default:
                        return;
                }

            case 3: // classic menu
                // active menu
                classicMenu.SetActive(true);

                timeTrialMenu.SetActive(false);
                titleCardMenu.SetActive(false);
                gameScreen.SetActive(false);
                mainMenu.SetActive(false);
                tutorialMenu.SetActive(false);
                lossEndScreen.SetActive(false);
                settingsMenu.SetActive(false);
                easySelection.enabled = false;
                mediumSelection.enabled = false;
                hardSelection.enabled = false;
                expertSelection.enabled = false;
                break;

            case 4: // settings menu
                // active menu
                settingsMenu.SetActive(true);

                classicMenu.SetActive(false);
                timeTrialMenu.SetActive(false);
                titleCardMenu.SetActive(false);
                gameScreen.SetActive(false);
                mainMenu.SetActive(false);
                tutorialMenu.SetActive(false);
                lossEndScreen.SetActive(false);
                easySelection.enabled = false;
                mediumSelection.enabled = false;
                hardSelection.enabled = false;
                expertSelection.enabled = false;
                break;

            case 5: // tutorial menu
                // active menu
                tutorialMenu.SetActive(true);

                titleCardMenu.SetActive(false);
                mainMenu.SetActive(false);
                gameScreen.SetActive(false);
                timeTrialMenu.SetActive(false);
                classicMenu.SetActive(false);
                lossEndScreen.SetActive(false);
                settingsMenu.SetActive(false);
                easySelection.enabled = false;
                mediumSelection.enabled = false;
                hardSelection.enabled = false;
                expertSelection.enabled = false;
                break;

            case 6: // game screen
                // active menu
                gameScreen.SetActive(true);

                titleCardMenu.SetActive(false);
                timeTrialMenu.SetActive(false);
                mainMenu.SetActive(false);
                tutorialMenu.SetActive(false);
                classicMenu.SetActive(false);
                settingsMenu.SetActive(false);
                easySelection.enabled = false;
                mediumSelection.enabled = false;
                hardSelection.enabled = false;
                expertSelection.enabled = false;
                break;

            default:
                break;
        }

        // hides the menu background if a round is running, and shows if not.
        if(currentMenu == 6){
            background.enabled = false;
        }
        else{
            background.enabled = true;
        }

        // if the bool to show each end screen is true, shows it.
        if(Grid.showLossEndScreen){
            lossEndScreen.SetActive(true);
        }
        else if(Grid.showWinEndScreen){
            winEndScreen.SetActive(true);
        }

        // if the escape key is pressed, open the quit confirm screen.
        if(Input.GetKeyDown("escape")){
            quitConfirmScreen.SetActive(true);
        }
    }
}

