﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]

public class CreateScreenSpaceUILayoutByName : MonoBehaviour
{
    // this script needs to be attached to a "launcher" - an empty gameobject in the appropriate scene
    // then, the correct menu layout, or layout combinations, is built based on the gameobject's name

    // define the background image sequence for each menu or screen
    string[] loadingScreenBackgroundSlideShowSequence = { "UI/LoadingScreenBackground1", "UI/LoadingScreenBackground2" };
    string[] mainMenuBackgroundSlideShowSequence = { "UI/MainMenuBackground1", "UI/MainMenuBackground2" };
    string[] pauseMenuBackgroundSlideShowSequence = { "UI/PauseMenuBackground1", "UI/PauseMenuBackground2" };

    void Start()
    {

        // ensure there's always an EventSystem
        if (GameObject.Find("EventSystem") == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // menu UI will be nested under the launcher object
        GameObject launcher = this.transform.gameObject;

        // build the menu based on the name of the object this script is attached to
        string name = this.name;

        // loading screen
        if (name.Contains("LoadingScreenLauncher"))
        {
            Debug.Log("Building the loading screen...");

            // the loading screen is responsible for preloading the large scenes so level choice is faster
            //LoadSceneByName LoadSceneByNameScript = mainMenu.AddComponent<LoadSceneByName>();

            // main menu canvas
            GameObject loadingScreen = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "LoadingScreen");

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenBackgroundImageSlideshow(loadingScreen, loadingScreenBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(loadingScreen);

            // create the title bar container
            GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(loadingScreen, logoHeader, "Loading...");
        }

        // the main menu
        else if (name.Contains("MainMenuLauncher"))
        {
            Debug.Log("Building the Main Menu...");

            // main menu canvas
            GameObject mainMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "MainMenu");

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenBackgroundImageSlideshow(mainMenu, mainMenuBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(mainMenu);

            // create the title bar container
            GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(mainMenu, logoHeader, "Choose a time and place:");

            // time & place picker and container
            GameObject mainMenuCentralNav = CreateScreenSpaceUIElements.CreateMainMenuCentralNav(mainMenu, titleBarContainer);
        }

        // pause menu
        else if (name.Contains("PauseMenuLauncher"))
        {
            Debug.Log("Building the Pause Menu...");

            // pause menu canvas
            GameObject pauseMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "PauseMenu");

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenBackgroundImageSlideshow(pauseMenu, pauseMenuBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(pauseMenu);

            // create the title bar container
            GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(pauseMenu, logoHeader, "Pause");

            // time travel column and pause menu buttons
            GameObject pauseMenuCentralNav = CreateScreenSpaceUIElements.CreatePauseMenuCentralNav(pauseMenu, titleBarContainer);
        }

        // otherwise, no UI will be built because the name wasn't found or recognized
        else
        {
            Debug.Log("Unknown UI type! " + name);
        }
    }
}
 
 
 
 
 