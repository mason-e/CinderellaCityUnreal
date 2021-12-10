﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Button))]

/// <summary>
/// Builds and positions all screen-space objects (UI)
/// </summary>

// holds values other scripts need to access
public class UIGlobals
{
    // all UI sprites stored in a file live here
    public static string projectUIPath = "Assets/Resources/UI/";

    // is used to determine when to generate time-travel specific thumbnails
    public static bool isTimeTravelThumbnail;

    // keep track of the UI elements that get dynamically updated when Pause is invoked
    public static GameObject pauseMenuBackgroundImage;
    public static List<GameObject> timeTravelThumbnails = new List<GameObject>();

    // define the various camera textures
    // this is always written when an FPSController is disabled
    public static Texture2D outgoingFPSControllerCameraTexture = new Texture2D(Screen.width, Screen.height);
    // these are the various textures that are used in the UI
    public static Texture2D pauseMenuBackgroundCameraTexture = new Texture2D(Screen.width, Screen.height);
    public static Texture2D FPSController60s70sCameraTexture = new Texture2D(Screen.width, Screen.height);
    public static Texture2D FPSController80s90sCameraTexture = new Texture2D(Screen.width, Screen.height);

    // these are the HUD UI elements that can be hidden/revealed
    public static GameObject underConstructionLabelContainer;
    public static GameObject currentTimePeriodNotificationContainer;
    public static GameObject timePeriodNotificationContainer60s70s;
    public static GameObject timePeriodNotificationContainer80s90s;
    public static GameObject timePeriodNotificationContainerAltFuture;
    public static GameObject timePeriodNotificationContainerExperimental;

    /// colors ///

    // text white
    public static Color32 typicalTextColor = new Color32(255, 255, 255, 255);

    // text grey
    public static Color32 subtleTextColor = new Color32(150, 150, 150, 255);

    // all button colors
    public static Color32 buttonColor = new Color32(20, 20, 20, 220);

    // all nav and container colors
    public static Color32 containerColor = new Color32(50, 50, 50, 100);

    // clear color
    public static Color32 clearColor = new Color32(255, 255, 255, 0);

    /// sizes ///

    // fonts and text sizes
    public static string labelFont = "AvenirNextLTPro-Demi";
    // the proportion of the text height that the descender is estimated to be
    public static float textDescenderProportion = 0.12f;

    public static float HUDTimePeriodLabelSize = 0.037f;
    public static float versionLabelSize = 0.014f;

    public static float menuTitleLabelSize = 0.028f;
    public static float mainMenuTextButtonLabelSize = 0.032f;
    public static float placeLabelSize = 0.046f;
    public static float timeLabelSize = 0.046f;

    public static float toggleGroupLabelSize = 0.023f;
    public static float toggleLabelSize = 0.019f;

    public static int visibilityMenuTextButtonlabelSize = 20;

    /// spacing, padding, margins ///

    // button sizes (ratio relative to screen size)
    public static float menuButtonScreenWidthRatio = 0.15f;
    public static float menuButtonTopBottomPaddingScreenHeightRatio = 0.01f;

    // sets of toggles or buttons
    public static float toggleContainerPadding = 0.01f;
    public static float toggleContainerMaxWidthScreenWidthRatio = 0.1f;
}

public class CreateScreenSpaceUIElements : MonoBehaviour
{
    // this is a single stack of place/time thumbnails for aligning time labels to
    public static List<GameObject> placeThumbnailsForAlignment = new List<GameObject>();
    public static List<GameObject> timeLabelsForAlignment = new List<GameObject>();

    // TODO: move these into UIGlobals

    /// space/padding/margin ///

    public static float centralNavPadding = 0.02f;

    public static float projectLogoHeightScreenHeightRatio = 0.18f;
    public static float projectLogoLeftMarginScreenWidthRatio = 0.1f;
    public static float projectLogoTopMarginScreenHeightRatio = 0.07f;
    public static float projectLogoContainerTopPaddingScreenHeightRatio = 0.02f;
    public static float projectLogoContainerBottomPaddingScreenHeightRatio = 0.02f;
    public static float projectLogoContainerRightPaddingScreenWidthRatio = 0.02f;

    public static float menuTitleTopMarginScreenHeightRatio = -0.02f;
    public static float menuTitleLeftMarginScreenWidthRatio = -0.02f;
    public static float menuTitleBottomMarginScreenHeightRatio = 0.02f;

    public static float toggleTopPaddingScreenHeightRatio = -0.01f;
    public static float toggleLeftPaddingScreenWidthRatio = -0.01f;
    public static float toggleBottomPaddingScreenHeightRatio = 0.01f;
    public static float toggleBottomMarginScreenHeightRatio = 0.02f;

    public static float logoHeaderBottomMarginScreenHeightRatio = 0.05f;

    public static float placeLabelTopMarginScreenHeightRatio = -0.02f;
    public static float placeLabelBottomMarginScreenHeightRatio = 0.01f;

    public static float timeLabelLeftMarginScreenWidthRatio = -0.02f;

    public static float timePlaceThumbnailHeightScreenHeightRatio = 0.2f;
    public static float timePlaceThumbnailBottomMarginScreenHeightRatio = 0.01f;

    public static float navContainerTopMarginScreenHeightRatio = 0.01f;
    public static float navContainerLeftMarginScreenWidthRatio = 0.1f;
    public static float navContainerBottomMarginScreenHeightRatio = 0.07f;

    public static float thumbnailStackBottomMarginScreenHeightRatio = 0.02f;

    public static float textButtonBottomMarginScreenHeightRatio = 0.015f;
    public static float textButtonLeftMarginScreenWidthRatio = 0.01f;

    public static float HUDBottomBarTopMarginScreenHeightRatio = 0.9f;
    public static float HUDBottonBarHeightScreenHeightRatio = 0.08f;
    public static float HUDTimePeriodLabelLeftMarginScreenWidthRatio = 0.85f;
    public static float HUDBottomBarBottomMarginScreenHeightRatio = 0.03f;

    public static float versionLabelLeftMarginScreenWidthRatio = 0.008f;
    public static float versionLabelTopMarginScreenHeightRatio = 0.98f;

    // fonts need to scale with the screen resolution
    // but need to be specified in pixel values
    public static int ConvertFontHeightRatioToPixelValue(float heightRatio)
    {
        float pixelValue = Screen.height * heightRatio;

        return Mathf.RoundToInt(pixelValue);
    }

    // create an empty list of GameObjects that need to be dynamically added to their parent
    // this list will be emptied and populated in UI constructors that make nested sets of objects
    public static List<GameObject> orphanedThumbnailStacks = new List<GameObject>();

    // assigns a list of orphaned objects to a parent
    public static void AssignOrphanedObjectListToParent(List<GameObject> orphanObjects, GameObject parent)
    {
        foreach (GameObject orphan in orphanObjects)
        {
            orphan.transform.SetParent(parent.transform);
        }
    }

    // write a camera's view to a global texture, depending on which FPSController this is
    public static void AssignCameraTextureToVariableByName()
    {
        // use the name of the active FPSController to determine which variable to write the texture to
        switch (ManageFPSControllers.FPSControllerGlobals.activeFPSController.name)
        {
            case string FPSControllerName when FPSControllerName.Contains("60s70s"):
                UIGlobals.FPSController60s70sCameraTexture = UIGlobals.outgoingFPSControllerCameraTexture;
                return;
            case string FPSControllerName when FPSControllerName.Contains("80s90s"):
                UIGlobals.FPSController80s90sCameraTexture = UIGlobals.outgoingFPSControllerCameraTexture;
                return;
        }
    }

    // determine which camera texture to read to or write from, depending on the requesting UI object's name
    public static Texture2D AssociateCameraTextureByName(string name)
    {
        switch (name)
        {
            case string imageHostName when imageHostName.Contains("PauseMenu"):
                return UIGlobals.outgoingFPSControllerCameraTexture;
            case string imageHostName when imageHostName.Contains("60s70s"):
                return UIGlobals.FPSController60s70sCameraTexture;
            case string imageHostName when imageHostName.Contains("80s90s"):
                return UIGlobals.FPSController80s90sCameraTexture;
            default:
                return null;
        }
    }

    // determine which camera texture to use for the pause menu background image
    public static Texture2D GetPauseMenuBackgroundImage(string referringSceneName)
    {
        switch (referringSceneName)
        {
            case string name when name.Contains("60s70s"):
                return UIGlobals.FPSController60s70sCameraTexture;
            case string name when name.Contains("80s90s"):
                return UIGlobals.FPSController80s90sCameraTexture;
            default:
                return UIGlobals.outgoingFPSControllerCameraTexture;
        }
    }

    // get the time travel notification container for each scene
    public static GameObject GetTimePeriodNotificationContainerByName(string sceneName)
    {
        switch (sceneName)
        {
            case string scenePartialName when scenePartialName.Contains("60s70s"):
                return UIGlobals.timePeriodNotificationContainer60s70s;
            case string scenePartialName when scenePartialName.Contains("80s90s"):
                return UIGlobals.timePeriodNotificationContainer80s90s;
            case string scenePartialName when scenePartialName.Contains("AltFuture"):
                return UIGlobals.timePeriodNotificationContainerAltFuture;
            case string scenePartialName when scenePartialName.Contains("Experimental"):
                return UIGlobals.timePeriodNotificationContainerExperimental;
            default:
                return null;
        }
    }

    // set the time travel notification container for each scene (invoked when the UI is built)
    public static GameObject SetTimePeriodNotificationContainerByName(GameObject containerToStore, string sceneName)
    {
        switch (sceneName)
        {
            case string scenePartialName when scenePartialName.Contains("60s70s"):
                UIGlobals.timePeriodNotificationContainer60s70s = containerToStore;
                return containerToStore;
            case string scenePartialName when scenePartialName.Contains("80s90s"):
                UIGlobals.timePeriodNotificationContainer80s90s = containerToStore;
                return containerToStore;
            case string scenePartialName when scenePartialName.Contains("AltFuture"):
                UIGlobals.timePeriodNotificationContainerAltFuture = containerToStore;
                return containerToStore;
            case string scenePartialName when scenePartialName.Contains("Experimental"):
                UIGlobals.timePeriodNotificationContainerExperimental = containerToStore;
                return containerToStore;
            default:
                return null;
        }
    }

    // takes the active FPSController, adds the render camera to image script, and renders the camera
    public static void CaptureActiveFPSControllerCamera()
    {
        // get the camera from the active FPSController
        Camera FPSCamera = ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform.GetChild(0).gameObject.GetComponent<Camera>();

        // attach and kick off the render script
        RenderCameraToImageSelfDestruct renderCameraScript = FPSCamera.gameObject.AddComponent<RenderCameraToImageSelfDestruct>();
        // set the override to true, so this will run once in game mode
        renderCameraScript.runInGameMode = true;
        // render the camera
        FPSCamera.Render();
    }

    // captures FPSCharacter cameras, then refreshes their global textures, so the pause menu can be updated
    public static void CaptureDisabledSceneFPSCameras()
    {
        // define the time periods that are considered disabled at this time
        List<string> disabledTimePeriodSceneNames = ManageScenes.GetDisabledTimePeriodSceneNames(SceneGlobals.referringSceneName);
        //Utils.DebugUtils.DebugLog("Disabled time period scene names: " + disabledTimePeriodSceneNames);

        // for each inactive time period, enable it without scripts, take a screenshot, and update textures
        foreach (string disabledSceneName in disabledTimePeriodSceneNames)
        {
            // toggle the scene, but ignore scriptHost objects - so no scripts or behaviors enable
            ToggleSceneAndUI.ToggleSceneObjectsOnExceptScriptHosts(disabledSceneName);

            // adjust the FPSController transform to account for hoisting
            Transform adjustedFPSControllerTransform = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform;
            adjustedFPSControllerTransform.position = HoistSceneObjects.AdjustPositionForHoistBetweenScenes(adjustedFPSControllerTransform.position, SceneGlobals.referringSceneName, disabledSceneName);

            // relocate the disabled FPSController to the correct position for a screenshot
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(adjustedFPSControllerTransform);

            // inherit the sun settings of the disabled scene, in order to generate an accurate screenshot
            Light sunToInherit = ManageSunSettings.GetSunBySceneName(disabledSceneName);
            Light sunToOverwrite = ManageSunSettings.GetSunBySceneName(SceneManager.GetActiveScene().name);
            ManageSunSettings.InheritSunSettingsBySceneName(disabledSceneName, sunToInherit, sunToOverwrite);

            // briefly set this as the active scene to update the skybox settings
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(disabledSceneName));

            // capture the current camera
            // this will also update the global variable with the texture, depending on the scene name
            CaptureActiveFPSControllerCamera();

            // reverse the position adjustment after camera is captured
            adjustedFPSControllerTransform.position = HoistSceneObjects.AdjustPositionForHoistBetweenScenes(adjustedFPSControllerTransform.position, disabledSceneName, SceneGlobals.referringSceneName);

            // relocate the disabled FPSController to the correct position for a screenshot
            ManageFPSControllers.RelocateAlignFPSControllerToFPSController(adjustedFPSControllerTransform);

            // turn everything off again
            ToggleSceneAndUI.ToggleSceneObjectsOff(disabledSceneName);

            // return the script hosts to their on state
            ToggleSceneAndUI.ToggleScriptHostObjectListOn();

            // set Pause as active again
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneGlobals.pauseMenuName));
        }
    }

    // rebuilds the sprite on this gameObject's image component
    public static void RefreshObjectImageSprite(GameObject imageObject)
    {
        if (imageObject)
        {
            Texture2D thumbnailTexture = AssociateCameraTextureByName(imageObject.name);
            Sprite updatedThumbnailSprite = Sprite.Create(thumbnailTexture, new Rect(0.0f, 0.0f, thumbnailTexture.width, thumbnailTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            imageObject.GetComponent<Image>().sprite = updatedThumbnailSprite;
        }
    }

    // forces a refresh of the time period selection thumbnails, in the Pause Menu
    // this rebuilds a sprite based on a texture, and sets it as the thumbnail image object's sprite
    public static void RefreshThumbnailSprites()
    {
        foreach (GameObject thumbnail in UIGlobals.timeTravelThumbnails)
        {
            RefreshObjectImageSprite(thumbnail);
        }
    }

    // time travel buttons take the player from a menu to a place and time in game
    public static void TimeTravelButtonAction(string buttonName)
    {
        // buttons need to be named with hyphens delimiting important info
        string[] nameSplitByDelimiter = buttonName.Split('-');
        // the place needs to be 1st
        string playerPosition = nameSplitByDelimiter[0];
        // the time period needs to be 2nd
        string timePeriod = nameSplitByDelimiter[1];

        // if the button name indicates a time traveler, don't specify an FPSController location (uses the current FPS location)
        if (buttonName.Contains("TimeTravel"))
        {
            // switch to the correct scene based on the time period and location in the button name
            ToggleSceneAndUI.ToggleFromSceneToSceneRelocatePlayerToFPSController(SceneManager.GetActiveScene().name, timePeriod, ManageFPSControllers.FPSControllerGlobals.activeFPSController.transform);
        }
        // otherwise, this request includes a specific location in its name, so relocate the player to that camera
        else
        {
            // switch to the correct scene based on the time period and location in the button name
            ToggleSceneAndUI.ToggleFromSceneToSceneRelocatePlayerToCamera(SceneManager.GetActiveScene().name, timePeriod, playerPosition);
        }
    }

    public static GameObject CreateMenuCanvas(GameObject parent, string name)
    {
        // create the menu object, and make sure its parent is the scene container object
        GameObject menu = new GameObject(name);
        menu.AddComponent<Canvas>();
        menu.transform.SetParent(parent.transform);

        // create the menu's canvas
        Canvas menuCanvas = menu.GetComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menu.AddComponent<CanvasScaler>();
        menu.AddComponent<GraphicRaycaster>();

        return menu;
    }

    public static GameObject CreateFullScreenImageFromCameraTexture(GameObject parent, bool refreshOnEnable)
    {
        // create the background object
        GameObject fullScreenBackground = new GameObject(parent.name + "BackgroundImage");
        fullScreenBackground.AddComponent<Image>();

        // create and configure the image
        Image fullScreenBackgroundImage = fullScreenBackground.GetComponent<Image>();
        fullScreenBackgroundImage.transform.SetParent(parent.transform);
        fullScreenBackgroundImage.preserveAspect = true;
        fullScreenBackgroundImage.SetNativeSize();

        // determine the texture we should use based on the object's name
        Texture2D backgroundTexture = GetPauseMenuBackgroundImage(SceneGlobals.referringSceneName);

        // set the sprite to the given texture
        fullScreenBackgroundImage.sprite = Sprite.Create(backgroundTexture, new Rect(0.0f, 0.0f, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

        // reset the scale before centering and full-screening
        fullScreenBackgroundImage.rectTransform.localScale = new Vector3(1, 1, 1);

        // center and full-screen the image
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(fullScreenBackgroundImage.gameObject);
        TransformScreenSpaceObject.ScaleImageToFillScreen(fullScreenBackgroundImage);

        // if enabled, add the script to refresh the image sprite when the object is re-enabled
        // this allows for using an image that's generated/updated during gameplay
        if (refreshOnEnable)
        {
            RefreshImageSprite refreshImageScript = fullScreenBackground.AddComponent<RefreshImageSprite>();
            refreshImageScript.refreshOnEnable = true;

            // set this image to the global image that needs updating
            UIGlobals.pauseMenuBackgroundImage = fullScreenBackground;
        }

        return fullScreenBackground;
    }

    public static GameObject CreateFullScreenImageBackground(GameObject parent, string imagePath)
    {
        // create the background object
        GameObject fullScreenBackgroundImage = new GameObject("BackgroundImage");
        fullScreenBackgroundImage.AddComponent<Image>();

        // set the image
        Image imageComponent = fullScreenBackgroundImage.GetComponent<Image>();

        imageComponent.sprite = (Sprite)Resources.Load(imagePath, typeof(Sprite));
        imageComponent.preserveAspect = true;
        imageComponent.SetNativeSize();

        // reset the scale before centering and full-screening
        imageComponent.rectTransform.localScale = new Vector3(1, 1, 1);

        // center and full-screen the sprite
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(imageComponent.gameObject);
        TransformScreenSpaceObject.ScaleImageToFillScreen(imageComponent);

        fullScreenBackgroundImage.transform.parent = parent.transform;

        return fullScreenBackgroundImage;
    }

    public static GameObject CreateFullScreenImageSlideshow(GameObject parent, string[] imageSequence)
    {
        // create the background object
        GameObject fullScreenBackgroundSlideShow = new GameObject("BackgroundSlideShow");
        fullScreenBackgroundSlideShow.AddComponent<Image>();

        // set the image
        Image fullScreenBackgroundSlideShowImage = fullScreenBackgroundSlideShow.GetComponent<Image>();
        fullScreenBackgroundSlideShowImage.transform.SetParent(parent.transform);

        // this script will sequence, transform, and animate the background images as required
        AnimateScreenSpaceObject AnimateScreenSpaceObjectScript = fullScreenBackgroundSlideShow.AddComponent<AnimateScreenSpaceObject>();
        AnimateScreenSpaceObjectScript.mainMenuBackgroundSlideShowSequence = imageSequence;

        return fullScreenBackgroundSlideShow;
    }

    public static GameObject CreateLogoHeader(GameObject parent)
    {
        // create the container object
        GameObject logoContainer = new GameObject("LogoHeader");
        logoContainer.AddComponent<CanvasRenderer>();
        Image logoContainerColor = logoContainer.AddComponent<Image>();
        logoContainerColor.color = UIGlobals.containerColor;

        // create the logo object
        GameObject logo = new GameObject("ProjectLogo");
        logo.AddComponent<Image>();

        // set the logo image
        Image logoImage = logo.GetComponent<Image>();
        logoImage.sprite = (Sprite)Resources.Load("UI/CCP logo", typeof(Sprite));
        logoImage.preserveAspect = true;
        logoImage.SetNativeSize();

        // adjust the scale of the logo
        TransformScreenSpaceObject.ScaleObjectByScreenHeightProportion(logo, projectLogoHeightScreenHeightRatio);

        // position the logo
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromScreenTop(logo, projectLogoTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromScreenLeft(logo, projectLogoLeftMarginScreenWidthRatio);

        // position the logo container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(logoContainer, logo, projectLogoContainerTopPaddingScreenHeightRatio);

        // resize the logo container
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(logoContainer, logo, projectLogoContainerBottomPaddingScreenHeightRatio);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborRight(logoContainer, logo, projectLogoContainerRightPaddingScreenWidthRatio);

        // set the parent/child hierarchy
        logoContainer.transform.SetParent(parent.transform);
        logo.transform.SetParent(logoContainer.transform);

        return logoContainer;
    }

    public static GameObject CreateMenuTitleBar(GameObject parent, GameObject topAlignmentObject, string titleString)
    {
        // create the title container object
        GameObject titleContainer = new GameObject("TitleContainer");
        titleContainer.AddComponent<CanvasRenderer>();
        Image logoContainerColor = titleContainer.AddComponent<Image>();
        logoContainerColor.color = UIGlobals.containerColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(titleContainer, topAlignmentObject, logoHeaderBottomMarginScreenHeightRatio);

        // resize the title container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchScreen(titleContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromScreenLeft(titleContainer, navContainerLeftMarginScreenWidthRatio);

        // add the title text
        GameObject titleLabel = new GameObject("TitleLabel");
        Text introMessageLabelText = titleLabel.AddComponent<Text>();
        introMessageLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        introMessageLabelText.text = titleString;
        introMessageLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.menuTitleLabelSize);
        introMessageLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position and resize the text and container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(titleLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(titleLabel, titleContainer, menuTitleTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(titleLabel, titleContainer, menuTitleLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(titleContainer, titleLabel, menuTitleBottomMarginScreenHeightRatio);

        // set parent/child hierarchy
        titleContainer.transform.SetParent(parent.transform);
        titleLabel.transform.SetParent(titleContainer.transform);

        return titleContainer;
    }

    public static GameObject CreateToggleGroupModule(GameObject parent, GameObject topAlignmentObject, GameObject leftAlignmentObject, bool useLeftSideOfAlignmentObject, float screenWidthRatio, string toggleGroupLabel)
    {
        // create the toggle group container object
        GameObject toggleGroupContainer = new GameObject("ToggleGroupContainer");
        toggleGroupContainer.AddComponent<CanvasRenderer>();
        Image toggleGroupContainerColor = toggleGroupContainer.AddComponent<Image>();
        toggleGroupContainerColor.color = UIGlobals.containerColor;

        // position the toggle group container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(toggleGroupContainer, topAlignmentObject, -centralNavPadding);

        // resize the toggle group container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchScreen(toggleGroupContainer);

        if (useLeftSideOfAlignmentObject)
        {
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(toggleGroupContainer, leftAlignmentObject, -centralNavPadding);
        }
        else
        {
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(toggleGroupContainer, leftAlignmentObject, centralNavPadding);
        }

        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(toggleGroupContainer, topAlignmentObject, -centralNavPadding);
        TransformScreenSpaceObject.ResizeObjectWidthByScreenWidthRatioTowardRight(toggleGroupContainer, screenWidthRatio);

        // add the toggle group label
        GameObject groupLabel = new GameObject("ToggleGroupLabel");
        Text introMessageLabelText = groupLabel.AddComponent<Text>();
        introMessageLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        introMessageLabelText.text = toggleGroupLabel;
        introMessageLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.toggleGroupLabelSize);
        introMessageLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position and resize the text and container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(groupLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(groupLabel, toggleGroupContainer, menuTitleTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(groupLabel, toggleGroupContainer, -UIGlobals.toggleContainerPadding);

        // set parent/child hierarchy
        groupLabel.transform.SetParent(toggleGroupContainer.transform);

        return toggleGroupContainer;
    }

    public static GameObject CreateToggleModule(GameObject parent, GameObject topAlignmentObject, string toggleLabel)
    {
        // create the toggle container object
        GameObject toggleColorContainer = new GameObject("ToggleContainer");
        toggleColorContainer.AddComponent<CanvasRenderer>();
        Image toggleContainerColor = toggleColorContainer.AddComponent<Image>();
        toggleContainerColor.color = UIGlobals.containerColor;

        // position the toggle container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(toggleColorContainer, topAlignmentObject, toggleBottomMarginScreenHeightRatio);

        // resize the toggle container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchScreen(toggleColorContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborLeft(toggleColorContainer, parent, -UIGlobals.toggleContainerPadding);
        TransformScreenSpaceObject.ResizeObjectWidthByScreenWidthRatioTowardRight(toggleColorContainer, UIGlobals.toggleContainerMaxWidthScreenWidthRatio);

        // contain the toggle elements in an object
        GameObject toggleObject = new GameObject("ToggleObject");

        // can't seem to access unity's default checkbox
        // so construct a toggle interface explicitly with a background and a checkmark
        GameObject toggleCheckboxBackground = new GameObject("ToggleCheckboxBackground");
        Image toggleObjectBackground = toggleCheckboxBackground.AddComponent<Image>();
        Vector2 toggleBackgroundSize = new Vector2(ConvertFontHeightRatioToPixelValue(UIGlobals.toggleLabelSize), ConvertFontHeightRatioToPixelValue(UIGlobals.toggleLabelSize));
        toggleObjectBackground.rectTransform.sizeDelta = toggleBackgroundSize;
        toggleObjectBackground.sprite = (Sprite)Resources.Load("UI/checkbox-background", typeof(Sprite));
        GameObject toggleCheckboxCheckmark = new GameObject("ToggleCheckboxCheckmark");
        Image toggleCheckmarkImage = toggleCheckboxCheckmark.AddComponent<Image>();
        toggleCheckmarkImage.sprite = (Sprite)Resources.Load("UI/checkbox-checkmark", typeof(Sprite));

        // add and configure the toggle
        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.graphic = toggleCheckmarkImage;

        // position the toggle background
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(toggleCheckboxBackground, toggleColorContainer, toggleTopPaddingScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(toggleCheckboxBackground, toggleColorContainer, toggleLeftPaddingScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectToMatchNeighborBothDirections(toggleCheckboxCheckmark, toggleCheckboxBackground);

        // center the checkmark
        TransformScreenSpaceObject.PositionObjectAtCenterpointOfNeighbor(toggleCheckboxCheckmark, toggleCheckboxBackground);

        // add the toggle label
        GameObject titleLabel = new GameObject("ToggleLabel");
        Text introMessageLabelText = titleLabel.AddComponent<Text>();
        introMessageLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        introMessageLabelText.text = toggleLabel;
        introMessageLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.toggleLabelSize);
        introMessageLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(introMessageLabelText);

        // position and resize the text and container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(titleLabel);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(titleLabel, toggleColorContainer, toggleTopPaddingScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(titleLabel, toggleCheckboxBackground, -toggleLeftPaddingScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(toggleColorContainer, titleLabel, toggleBottomPaddingScreenHeightRatio);

        // set parent/child hierarchy
        toggleObject.transform.SetParent(toggleColorContainer.transform);
        titleLabel.transform.SetParent(toggleObject.transform);
        toggleCheckboxBackground.transform.SetParent(toggleObject.transform);
        toggleCheckboxCheckmark.transform.SetParent(toggleCheckboxBackground.transform);
        toggleCheckboxBackground.transform.SetParent(toggle.transform);

        return toggleColorContainer;
    }

    /* specific versions of toggles */

    // object visibility toggle: display and modify object visibility states
    public static GameObject CreateVisibilityToggleModule(GameObject parent, GameObject topAlignmentObject, string toggleLabel, GameObject[] objectsToToggle)
    {
        // first, create a vanilla toggle
        GameObject visibilityToggleModule = CreateToggleModule(parent, topAlignmentObject, toggleLabel);

        // configure the toggle for visibility toggling
        ConfigureToggleForObjectVisibility(visibilityToggleModule, objectsToToggle);

        return visibilityToggleModule;
    }

    public static GameObject PopulateToggleGroup(GameObject toggleGroup, List<GameObject> togglesToDisplay)
    {
        float lastToggleBottomEdge = TransformScreenSpaceObject.GetObjectBottomEdgePositionY(togglesToDisplay[togglesToDisplay.Count - 1]);
        float toggleGroupBottomEdge = TransformScreenSpaceObject.GetObjectBottomEdgePositionY(toggleGroup);

        // if there are enough toggles to cause the last toggle to require scrolling,
        // resize the toggle group container to fit the toggles
        if (lastToggleBottomEdge < toggleGroupBottomEdge)
        {
            // resize the group bottom edge to encompass the toggles
            TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(toggleGroup, togglesToDisplay[togglesToDisplay.Count - 1], toggleBottomMarginScreenHeightRatio);
        }

        // resize the group right edge to hug the typical toggle width
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborRight(toggleGroup, togglesToDisplay[0], UIGlobals.toggleContainerPadding);

        // position the label again to account for the resizing of the group
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(toggleGroup.transform.GetChild(0).gameObject, toggleGroup, menuTitleTopMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(toggleGroup.transform.GetChild(0).gameObject, toggleGroup, -UIGlobals.toggleContainerPadding);

        // add each of the specified toggles to the toggle group
        foreach (GameObject toggle in togglesToDisplay)
        {
            // set the toggle as a child of the toggle group
            toggle.transform.SetParent(toggleGroup.transform);
        }

        return toggleGroup;
    }

    // configure a typical checkbox by passing in
    // functions for the checkbox action and state update
    public static void ConfigureTypicalToggle(GameObject toggleContainer, Action onValueChangedFunction, bool toggleState)
    {
        Toggle toggle = toggleContainer.GetComponentInChildren<Toggle>();

        toggle.isOn = toggleState;

        if (toggle == null)
        {
            return;
        }

        toggle.onValueChanged.AddListener(delegate {

            onValueChangedFunction.Invoke();

        });
    }

    // configure toggles to read and set object visibility states
    public static void ConfigureToggleForObjectVisibility(GameObject toggleContainer, GameObject[] objectsToToggle)
    {
        // get the actual toggle
        Toggle toggle = toggleContainer.GetComponentInChildren<Toggle>();

        // only configure if there are objects to toggle
        if (objectsToToggle.Length > 0)
        {
            // set the toggle to match the visibility of the requested objects
            // note: this only tests the first object specified - assumes that the other objects match
            UpdateToggleStateToMatchObjectVisibility(toggleContainer, objectsToToggle[0]);

            // set the toggle to invoke changing the visibility of the object
            toggle.onValueChanged.AddListener(delegate {
                foreach (GameObject objectToToggle in objectsToToggle)
                {
                    ToggleObjects.ToggleGameObjectChildrenVisibility(objectToToggle);
                }
            });
        }
    }

    public static void UpdateToggleStateToMatchObjectVisibility(GameObject toggleContainer, GameObject objectToMatch)
    {
        // get the toggle from the toggle container first
        Toggle toggle = toggleContainer.GetComponentInChildren<Toggle>();

        if (toggle != null)
        {
            // set the toggle state to match the visibility state
            toggle.isOn = ObjectVisibility.GetIsAnyChildObjectVisible(objectToMatch);
        }
    }

    // create a vanilla scrollable area
    public static GameObject CreateScrollableArea(string name, string scrollDirection)
    {
        GameObject scrollableArea = new GameObject(name + "ScrollableArea");

        scrollableArea.AddComponent<RectMask2D>();
        ScrollRect scrollRect = scrollableArea.AddComponent<ScrollRect>();
        scrollRect.scrollSensitivity = 10;

        switch (scrollDirection)
        {
            case string scrollDir when scrollDirection.Contains("horizontal"):
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                // horizontally-enabled scroll areas
                // get a special script to scroll based on cursor location
                ScrollByCursorPosition scrollScript = scrollableArea.AddComponent<ScrollByCursorPosition>();
                scrollScript.scrollDirection = "x";
                return scrollableArea;
            case string scrollDir when scrollDirection.Contains("vertical"):
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
                return scrollableArea;
            case string scrollDir when scrollDirection.Contains("both"):
                scrollRect.horizontal = true;
                scrollRect.vertical = true;
                // horizontally-enabled scroll areas
                // get a special script to scroll based on cursor location
                ScrollByCursorPosition mixedScrollScript = scrollableArea.AddComponent<ScrollByCursorPosition>();
                mixedScrollScript.scrollDirection = "x";
                return scrollableArea;
            default:
                return scrollableArea;
        }
    }

    public static void ConfigureScrollAreaToMatchChildRect(GameObject scrollAreaObject, GameObject child)
    {
        RectTransform sourceObjectRectTransform = child.GetComponent<RectTransform>();
        RectTransform targetRectTransform = scrollAreaObject.GetComponent<RectTransform>();

        TransformScreenSpaceObject.MatchRectTransform(child, scrollAreaObject);

        ScrollRect scrollAreaRect = scrollAreaObject.GetComponent<ScrollRect>();
        scrollAreaRect.content = sourceObjectRectTransform;
    }

    public static GameObject CreateHUDTimePeriodIndicator(GameObject parent, string titleString)
    {
        // create the title container object
        GameObject timePeriodContainer = new GameObject("TimePeriodContainer");
        timePeriodContainer.AddComponent<CanvasRenderer>();
        Image timePeriodContainerColor = timePeriodContainer.AddComponent<Image>();
        timePeriodContainerColor.color = UIGlobals.containerColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromScreenTop(timePeriodContainer, HUDBottomBarTopMarginScreenHeightRatio);

        // resize the title container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchScreen(timePeriodContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromScreenLeft(timePeriodContainer, HUDTimePeriodLabelLeftMarginScreenWidthRatio);

        // add the title text
        GameObject timePeriodLabel = new GameObject("TimePeriodLabel");
        Text timePeriodLabelText = timePeriodLabel.AddComponent<Text>();
        timePeriodLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        timePeriodLabelText.text = titleString;
        timePeriodLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.HUDTimePeriodLabelSize);
        timePeriodLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text, before any transforms
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(timePeriodLabelText);

        // position and resize the text and container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(timePeriodLabel);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromScreenBottom(timePeriodContainer, HUDBottomBarBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(timePeriodLabel, timePeriodContainer, menuTitleLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromScreenBottom(timePeriodContainer, HUDBottomBarBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionTextAtHorizontalCenterlineOfNeighbor(timePeriodLabel, timePeriodContainer);

        // set parent/child hierarchy
        timePeriodContainer.transform.SetParent(parent.transform);
        timePeriodLabel.transform.SetParent(timePeriodContainer.transform);

        return timePeriodContainer;
    }

    // displays the time period in the center of the screen
    // only used when time traveling, to remind the user what time period they've switched to
    public static GameObject CreateHUDTimePeriodNotification(GameObject parent, string notificationText)
    {
        // create the label container
        GameObject timePeriodNotificationContainer = new GameObject("TimePeriodNotificationContainer" + parent.scene.name);
        timePeriodNotificationContainer.AddComponent<CanvasRenderer>();

        SetTimePeriodNotificationContainerByName(timePeriodNotificationContainer, parent.scene.name);

        // image is needed to create a rect transform
        Image timePeriodContainerColor = timePeriodNotificationContainer.AddComponent<Image>();
        timePeriodContainerColor.color = UIGlobals.containerColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(timePeriodNotificationContainer);

        // add the title text
        GameObject underConstructionLabel = new GameObject("TimePeriodNotificationLabel");
        Text underConstructionLabelText = underConstructionLabel.AddComponent<Text>();
        underConstructionLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        underConstructionLabelText.text = notificationText;
        underConstructionLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.menuTitleLabelSize);
        underConstructionLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text
        Vector2 textSize = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(underConstructionLabelText);

        RectTransform rt = timePeriodContainerColor.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(textSize.x, textSize.y);

        Vector2 newSize = TransformScreenSpaceObject.ResizeObjectFromCenterByMargin(timePeriodNotificationContainer, 0.01f, 0.01f);

        // position the title text
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(underConstructionLabel);

        // set parent/child hierarchy
        timePeriodNotificationContainer.transform.SetParent(parent.transform);
        underConstructionLabel.transform.SetParent(timePeriodNotificationContainer.transform);

        // disable initially
        timePeriodNotificationContainer.SetActive(false);

        return timePeriodNotificationContainer;
    }

    public static GameObject CreateHUDUnderConstructionLabel(GameObject parent, string message)
    {
        // create the label container
        UIGlobals.underConstructionLabelContainer = new GameObject("UnderConstructionContainer");
        UIGlobals.underConstructionLabelContainer.AddComponent<CanvasRenderer>();
        // image is needed to create a rect transform
        Image timePeriodContainerColor = UIGlobals.underConstructionLabelContainer.AddComponent<Image>();
        timePeriodContainerColor.color = UIGlobals.clearColor;

        // position the title container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(UIGlobals.underConstructionLabelContainer);

        // add the title text
        GameObject underConstructionLabel = new GameObject("UnderConstructionLabel");
        Text underConstructionLabelText = underConstructionLabel.AddComponent<Text>();
        underConstructionLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        underConstructionLabelText.text = message;
        underConstructionLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.menuTitleLabelSize);
        underConstructionLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(underConstructionLabelText);

        // position the title text
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(underConstructionLabel);

        // set parent/child hierarchy
        UIGlobals.underConstructionLabelContainer.transform.SetParent(parent.transform);
        underConstructionLabel.transform.SetParent(UIGlobals.underConstructionLabelContainer.transform);

        return UIGlobals.underConstructionLabelContainer;
    }

    public static GameObject CreateVersionLabel(GameObject parent)
    {
        string version = "v" + Application.version;

        // create the version container
        GameObject versionLabelContainer = new GameObject("VersionLabelContainer");
        versionLabelContainer.AddComponent<CanvasRenderer>();
        // image is needed to create a rect transform
        Image versionLabelContainerColor = versionLabelContainer.AddComponent<Image>();
        versionLabelContainerColor.color = UIGlobals.clearColor;

        // position the version container
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(versionLabelContainer);

        // add the version text
        GameObject versionLabel = new GameObject("VersionLabel");
        Text versionLabelText = versionLabel.AddComponent<Text>();
        versionLabelText.color = UIGlobals.subtleTextColor;
        versionLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        versionLabelText.text = version;
        versionLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.versionLabelSize);
        versionLabelText.alignment = TextAnchor.UpperLeft;

        // resize the text's bounding box to fit the text
        TransformScreenSpaceObject.ResizeTextExtentsToFitContents(versionLabelText);

        // position the version text
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(versionLabel);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromScreenLeft(versionLabel, versionLabelLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromScreenTop(versionLabel, versionLabelTopMarginScreenHeightRatio);

        // set parent/child hierarchy
        versionLabelContainer.transform.SetParent(parent.transform);
        versionLabel.transform.SetParent(versionLabelContainer.transform);

        return versionLabelContainer;
    }

    public static GameObject CreateCentralNavContainer(GameObject parent, GameObject topAlignmentObject)
    {
        // create the central nav container
        GameObject centralNavContainer = new GameObject("CentralNavContainer");
        centralNavContainer.AddComponent<CanvasRenderer>();

        // set the color of the central nav container
        Image centralNavContainerColor = centralNavContainer.AddComponent<Image>();
        centralNavContainerColor.color = UIGlobals.containerColor;

        // position the central nav container
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(centralNavContainer, topAlignmentObject, navContainerTopMarginScreenHeightRatio);

        // resize the central nav container
        TransformScreenSpaceObject.ResizeObjectWidthToMatchScreen(centralNavContainer);
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromScreenLeft(centralNavContainer, navContainerLeftMarginScreenWidthRatio);
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromScreenBottom(centralNavContainer, navContainerBottomMarginScreenHeightRatio);

        return centralNavContainer;
    }

    public static GameObject CreateTextButton(string text, GameObject parent, int fontSize, Color32 color)
    {
        // create the text label
        GameObject buttonTextObject = new GameObject(Utils.StringUtils.CleanString(text) + "ButtonText");
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.font = (Font)Resources.Load(UIGlobals.labelFont);
        buttonText.text = text;
        buttonText.fontSize = fontSize;
        buttonText.alignment = TextAnchor.MiddleCenter;

        Vector2 textSize = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(buttonText);

        // create the button
        GameObject buttonContainer = new GameObject(Utils.StringUtils.CleanString(text) + "Button");
        buttonContainer.AddComponent<CanvasRenderer>();
        buttonContainer.AddComponent<RectTransform>();

        // resize the button background to encapsulate the text, plus padding
        RectTransform buttonRect = buttonContainer.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(Mathf.Round((UIGlobals.menuButtonScreenWidthRatio * Screen.width)), Mathf.Round(textSize.y + (2 * (UIGlobals.menuButtonTopBottomPaddingScreenHeightRatio * Screen.height))));

        // set the color of the button
        Image buttonContainerColor = buttonContainer.AddComponent<Image>();
        buttonContainerColor.color = color;

        // create the Unity button
        Button button = buttonContainer.AddComponent<Button>();

        // set the parent/child hierarchy
        buttonContainer.transform.SetParent(parent.transform);
        buttonTextObject.transform.SetParent(buttonContainer.transform);

        // move the button
        TransformScreenSpaceObject.PositionObjectAtCenterofScreen(buttonContainer);

        return buttonContainer;
    }

    // create the time label stack
    public static GameObject CreateTimeLabelStack(GameObject parent, GameObject leftAlignmentObject, string[] availableTimePeriodFriendlyNames)
    {
        // create a container object
        GameObject timeLabelStackContainer = new GameObject("TimePeriodLabelStack");

        foreach (string timePeriod in availableTimePeriodFriendlyNames)
        {
            // create the time periods labels
            GameObject timePeriodLabel = new GameObject(timePeriod + "Label");

            Text timePeriodLabelText = timePeriodLabel.AddComponent<Text>();
            timePeriodLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
            timePeriodLabelText.text = timePeriod;
            timePeriodLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.timeLabelSize);
            timePeriodLabelText.alignment = TextAnchor.MiddleCenter;

            // get the text's dimensions to match only the space it needs, before any transforms
            TransformScreenSpaceObject.ResizeTextExtentsToFitContents(timePeriodLabelText);

            // position the time period label
            // note this only positions it horizontally
            // vertical positioning happens later, after the thumbnails are built
            TransformScreenSpaceObject.PositionObjectAtCenterofScreen(timePeriodLabel);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborLeft(timePeriodLabel, leftAlignmentObject, timeLabelLeftMarginScreenWidthRatio);

            // add this label to the list of labels for alignment
            timeLabelsForAlignment.Add(timePeriodLabel);

            // set the parent/child hierarchy
            timePeriodLabel.transform.SetParent(timeLabelStackContainer.transform);
        }

        return timeLabelStackContainer;
    }

    // create the stacked thumbnails for a place, across time periods
    public static GameObject CreatePlaceTimeThumbnailStack(GameObject parent, GameObject topAlignmentObject, GameObject leftAlignmentObject, string placeName, string[] timePeriodNames)
    {
        // clear the list of thumbnail objects from the previous stack created
        // this list will be used to set parents and as alignment guides for other objects
        placeThumbnailsForAlignment.Clear();

        // we'll need to generate the thumbnails differently if this stack is intended for "time traveling"
        // NOTE: this requires a match with the user-facing thumbnail stack title UI

        // set the flag if generating a time travel thumbnail
        if (placeName.Contains("Time Travel"))
        {
            UIGlobals.isTimeTravelThumbnail = true;
        }
        else
        {
            UIGlobals.isTimeTravelThumbnail = false;
        }

        // make a vertical scroll area for the thumbnails
        // make an object to hold the thumbnails
        GameObject thumbnailStackContainer = new GameObject(Utils.StringUtils.CleanString(placeName) + "ThumbnailStack");

        // location text
        GameObject placeLabel = new GameObject(Utils.StringUtils.CleanString(placeName) + "Label");
        Text placeLabelText = placeLabel.AddComponent<Text>();
        placeLabelText.font = (Font)Resources.Load(UIGlobals.labelFont);
        placeLabelText.text = placeName;
        placeLabelText.fontSize = ConvertFontHeightRatioToPixelValue(UIGlobals.placeLabelSize);
        placeLabelText.alignment = TextAnchor.MiddleCenter;

        // adjust the text's rectTransform to ensure objects aligning to it fit closely
        RectTransform placeLabelRectTransform = placeLabel.GetComponent<RectTransform>();
        Vector2 sizeVector = TransformScreenSpaceObject.ResizeTextExtentsToFitContents(placeLabelText);
        placeLabelRectTransform.sizeDelta = sizeVector;
        placeLabelText.alignByGeometry = true;

        // position the location text
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(placeLabel, topAlignmentObject, placeLabelTopMarginScreenHeightRatio);

        // for each image name provided, make a new thumbnail button
        for (var i = 0; i < timePeriodNames.Length; i++)
        {
            // combine the place name and time period strings
            string combinedPlaceTimeNameSpacelessDashed = Utils.StringUtils.CleanString(placeName) + "-" + SceneGlobals.availableTimePeriodSceneNamesList[i];

            // create the button
            GameObject timePeriodButton = new GameObject(combinedPlaceTimeNameSpacelessDashed + "-Button");
            timePeriodButton.AddComponent<Image>();

            // set the image
            // note this requires a valid image in the Resources folder path below, with a file name that matches combinedPlaceTimeNameSpaceless
            Image timePeriodButtonImage = timePeriodButton.GetComponent<Image>();

            // need to set the sprite differently depending on whether this is a time travel thumbnail, or standard time/place thumbnail

            // time travel sprites need to get a public texture generated by the FPSCameras
            if (UIGlobals.isTimeTravelThumbnail)
            {
                Texture2D timeTravelCameraTexture = AssociateCameraTextureByName(timePeriodButton.name);

                timePeriodButtonImage.sprite = Sprite.Create(timeTravelCameraTexture, new Rect(0.0f, 0.0f, timeTravelCameraTexture.width, timeTravelCameraTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

                // also attach the script to force update the sprite when the Pause Menu is called
                timePeriodButton.AddComponent<RefreshImageSprite>();

                // add this to the global list of time travel thumbnails, so we can update it later
                UIGlobals.timeTravelThumbnails.Add(timePeriodButton);
            }
            // otherwise, standard time periods look for their sprite in the file system
            else
            {
                timePeriodButtonImage.sprite = (Sprite)Resources.Load("UI/Camera-Thumbnail-" + combinedPlaceTimeNameSpacelessDashed, typeof(Sprite));
            }

            timePeriodButtonImage.preserveAspect = true;
            timePeriodButtonImage.SetNativeSize();

            // scale the button to be a proportion of the camera height
            TransformScreenSpaceObject.ScaleObjectByScreenHeightProportion(timePeriodButton, 0.2f);

            // position the button
            TransformScreenSpaceObject.PositionObjectAtCenterofScreen(timePeriodButton);
            TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(timePeriodButton, leftAlignmentObject, 0.02f);

            // if this is the first item, position it below the place label
            if (i == 0)
            {
                TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePeriodButton, placeLabel, placeLabelBottomMarginScreenHeightRatio);
            }
            // otherwise, position it below the previous thumbnail
            else
            {
                TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(timePeriodButton, placeThumbnailsForAlignment[i - 1], timePlaceThumbnailBottomMarginScreenHeightRatio);
            }

            // add this new thumbnail button to the list for tracking
            placeThumbnailsForAlignment.Add(timePeriodButton);

            // add the button property
            timePeriodButton.AddComponent<Button>();
            timePeriodButton.GetComponent<Button>().onClick.AddListener(() => { TimeTravelButtonAction(timePeriodButton.name); }); ;

            // set the parent/child hierarchy
            timePeriodButton.transform.SetParent(thumbnailStackContainer.transform);
        }

        // position the place label centered at the first thumbnail
        TransformScreenSpaceObject.PositionObjectAtVerticalCenterlineOfNeighbor(placeLabel, placeThumbnailsForAlignment[0]);

        // set the parent/child hierarchy
        placeLabel.transform.SetParent(thumbnailStackContainer.transform);

        // due to a problem with parent transforms, 
        // we can't set the parent/child hierarchy of the entire stack yet
        // instead, add this stack to the orphaned object list, and we'll assign its parent later
        orphanedThumbnailStacks.Add(thumbnailStackContainer);

        return thumbnailStackContainer;
    }

    // create the main menu central navigation
    public static GameObject CreateMainMenuCentralNav(GameObject parent, GameObject topNeighbor)
    {
        // clear lists
        orphanedThumbnailStacks.Clear();
        timeLabelsForAlignment.Clear();

        // put all the thumbnail stacks in a horizontal scroll area
        // and central nav container
        GameObject mainMenuHorizontalScrollArea = CreateScreenSpaceUIElements.CreateScrollableArea("MainMenuHorizontalScrollArea", "both");
        GameObject mainMenuCentralNavContainer = CreateScreenSpaceUIElements.CreateCentralNavContainer(parent, topNeighbor);
        CreateScreenSpaceUIElements.ConfigureScrollAreaToMatchChildRect(mainMenuHorizontalScrollArea, mainMenuCentralNavContainer);

        // create the time label stack
        GameObject timeLabelStack = CreateTimeLabelStack(mainMenuCentralNavContainer, mainMenuCentralNavContainer, SceneGlobals.availableTimePeriodFriendlyNames);

        // use the first time label for aligning other objects horizontally
        GameObject timeLabelForAlignment = timeLabelStack.transform.GetChild(0).gameObject;

        // create each place thumbnail stack, and their associated place labels
        GameObject blueMallThumbnailStack = CreatePlaceTimeThumbnailStack(mainMenuCentralNavContainer, mainMenuCentralNavContainer, timeLabelForAlignment, "Blue Mall", SceneGlobals.availableTimePeriodFriendlyNames);

        GameObject roseMallThumbnailStack = CreatePlaceTimeThumbnailStack(mainMenuCentralNavContainer, mainMenuCentralNavContainer, blueMallThumbnailStack.transform.GetChild(0).gameObject, "Rose Mall", SceneGlobals.availableTimePeriodFriendlyNames);

        GameObject goldMallThumbnailStack = CreatePlaceTimeThumbnailStack(mainMenuCentralNavContainer, mainMenuCentralNavContainer, roseMallThumbnailStack.transform.GetChild(0).gameObject, "Gold Mall", SceneGlobals.availableTimePeriodFriendlyNames);

        GameObject cinderAlleyThumbnailStack = CreatePlaceTimeThumbnailStack(mainMenuCentralNavContainer, mainMenuCentralNavContainer, goldMallThumbnailStack.transform.GetChild(0).gameObject, "Cinder Alley", SceneGlobals.availableTimePeriodFriendlyNames);

        // resize the container to align with the last thumbnail in the column
        int thumbnailCount = blueMallThumbnailStack.transform.childCount - 1; // exclude the label
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(mainMenuCentralNavContainer, blueMallThumbnailStack.transform.GetChild(thumbnailCount - 1).gameObject, thumbnailStackBottomMarginScreenHeightRatio);

        // resize the content within the scroll area to just past the last sub-element
        TransformScreenSpaceObject.ResizeObjectWidthByBufferRatioFromNeighborRight(mainMenuCentralNavContainer, cinderAlleyThumbnailStack.transform.GetChild(1).gameObject, UIGlobals.toggleContainerPadding);

        // position the time labels to align horizontally with the place thumbnails
        TransformScreenSpaceObject.PositionMultiObjectsAtHorizontalCenterlinesOfNeighbors(timeLabelsForAlignment, placeThumbnailsForAlignment);

        // set the parent/child hierarchy
        mainMenuHorizontalScrollArea.transform.SetParent(parent.transform);
        mainMenuCentralNavContainer.transform.SetParent(mainMenuHorizontalScrollArea.transform);
        timeLabelStack.transform.SetParent(mainMenuCentralNavContainer.transform);
        AssignOrphanedObjectListToParent(orphanedThumbnailStacks, mainMenuCentralNavContainer);

        return mainMenuCentralNavContainer;
    }

    public static GameObject CreatePauseMenuCentralNav(GameObject parent, GameObject topNeighbor)
    {
        // clear lists
        orphanedThumbnailStacks.Clear();
        timeLabelsForAlignment.Clear();

        // create the central nav container
        GameObject pauseMenuCentralNavContainer = CreateCentralNavContainer(parent, topNeighbor);

        // create the time label stack
        GameObject timeLabelStack = CreateTimeLabelStack(pauseMenuCentralNavContainer, pauseMenuCentralNavContainer, SceneGlobals.availableTimePeriodFriendlyNames);

        // use the first time label for aligning other objects horizontally
        GameObject timeLabelForAlignment = timeLabelStack.transform.GetChild(0).gameObject;

        // create the time travel thumbnail container
        GameObject timeTravelThumbnailStack = CreatePlaceTimeThumbnailStack(pauseMenuCentralNavContainer, pauseMenuCentralNavContainer, timeLabelForAlignment, "Time Travel:", SceneGlobals.availableTimePeriodFriendlyNames);

        // resize the container to align with the last thumbnail in the column
        int thumbnailCount = timeTravelThumbnailStack.transform.childCount - 1; // exclude the label
        TransformScreenSpaceObject.ResizeObjectHeightByBufferRatioFromNeighborBottom(pauseMenuCentralNavContainer, timeTravelThumbnailStack.transform.GetChild(thumbnailCount - 1).gameObject, thumbnailStackBottomMarginScreenHeightRatio);

        // position the time labels to align horizontally with the place thumbnails
        TransformScreenSpaceObject.PositionMultiObjectsAtHorizontalCenterlinesOfNeighbors(timeLabelsForAlignment, placeThumbnailsForAlignment);

        /// buttons ///

        // define a gameObject to align the buttons to
        GameObject buttonAlignmentObject = timeTravelThumbnailStack.transform.GetChild(0).gameObject;

        // create the resume button
        GameObject resumeButton = CreateTextButton("Resume", pauseMenuCentralNavContainer, ConvertFontHeightRatioToPixelValue(UIGlobals.mainMenuTextButtonLabelSize), UIGlobals.buttonColor);
        resumeButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            ManageFPSControllers.FPSControllerGlobals.isTimeTraveling = false;
            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, SceneGlobals.referringSceneName);

        }); ;
        // align and position the main menu button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborTop(resumeButton, buttonAlignmentObject, 0.0f);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(resumeButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // create the main menu button
        GameObject mainMenuButton = CreateTextButton("Main Menu", pauseMenuCentralNavContainer, ConvertFontHeightRatioToPixelValue(UIGlobals.mainMenuTextButtonLabelSize), UIGlobals.buttonColor);
        mainMenuButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            ToggleSceneAndUI.ToggleFromSceneToScene(SceneManager.GetActiveScene().name, "MainMenu");

        }); ;
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(mainMenuButton, resumeButton, textButtonBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(mainMenuButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // exit button
        GameObject exitButton = CreateTextButton("Quit", pauseMenuCentralNavContainer, ConvertFontHeightRatioToPixelValue(UIGlobals.mainMenuTextButtonLabelSize), UIGlobals.buttonColor);
        exitButton.GetComponentInChildren<Button>().onClick.AddListener(() => {

            Application.Quit();

        }); ;
        // align and position the exit button
        TransformScreenSpaceObject.PositionObjectByHeightRatioFromNeighborBottom(exitButton, mainMenuButton, textButtonBottomMarginScreenHeightRatio);
        TransformScreenSpaceObject.PositionObjectByWidthRatioFromNeighborRight(exitButton, buttonAlignmentObject, textButtonLeftMarginScreenWidthRatio);

        // set the parent/child hierarchy
        pauseMenuCentralNavContainer.transform.SetParent(parent.transform);
        timeLabelStack.transform.SetParent(pauseMenuCentralNavContainer.transform);
        AssignOrphanedObjectListToParent(orphanedThumbnailStacks, pauseMenuCentralNavContainer);

        return pauseMenuCentralNavContainer;
    }

    // button actions that are also invoked by shortcuts or other input events

    public static void CaptureScreenshotButtonAction()
    {
        // dismiss any active overlay menus
        ManageOverlayVisibility.DismissActiveOverlayMenu();

        TakeScreenshots.CaptureScreenshotOfCurrentCamera(ManageCameraActions.GetScreenshotPathByContext());
    }
    
    public static void SaveViewButtonAction()
    {
        string restoreData = ManageFPSControllers.GetSerializedFPSControllerRestoreData(ManageFPSControllers.FPSControllerGlobals.activeFPSController);

        ManageFPSControllers.FPSControllerRestoreData.WriteFPSControllerRestoreDataToDir(ManageFPSControllers.GetSerializedFPSControllerRestoreData(ManageFPSControllers.FPSControllerGlobals.activeFPSController));

        // dismiss any active overlay menus
        ManageOverlayVisibility.DismissActiveOverlayMenu();
    }

    public static void RestoreViewButtonAction()
    {
        // get the restore data from the clipboard
        ManageFPSControllers.FPSControllerRestoreData restoreData = ManageFPSControllers.FPSControllerRestoreData.ReadFPSControllerRestoreDataFromClipboard();

        if (restoreData != null)
        {
            ManageFPSControllers.RelocateAlignFPSControllerToMatchRestoreData(restoreData);

            // dismiss any active overlay menus
            ManageOverlayVisibility.DismissActiveOverlayMenu();
        }
    }
}




