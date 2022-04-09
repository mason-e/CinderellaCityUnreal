﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides access to object visibility keywords, top-level object finding, and object visibility checks
/// </summary>

public static class ObjectVisibilityGlobals
{
    // identify key words in order to toggle visibility of several objects at once
    public static string[] anchorStoreObjectKeywords = { "anchor-" };
    public static string[] ceilingObjectKeywords = { "mall-ceilings", "mall-speakers", "store-ceilings" };
    public static string[] exteriorWallObjectKeywords = { "mall-walls-detailing-exterior" };
    public static string[] floorObjectKeywords = { "mall-floors-vert", "store-floors" };
    public static string[] furnitureObjectKeywords = { "mall-furniture" };
    public static string[] historicPhotographObjectKeywords = { "proxy-cameras-photos" };
    public static string[] lightsObjectKeyword = { "mall-lights" };
    public static string[] interiorDetailingObjectKeywords = { "mall-detailing-interior","mall-flags", "store-detailing" };
    public static string[] interiorWallObjectKeywords = { "mall-walls-interior", "store-walls" };
    public static string[] peopleObjectKeywords = { "proxy-people" };
    public static string[] roofObjectKeywords = { "mall-roof" };
    public static string[] signageObjectKeywords = { "mall-signage" };
    public static string[] vegetationObjectKeywords = { "proxy-trees-veg" };
    public static string[] waterFeatureObjectKeywords = { "proxy-water" };
}

public class ObjectVisibility
{
    public static GameObject[] GetTopLevelGameObjectByKeyword(string[] visibilityKeywords)
    {
        // start with an empty list and add to it
        List<GameObject> foundObjectsList = new List<GameObject>();

        foreach (string keyword in visibilityKeywords)
        {
            GameObject[] foundObjects = ManageScenes.GetTopLevelSceneContainerGameObjectsByName(keyword);
            if (foundObjects.Length > 0)
            {
                for (var i = 0; i < foundObjects.Length; i++)
                {
                    foundObjectsList.Add(foundObjects[i]);
                }
            }
        }

        GameObject[] foundObjectsArray = foundObjectsList.ToArray();
        return foundObjectsArray;
    }

    public static bool GetIsObjectVisible(string objectName)
    {
        bool isVisible = false;

        GameObject objectToTest = GameObject.Find(objectName);

        if (objectToTest != null)
        {
            isVisible = objectToTest.activeSelf;
        }

        return isVisible;
    }

    public static bool GetIsAnyChildObjectVisible(GameObject parentObject)
    {
        foreach (Transform child in parentObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }
}