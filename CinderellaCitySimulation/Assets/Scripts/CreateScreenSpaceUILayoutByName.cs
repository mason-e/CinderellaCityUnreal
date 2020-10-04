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
    // then, the correct menu layout or layout combinations will be built based on the launcher's name

    // define the background image sequence for each menu or screen
    string[] loadingScreenBackgroundSlideShowSequence = { "UI/LoadingScreenBackground1", "UI/LoadingScreenBackground2" };
    string[] mainMenuBackgroundSlideShowSequence = { "UI/MainMenuBackground1", "UI/MainMenuBackground2" };

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
            Utils.DebugUtils.DebugLog("Building the loading screen...");

            // the loading screen is responsible for preloading the large scenes so level choice is faster

            // loading screen canvas
            GameObject loadingScreen = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "LoadingScreen");

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenImageSlideshow(loadingScreen, loadingScreenBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(loadingScreen);

            // create the title bar container
            GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(loadingScreen, logoHeader, "Building Cinderella City...");

            // create the game version indicator
            GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(loadingScreen);
        }

        // the main menu
        else if (name.Contains("MainMenuLauncher"))
        {
            Utils.DebugUtils.DebugLog("Building the Main Menu...");

            // main menu canvas
            GameObject mainMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "MainMenu");

            // background image slideshow
            GameObject backgroundSlideShow = CreateScreenSpaceUIElements.CreateFullScreenImageSlideshow(mainMenu, mainMenuBackgroundSlideShowSequence);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(mainMenu);

            // create the title bar container
            GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(mainMenu, logoHeader, "Choose a time and place:");

            // time & place picker and container
            GameObject mainMenuCentralNav = CreateScreenSpaceUIElements.CreateMainMenuCentralNav(mainMenu, titleBarContainer);

            // create the game version indicator
            GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(mainMenu);
        }

        // pause menu
        else if (name.Contains("PauseMenuLauncher"))
        {
            Utils.DebugUtils.DebugLog("Building the Pause Menu...");

            // pause menu canvas
            GameObject pauseMenu = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "PauseMenu");

            // background image
            GameObject backgroundImage = CreateScreenSpaceUIElements.CreateFullScreenImageFromCameraTexture(pauseMenu, true);

            // project logo and container
            GameObject logoHeader = CreateScreenSpaceUIElements.CreateLogoHeader(pauseMenu);

            // create the title bar container
            GameObject titleBarContainer = CreateScreenSpaceUIElements.CreateMenuTitleBar(pauseMenu, logoHeader, "Pause");

            // time travel column and pause menu buttons
            GameObject pauseMenuCentralNav = CreateScreenSpaceUIElements.CreatePauseMenuCentralNav(pauseMenu, titleBarContainer);

            // create the game version indicator
            GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(pauseMenu);
        }

        // generic UI launcher in FPSController scenes
        // used here to display persistent HUD UI
        else if (name.Contains("UILauncher"))
        {
            Utils.DebugUtils.DebugLog("Building the Heads Up Display...");

            // Heads Up Display canvas
            GameObject HUDCanvas = CreateScreenSpaceUIElements.CreateMenuCanvas(launcher, "HUD");

            // create the time period indicator
            GameObject HUDTimePeriodIndicator = CreateScreenSpaceUIElements.CreateHUDTimePeriodIndicator(HUDCanvas, StringUtils.ConvertSceneNameToFriendlyName(this.gameObject.scene.name));

            // create the time period notification
            GameObject HUDTimePeriodNotificationContainer = CreateScreenSpaceUIElements.CreateHUDTimePeriodNotification(HUDCanvas, StringUtils.ConvertSceneNameToFriendlyName(this.gameObject.scene.name));

            // create the game version indicator
            GameObject versionIndicator = CreateScreenSpaceUIElements.CreateVersionLabel(HUDCanvas);

            // if this is the 60s70s scene, note that it's under construction
            if (this.gameObject.scene.name.Contains("AltFuture"))
            {
                CreateScreenSpaceUIElements.CreateHUDUnderConstructionLabel(HUDCanvas, "/// Under Construction ///");
            }
        }

        // otherwise, no UI will be built because the name wasn't found or recognized
        else
        {
            Utils.DebugUtils.DebugLog("Unknown UI type! " + name);
        }
    }
}
 
 
 
 
 