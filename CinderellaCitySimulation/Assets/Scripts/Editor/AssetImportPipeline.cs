﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.AI;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


[InitializeOnLoad]

public class AssetImportUpdate : AssetPostprocessor {

    // this script only runs when an asset is updated
    // when this happens, find out what file was updated
    // and globally store its name, path, etc. for all functions to access
    static String globalAssetFilePath;
    static String globalAssetFileNameAndExtension;
    static String globalAssetFileName;
    static String globalAssetFileDirectory;
    static String globalAssetTexturesDirectory;

    // if the model just updated isn't already in the scene, we need to keep track of it
    // in order to maintain parent/child hierarchy in the post-processor
    static GameObject newlyInstantiatedPrefab;
    static bool newlyInstantiated;

    // get the current scene
    static Scene currentScene = EditorSceneManager.GetActiveScene();

    // all incoming models are scaled to this value
    static float globalScale = 1.0f;

    // in some cases, we need to stop processing from happening if it's already been done
    static float prevTime;

    // post-processing should only happen after pre-processing
    static bool postProcessingRequired = false;

    // proxy replacement post-processing is needed only until replacements are valid and in the model
    static bool proxyReplacementProcessingRequired = true;

    // instantiate proxy strings
    static string proxyType;
    static string replacementObjectPath;
    static string animatorControllerPath;

    // post-processing seems to repeat itself a lot, so set a max and keep track of how many times
    // note that if an object was just instantiated in the scene, this max hit value gets incremented by 1
    static int globalMaxPostProcessingHits = 2;
    static List<bool> postProcessingHits = new List<bool>();

    //
    // master list of option flags that any file just changed can receive
    // all option flags should default to false, except global scale
    //

    // master pre-processor option flags
    // ... for models
    static bool doSetGlobalScale = false;
    static bool doInstantiateAndPlaceInCurrentScene = false;
    static bool doSetColliderActive = false;
    static bool doSetUVActiveAndConfigure = false;
    static bool doDeleteReimportMaterialsTextures = false;
    static bool doAddBehaviorComponents = false;
    // ... for audio clips
    static bool doSetClipImportSettings = false;
    // ... for textures and images
    static bool doSetTextureToSpriteImportSettings = false;

    // master post-processor option flags
    // ... for models
    static bool doSetStatic = false;
    static bool doSetMaterialEmission = false;
    static bool doSetMaterialSmoothnessMetallic = false;
    static bool doInstantiateProxyReplacements = false;
    static bool doHideProxyObjects = false;
    static bool doRebuildNavMesh = false;

    //
    // end master list
    //

    // set up callbacks
    public AssetImportUpdate()
    {
        EditorSceneManager.activeSceneChangedInEditMode += SceneChangedInEditModeCallback;
    }
    // for some reason, we need to subscribe to this message and update the currentScene name when a different scene is opened in the Editor
    private void SceneChangedInEditModeCallback(Scene previousScene, Scene newScene)
    {
        currentScene = newScene;
        Utils.DebugUtils.DebugLog("Opened a different Scene in the Editor: " + newScene.name);
    }

    // define how to clear the console
    public static void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    // define how to set the scale of the imported model
    // should be called on all imported objects for consistency
    void SetGlobalScale(ModelImporter mi)
    {
        mi.globalScale = globalScale;
    }

    // define how to set colliders for the imported model
    void SetColliderActive(ModelImporter mi)
    {
        mi.addCollider = true;
    }

    // define how to enable UVs and configure them
    void SetUVActiveAndConfigure(ModelImporter mi)
    {
        mi.generateSecondaryUV = true;
        mi.secondaryUVHardAngle = 88;
        mi.secondaryUVPackMargin = 64;
        mi.secondaryUVAngleDistortion = 4;
        mi.secondaryUVAreaDistortion = 4;
        mi.importNormals = ModelImporterNormals.Calculate;
    }

    // define how to set audio clip import settings
    void SetClipImportSettings(AudioImporter ai)
    {
        ai.forceToMono = true;
        ai.loadInBackground = true;
        ai.preloadAudioData = true;

        //create a temp variable that contains everything we could apply to the imported AudioClip (possible changes: .compressionFormat, .conversionMode, .loadType, .quality, .sampleRateOverride, .sampleRateSetting)
        AudioImporterSampleSettings aiSampleSettings = ai.defaultSampleSettings;
        aiSampleSettings.loadType = AudioClipLoadType.CompressedInMemory;
        aiSampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
        aiSampleSettings.quality = 70f;

        // apply the settings
        ai.defaultSampleSettings = aiSampleSettings;
    }

    // define how to set texture-to-sprite import settings
    void SetTextureToSpriteImportSettings(TextureImporter ti)
    {
        ti.textureType = TextureImporterType.Sprite;
    }

    // define how to instantiate the asset (typically an FBX file) in the scene
    void InstantiateAndPlaceAssetAsGameObject(string assetFilePath, Scene scene)
    {
        GameObject gameObjectFromAsset = (GameObject)AssetDatabase.LoadAssetAtPath(assetFilePath, typeof(GameObject));

        // if the game object is null, this is a new file... so refresh the asset database and try again
        if (!gameObjectFromAsset)
        {
            AssetDatabase.Refresh();
            gameObjectFromAsset = (GameObject)AssetDatabase.LoadAssetAtPath(assetFilePath, typeof(GameObject));
        }

        // skip instantiating if it already exists in this scene
        foreach (GameObject g in Transform.FindObjectsOfType<GameObject>())
        {
            if (g.name == gameObjectFromAsset.name)
            {
                Utils.DebugUtils.DebugLog("This object is already present in the model.");
                return;
            }
        }

        // otherwise, instantiate as a prefab with the name of the file
        newlyInstantiatedPrefab = PrefabUtility.InstantiatePrefab(gameObjectFromAsset, scene) as GameObject;
        newlyInstantiatedPrefab.name = gameObjectFromAsset.name;
        Utils.DebugUtils.DebugLog("This object was instantiated in the model hierarchy.");

        // set the flag that an object was just instantiated so we can fix parent/child hierarchy in post-processor
        newlyInstantiated = true;

        // allow additional post processing hits since this model was just instantiated
        globalMaxPostProcessingHits = globalMaxPostProcessingHits + 2;
    }

    // sets an object as a child of the scene container
    // needs to happen in post-processor
    static void SetObjectAsChildOfSceneContainer(GameObject prefab)
    {
        // place the object in the scene's container (used to disable all scene objects)
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        // this assumes there's only 1 object in the scene: a container for all objects
        GameObject sceneContainer = rootObjects[0];

        prefab.transform.SetParent(sceneContainer.transform);
    }

    // define how to delete and reimport materials and textures
    public void DeleteReimportMaterialsTextures(string assetFilePath)
    {
        // initialize ModelImporter
        ModelImporter importer = assetImporter as ModelImporter;

        // set reimport required initially to false
        bool reimportRequired = false;

        // get the material and texture dependencies and delete them
        //var prefab = AssetDatabase.LoadMainAssetAtPath(assetFilePath);
        //foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        foreach (var dependencyPath in AssetDatabase.GetDependencies(globalAssetFilePath, false))
        {
            //var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Utils.DebugUtils.DebugLog("Dependency path: " + dependencyPathString);

            // if there are materials or textures detected in the path, delete them
            // note that we also check that the path includes the file name to avoid other assets from being affected
            if ((dependencyPathString.Contains(".mat") || dependencyPathString.Contains(".jpg") || dependencyPathString.Contains(".jpeg") || dependencyPathString.Contains(".png")) && dependencyPathString.Contains(globalAssetFileName))
            {
                UnityEngine.Windows.File.Delete(dependencyPathString);
                UnityEngine.Windows.File.Delete(dependencyPathString + ".meta");
                Utils.DebugUtils.DebugLog("<b>Deleting files and meta files:</b> " + dependencyPathString);
                reimportRequired = true;
                prevTime = Time.time;
            }

            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }

        // if materials or textures were deleted, force reimport the model
        if (reimportRequired)
        {
            Utils.DebugUtils.DebugLog("Reimport model triggered. Forcing asset update...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        Utils.DebugUtils.DebugLog("Re-importing materials...");

        // import materials
        importer.importMaterials = true;

        // make sure materials are being stored externally
        // and name them based on incoming names
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Local;

        // Materials are automatically stored in a Materials folder (Unity default behavior)

        // Textures are automatically stored in an ".fbm" folder (Unity default behavior)
        // However, for clean organization, we want to store textures in a "Textures" folder
        // the old .FBM folder will be deleted in the post-processor

        // textures will be extracted to a "Textures" folder next to the asset
        string assetTexturesDirectory = globalAssetFileDirectory + "Textures";
        globalAssetTexturesDirectory = assetTexturesDirectory;

        // re-extract textures
        Utils.DebugUtils.DebugLog("Re-importing textures...");
        importer.ExtractTextures(assetTexturesDirectory);

    }

    // define how to enable emission on a material and set its color and texture to emissive
    public static void SetStandardMaterialEmission(string materialFilePath)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // enable emission
        mat.EnableKeyword("_EMISSION");

        // set the material to baked emissive
        var emissiveFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        mat.globalIlluminationFlags = emissiveFlags;

        // set emission color to the material's color
        Color color = mat.GetColor("_Color");
        mat.SetColor("_EmissionColor", color);

        // set the emission map to the material's texture
        Texture texture = mat.GetTexture("_MainTex");
        mat.SetTexture("_EmissionMap", texture);

        Utils.DebugUtils.DebugLog("<b>Set standard emission on Material: </b>" + mat);
    }

    // define how to enable custom emission color and intensity on a material
    public static void SetCustomMaterialEmissionIntensity(string materialFilePath, float intensity)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));
        Color color = mat.GetColor("_Color");

        // for some reason, the desired intensity value (set in the UI slider) needs to be modified slightly for proper internal consumption
        float adjustedIntensity = intensity - (0.4169F);

        // redefine the color with intensity factored in - this should result in the UI slider matching the desired value
        color *= Mathf.Pow(2.0F, adjustedIntensity);
        mat.SetColor("_EmissionColor", color);
        Utils.DebugUtils.DebugLog("<b>Set custom emission intensity of " + intensity + " (" + adjustedIntensity + " internally) on Material: </b>" + mat);
    }

    // define how to set a custom material emission color
    public static void SetCustomMaterialEmissionColor(string materialFilePath, float R, float G, float B)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // if any of the values are provided as RGB 255 format, remap them between 0 and 1
        if (R > 1 || G > 1 || B > 1)
        {
            R = R / 255;
            G = G / 255;
            B = B / 255;
        }

        mat.color = new Color(R, G, B);
        Utils.DebugUtils.DebugLog("<b>Set custom emission color on Material: </b>" + mat);
    }

    // define how to create a greyscale new texture given a scale factor from 0 (black) to 1 (white)
    public static string CreateGreyscaleTexture(float scale)
    {
        // name the metallic map based on its rgb values
        string fileName = "metallicMap-" + scale + "-" + scale + "-" + scale + ".png";

        // define the path where this texture will be stored
        string filePath = globalAssetFileDirectory + "Textures/" + fileName;

        // only make a new texture if it doesn't exist yet
        // note that this texture does not get cleaned up in DeleteReimportMaterialsTextures
        // so it's likely already made and can be reusedused
        if (!AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture)))
        {
            //Utils.DebugUtils.DebugLog("No MetallicGlossMap detected. Creating one.");
            // since the texture is just one color, we don't need a high resolution
            int sizeX = 100;
            int sizeY = 100;

            // convert the given scale factor to a color
            Color color = new Color(scale, scale, scale);

            // create a blank texture at the given size
            Texture2D newTexture = new Texture2D(sizeX, sizeY);

            // fill each pixel the given color
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    newTexture.SetPixel(x, y, color);
                }
            }

            // write the texture to the file system
            UnityEngine.Windows.File.WriteAllBytes(filePath, (byte[])newTexture.EncodeToPNG());

            newTexture.Apply();
        }
        else
        {
            //Utils.DebugUtils.DebugLog("MetallicGlossMap texture already exists.");
        }

        return filePath;
        //return newTexture;
    }


    // define how to set material smoothness
    public static void SetMaterialSmoothness(string materialFilePath, float smoothness)
    {
        Utils.DebugUtils.DebugLog("<b>Set smoothness on Material: </b>" + materialFilePath);

        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // enable the keyword so the canvas updates with new gloss values
        mat.EnableKeyword("_METALLICGLOSSMAP");

        // set its smoothness channel to albedo
        mat.SetInt("_SmoothnessTextureChannel", 1);

        // set it to the given smoothness (glossiness) value
        mat.SetFloat("_GlossMapScale", smoothness);

        // once _METALLICGLOSSMAP keyword is enabled, we have to provide a gloss map
        // otherwise metallic gets set to 100% visually in the scene
        // all materials with gloss need get a black (0) gloss map generated for them to negate the metallic effect
        // Materials may get Metallic set to a non-0 value in a separate function

        // create the black texture
        string metallicGlossTexturePath = CreateGreyscaleTexture(0.0f);
        Texture blackTexture = (Texture)AssetDatabase.LoadAssetAtPath(metallicGlossTexturePath, typeof(Texture));

        // set the MetallicGlossMap as the black texture
        mat.SetTexture("_MetallicGlossMap", blackTexture);
    }

    // define how to set material metallic
    public static void SetMaterialMetallic(string materialFilePath, float metallic)
    {
        // get the material at this path
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialFilePath, typeof(Material));

        // create a greyscale texture based on the specified metallic value
        string metallicGlossTexturePath = CreateGreyscaleTexture(metallic);
        Texture metallicGlossTexture = (Texture)AssetDatabase.LoadAssetAtPath(metallicGlossTexturePath, typeof(Texture));

        // set the MetallicGlossMap as the black texture
        mat.SetTexture("_MetallicGlossMap", metallicGlossTexture);

        Utils.DebugUtils.DebugLog("<b>Set metallic on Material: </b>" + mat);
    }

    // define how to clean up an automatically-created .fbm folder
    public static void CleanUpFBMDirectory(string assetFileDirectory, string assetFileName)
    {
        if (AssetDatabase.IsValidFolder(assetFileDirectory + assetFileName + ".fbm"))
        {
            Utils.DebugUtils.DebugLog("<b>Deleting a leftover .FBM folder.</b>");
            //Utils.DebugUtils.DebugLog(assetFileDirectory + assetFileName + ".fbm");
            UnityEngine.Windows.Directory.Delete(globalAssetFileDirectory + globalAssetFileName + ".fbm");
            UnityEngine.Windows.File.Delete(globalAssetFileDirectory + globalAssetFileName + ".fbm.meta");
        }
    }

    // define how to create a component attached to a GameObject, or delete it and create a new one if component already exists
    public static void RemoveComponentIfExisting(GameObject gameObjectForComponent, string componentType)
    {
        // if this object already has a component of this type, delete it
        if (gameObjectForComponent.GetComponent(componentType) as Component)
        {
            Utils.DebugUtils.DebugLog("<b>Deleted</b> existing behavior component " + componentType + " on " + gameObjectForComponent + ".");
            Component componentToDelete = gameObjectForComponent.GetComponent(componentType) as Component;
            //AudioSource audioSourceToDelete = gameObjectForComponent.gameObject.GetComponent<AudioSource>();

            // delete the existing component
            UnityEngine.Object.DestroyImmediate(componentToDelete);
        }
    }

    // adds Unity Engine components to a GameObject
    public static void AddUnityEngineComponentToGameObject(GameObject gameObjectForComponent, string componentName)
    {
        // define the generic Unity Engine component type
        Type unityEngineComponentType = Type.GetType("UnityEngine." + componentName + ", UnityEngine");

        // first, remove the component if it exists already
        RemoveComponentIfExisting(gameObjectForComponent, componentName);

        // now add the component
        gameObjectForComponent.AddComponent(unityEngineComponentType);

        Utils.DebugUtils.DebugLog("<b>Added</b> " + componentName + " component to " + gameObjectForComponent);
    }

    // adds Unity Engine components to a GameObject's children
    public static void AddUnityEngineComponentToGameObjectChildren(GameObject parentGameObject, string componentName)
    {
        foreach (Transform childTransform in parentGameObject.transform)
        {
            AddUnityEngineComponentToGameObject(childTransform.gameObject, componentName);
        }
    }

    // adds custom components to a GameObject
    public static void AddCustomScriptComponentToGameObject(GameObject gameObjectForComponent, string scriptName)
    {
        // define the type of script based on the name
        Type customScriptComponentType = Type.GetType(scriptName + ", Assembly-CSharp");

        // first, remove the component if it exists already
        RemoveComponentIfExisting(gameObjectForComponent, scriptName);

        // now add the component
        Component customScriptComponent = gameObjectForComponent.AddComponent(customScriptComponentType);

        Utils.DebugUtils.DebugLog("<b>Added</b> " + scriptName + " component to " + gameObjectForComponent);
    }

    // adds custom components to a GameObject's children
    public static void AddCustomScriptComponentToGameObjectChildren(GameObject parentGameObject, string scriptName)
    {
        foreach (Transform childTransform in parentGameObject.transform)
        {
            AddCustomScriptComponentToGameObject(childTransform.gameObject, scriptName);
        }
    }

    // add certain behavior components to certain GameObjects as defined by their host file name
    public static void AddBehaviorComponentsByName(string assetName)
    {
        Utils.DebugUtils.DebugLog("Adding behavior components...");

        // find the associated GameObject by this asset's name, and all of its children objects
        GameObject gameObjectByAssetName = GameObject.Find(assetName);

        //
        // set rules based on name
        // 

        // speakers
        if (assetName.Contains("speakers"))
        {
            // add components to the parent gameObject

            // add components to children gameObjects

            // delete the existing script host objects
            string speakerScriptHostDeleteTag = ManageTags.GetOrCreateTagByScriptHostType("Speakers");
            ManageTaggedObjects.DeleteObjectsByTag(speakerScriptHostDeleteTag);

            foreach (Transform child in gameObjectByAssetName.transform)
            {
                // first, create a host gameobject for all scripts and interactive elements
                GameObject scriptHostObject = new GameObject(child.name + "-ScriptHost");
                // set the script host to nest under the original speaker
                scriptHostObject.transform.parent = child.transform;
                // move the script host to the same position in space - critical for proximity-based scripts!
                scriptHostObject.transform.position = child.transform.position;

                // add the required scripts and behaviors
                AddUnityEngineComponentToGameObject(scriptHostObject, "AudioSource");
                AddCustomScriptComponentToGameObject(scriptHostObject, "PlayAudioSequencesByName");
                AddCustomScriptComponentToGameObject(scriptHostObject, "ToggleComponentByProximityToPlayer");

                ToggleComponentByProximityToPlayer ToggleComponentByProximityScript = scriptHostObject.GetComponent<ToggleComponentByProximityToPlayer>();
                ToggleComponentByProximityScript.toggleComponentTypes = new string[] { "AudioSource", "PlayAudioSequencesByName" };

                // mark this object as a delete candidate
                scriptHostObject.tag = speakerScriptHostDeleteTag;
            }
        }

        // proxy people
        if (assetName.Contains("proxy-people"))
        {
            // add components to the parent gameObject

            // add the script to toggle this entire game object by input event
            AddCustomScriptComponentToGameObject(gameObjectByAssetName, "ToggleObjectsByInputEvent");
            ToggleObjectsByInputEvent ToggleVisibilityScript = gameObjectByAssetName.GetComponent<ToggleObjectsByInputEvent>();

            // add the script to disable components of this object's children by proximity to the player
            AddCustomScriptComponentToGameObject(gameObjectByAssetName, "ToggleChildrenComponentsByProximityToPlayer");
            ToggleChildrenComponentsByProximityToPlayer toggleComponentByProximityScript = gameObjectByAssetName.GetComponent<ToggleChildrenComponentsByProximityToPlayer>();
            toggleComponentByProximityScript.maxDistance = NPCControllerGlobals.maxDistanceBeforeSuspend;
            toggleComponentByProximityScript.toggleComponentTypes = new string[] { "NavMeshAgent", "FollowPathOnNavMesh" };

            // add components to children gameObjects

        }
    }


    //
    // ><><><><><><><><><><><><>
    //
    // these functions must be called in the post-processor
    // some may run multiple times in one import due to Unity post-processor behavior
    //
    // <><><><><><><><><><><><><>
    //

    // define how to get the asset's in-scene gameObject and set it to static
    static void SetAssetAsStaticGameObject(string assetName)
    {
        // get the game object from the global asset name that was changed
        GameObject gameObjectFromAsset = GameObject.Find(globalAssetFileName);
        if (!gameObjectFromAsset)
        {
            Utils.DebugUtils.DebugLog("For some reason, couldn't find the modified game object.");
            return;
        }

        Transform[] allChildren = gameObjectFromAsset.GetComponentsInChildren<Transform>();

        // set the GameObject itself as static, if it isn't already
        if (gameObjectFromAsset.gameObject.isStatic == false)
        {
            gameObjectFromAsset.isStatic = true;
            Utils.DebugUtils.DebugLog("<b>Setting GameObject to static: </b>" + gameObjectFromAsset);
        }
        else
        {
            Utils.DebugUtils.DebugLog("GameObject was already static: " + gameObjectFromAsset);
        }

        // also set each of the GameObject's children as static, if they aren't already
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.isStatic == false)
            {
                child.gameObject.isStatic = true;
                //Utils.DebugUtils.DebugLog("<b>Setting GameObject to static: </b>" + child.gameObject);
            }
            else
            {
                //Utils.DebugUtils.DebugLog("GameObject was already static: " + child.gameObject);
            }
        }
    }

    //define how to look for materials with certain names and add emission to them
    static void SetMaterialEmissionByName()
    {
        // define the asset that was changed as the prefab
        var prefab = AssetDatabase.LoadMainAssetAtPath(globalAssetFilePath);

        // make changes to this prefab's dependencies (materials)
        foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Utils.DebugUtils.DebugLog("Dependency path: " + dependencyPathString);

            //
            // apply general rules
            //

            // all LIGHTs get their color and texture set as emission color/texture
            if (dependencyPathString.Contains("LIGHT"))
            {
                SetStandardMaterialEmission(dependencyPathString);
            }

            //
            // look for certain materials, and modify them further
            //

            if (dependencyPathString.Contains("emission test"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("wayfinding directory"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("fluorescent panel"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            if (dependencyPathString.Contains("green fluor"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.5F);
            }

            if (dependencyPathString.Contains("high intensity white"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.0F);
            }

            if (dependencyPathString.Contains("high intensity green"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.0F);
            }

            if (dependencyPathString.Contains("low intensity yellow"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("very low intensity white"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -1.0F);
            }

            if (dependencyPathString.Contains("americana shop"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -1.0F);
            }

            if (dependencyPathString.Contains("store yellowing"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -0.50F);
            }

            if (dependencyPathString.Contains("food court high intensity"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 3.25F);
            }

            if (dependencyPathString.Contains("food court incandescent"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            if (dependencyPathString.Contains("cinder alley incandescent"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, 2.0F);
            }

            // temporarily reducing the brightness of these until all signage brightness can be adjusted to affset albedo boost,
            // or if using Baked Lightmap instead of Shadowmask, which will make things brighter without an albedo boost override
            if (dependencyPathString.Contains("store rtc sign") ||
                dependencyPathString.Contains("store fl runner"))
            {
                SetCustomMaterialEmissionIntensity(dependencyPathString, -1.0F);
            }

            if (string.IsNullOrEmpty(dependencyPath)) continue;
        }
    }

    // define how to look for materials with certain names and configure their smoothness
    public static void SetMaterialSmoothnessMetallicByName()
    {
        // define the asset that was changed as the prefab
        var prefab = AssetDatabase.LoadMainAssetAtPath(globalAssetFilePath);

        // make changes to this prefab's dependencies (materials)
        foreach (var dependency in EditorUtility.CollectDependencies(new[] { prefab }))
        {
            var dependencyPath = AssetDatabase.GetAssetPath(dependency);
            var dependencyPathString = dependencyPath.ToString();
            //Utils.DebugUtils.DebugLog("Dependency path: " + dependencyPathString);

            //
            // apply general rules
            //

            if (dependencyPathString.Contains("drywall"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.05F);
            }

            if (dependencyPathString.Contains("glass")
                || dependencyPathString.Contains("mirror"))
            {
                SetMaterialSmoothness(dependencyPathString, 1.0F);
            }

            if (dependencyPathString.Contains("metal") && dependencyPathString.Contains(".mat"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.5F);
                SetMaterialMetallic(dependencyPathString, 0.5F);
            }

            //
            // look for certain materials and set their smoothness and metallic values
            //

            if (dependencyPathString.Contains("mall - parquet floor"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.5F);
            }

            if (dependencyPathString.Contains("mall - polished concrete"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.45F);
            }

            if (dependencyPathString.Contains("mall - polished concrete cinder alley")
                || dependencyPathString.Contains("mall - cinder alley scored concrete"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.35F);
            }

            if (dependencyPathString.Contains("mall - shamrock floor brick"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.35F);
                SetMaterialMetallic(dependencyPathString, 0.14F);
            }

            if (dependencyPathString.Contains("mall - shamrock planter brick"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.075F);
            }

            if (dependencyPathString.Contains("mall - stair terrazzo"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.2F);
            }

            if (dependencyPathString.Contains("food court tile"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.2F);
            }

            if (dependencyPathString.Contains("mall - food court ceiling"))
            {
                SetMaterialSmoothness(dependencyPathString, 0.6F);
            }

            if (string.IsNullOrEmpty(dependencyPathString)) continue;
        }
    }

    // define the proxyType based on this asset's name
    public static string GetProxyTypeByName(string assetName)
    {
        if (assetName.Contains("proxy-trees"))
        {
            proxyType = "Trees";
            Utils.DebugUtils.DebugLog("Proxy type: " + proxyType);

            return proxyType;
        }

        if (assetName.Contains("cameras"))
        {
            proxyType = "Cameras";
            Utils.DebugUtils.DebugLog("Proxy type: " + proxyType);

            return proxyType;
        }

        if (assetName.Contains("proxy-people"))
        {
            proxyType = "People";
            Utils.DebugUtils.DebugLog("Proxy type: " + proxyType);

            return proxyType;
        }

        else
        {
            return "";
        }
    }

    public static void ConfigureNPCForPathfinding(GameObject NPCObject)
    {
        // add the script to follow a path
        FollowPathOnNavMesh followPathOnNavMeshScript = NPCObject.AddComponent<FollowPathOnNavMesh>();
        followPathOnNavMeshScript.enabled = false;

        // add the script to update the animation based on the speed
        UpdateNPCAnimatorByState updateAnimatorScript = NPCObject.AddComponent<UpdateNPCAnimatorByState>();

        // add a navigation mesh agent to this person so it can find its way on the navmesh
        NavMeshAgent thisAgent;
        if (!NPCObject.GetComponent<NavMeshAgent>())
        {
            thisAgent = NPCObject.AddComponent<NavMeshAgent>();
        }
        else
        {
            thisAgent = NPCObject.GetComponent<NavMeshAgent>();
        }
        thisAgent.speed = 1.0f;
        thisAgent.angularSpeed = 60f;
        thisAgent.radius = 0.25f;
        thisAgent.autoTraverseOffMeshLink = false;
        thisAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        thisAgent.enabled = false;

        // ensure the agent is moved to a valid location on the navmesh

        // if the distance between the current position, and the nearest navmesh position
        // is less than the max distance, use the closest point on the navmesh
        if (Vector3.Distance(NPCObject.transform.position, Utils.GeometryUtils.GetNearestPointOnNavmesh(NPCObject.transform.position)) < NPCControllerGlobals.maxDistanceForClosestPointAdjustment)
        {
            NPCObject.transform.position = Utils.GeometryUtils.GetNearestPointOnNavmesh(NPCObject.transform.position);
        }
        // otherwise, this person is probably floating in space
        // so find a totally random location on the navmesh for them to go
        else
        {
            NPCObject.transform.position = Utils.GeometryUtils.GetRandomNavMeshPointWithinRadius(NPCObject.transform.position, 1000, false);
        }
    }

    // add the typical controller, nav mesh agent, and associated scripts to a gameObject
    public static void ConfigureNPCForAnimationAndPathfinding(GameObject proxyObject, GameObject NPCObject)
    {
        // if there's a proxy object, check the proxy name for a specific animation to use
        if (proxyObject)
        {
            // set the default animator controller for this person
            Animator thisAnimator = NPCObject.GetComponent<Animator>();
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetDefaultAnimatorControllerFilePathByName(proxyObject.name));

            // TODO: get the initial animation to appear in the editor
            /*
            string animationName = thisAnimator.runtimeAnimatorController.animationClips[0].name;

            setAnimationFrame(thisAnimator, animationName);
            */

            // anyone not the following gestures will get configured to walk
            if (!proxyObject.name.Contains("talking") && !proxyObject.name.Contains("listening") && !proxyObject.name.Contains("idle") && !proxyObject.name.Contains("sitting"))
            {
                ConfigureNPCForPathfinding(NPCObject);
            }
        }

        // otherwise, this is a random filler person and can be configured to walk
        else
        {
            // set the default animator controller for this person
            Animator thisAnimator = NPCObject.GetComponent<Animator>();
            thisAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(ManageNPCControllers.GetWalkingAnimatorControllerByGender(NPCObject.name));

            // TODO: get the initial animation to appear in the editor

            // configure the random filler person for pathfinding
            ConfigureNPCForPathfinding(NPCObject);
        }
    }

    // define how to instantiate proxy replacement objects
    public static void InstantiateProxyReplacements(string assetName)
    {
        // don't do anything if this isn't required
        if (proxyReplacementProcessingRequired == false)
        {
            Utils.DebugUtils.DebugLog("ProxyReplacementProcessing was not required.");
            return;
        }

        // update the global proxy type variable based on the asset name
        string proxyType = GetProxyTypeByName(assetName);

        // define the delete tag to look for, based on the proxyType, then delete existing proxy replacements
        string proxyReplacementDeleteTag = ManageTags.GetOrCreateTagByProxyType(proxyType);
        ManageTaggedObjects.DeleteObjectsByTag(proxyReplacementDeleteTag);

        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        
        // we might get here, if so we need to return to prevent an error
        // TODO: find out why this is sometimes happening
        if (!gameObjectByAsset)
        {
            Utils.DebugUtils.DebugLog("Couldn't find the GameObject by name: " + assetName);
            return;
        }

        // get all the children from the gameObject
        Transform[] allChildren = gameObjectByAsset.GetComponentsInChildren<Transform>();

        // do something with each of the object's children
        foreach (Transform child in allChildren)
        {
            // these objects are visible proxy objects that need to be replaced with Unity equivalents
            if (child.name.Contains("REPLACE"))
            {
                // replace the proxy object with the replacement prefab
                GameObject instancedPrefab = ManageProxyMapping.ReplaceProxyObjectWithPrefab(child.gameObject, proxyType);

                // only do something if the instanced prefab is valid
                if (instancedPrefab)
                {
                    // special rules if we're replacing proxy people
                    if (child.name.Contains("people"))
                    {
                        // apply animator controllers, agents, and scripts to the new prefab
                        ConfigureNPCForAnimationAndPathfinding(child.gameObject, instancedPrefab);

                        // create additional random filler people around this one
                        for (var i = 0; i < ProxyGlobals.numberOfFillersToGenerate; i++)
                        {
                            // create a random point on the navmesh
                            Vector3 randomPoint = Utils.GeometryUtils.GetRandomNavMeshPointWithinRadius(child.transform.localPosition, ProxyGlobals.numberOfFillersToGenerate, false);

                            // determine which pool to get people from given the scene name
                            string[] peoplePrefabPoolForCurrentScene = ManageProxyMapping.GetPeoplePrefabPoolBySceneName(SceneManager.GetActiveScene().name);

                            // instantiate a random person at the point
                            GameObject randomInstancedPrefab = ManageProxyMapping.InstantiateRandomPrefabFromPoolAtPoint(child.gameObject.transform.parent.gameObject, peoplePrefabPoolForCurrentScene, randomPoint);

                            // only do something if the prefab is valid
                            if (randomInstancedPrefab)
                            {
                                // feed this into the NPC configurator to indicate there is no proxy to match
                                GameObject nullProxyObject = null;

                                // apply animator controllers, agents, and scripts to the new random prefab
                                ConfigureNPCForAnimationAndPathfinding(nullProxyObject, randomInstancedPrefab);

                                // tag this instanced prefab as a delete candidate for the next import
                                randomInstancedPrefab.gameObject.tag = proxyReplacementDeleteTag;
                            }
                        }
                    }
                }
            }

            // camera thumbnail objects - these are used to generate thumbnails
            else if (child.name.Contains("Camera-Thumbnail"))
            {
                // get the current scene
                Scene currentScene = EditorSceneManager.GetActiveScene();

                // need to manipulate the default FormIt Group/Instance name to remove the digits at the end

                // first, remove the characters after the last hyphen
                string tempName = child.name.Remove(child.name.LastIndexOf("-"), child.name.Length - child.name.LastIndexOf("-"));
                // then remove the characters after the last hyphen again
                string cameraName = tempName.Remove(tempName.LastIndexOf("-"), tempName.Length - tempName.LastIndexOf("-"));

                // create a new object to host the camera
                // include the name of the current scene (assumes reimporting into the correct scene)
                GameObject cameraObject = new GameObject(cameraName + "-" + currentScene.name);

                // create a camera
                var camera = cameraObject.AddComponent<Camera>();

                // configure the camera to work with PostProcessing
                camera.renderingPath = RenderingPath.DeferredShading;
                camera.allowMSAA = false;
                camera.useOcclusionCulling = false;

                // make the camera a sibling of the original camera geometry
                cameraObject.transform.parent = child.transform.parent;
                // match the position and rotation
                cameraObject.transform.SetPositionAndRotation(child.transform.localPosition, child.transform.localRotation);

                // tag this camera object as a delete candidate for the next import
                cameraObject.tag = proxyReplacementDeleteTag;

                // make the camera look at the plane
                // this assumes the only child of the camera is a plane (a FormIt Group with LCS at the center of the plane)
                cameraObject.transform.LookAt(child.GetChild(0).transform.position);

                // copy the PostProcessing effects from the Main Camera
                PostProcessVolume existingVolume = Camera.main.GetComponent<PostProcessVolume>();
                PostProcessLayer existingLayer = Camera.main.GetComponent<PostProcessLayer>();
                CopyComponent<PostProcessVolume>(existingVolume, cameraObject);
                CopyComponent<PostProcessLayer>(existingLayer, cameraObject);

                // this script writes a new image from the camera's view, from the Editor
                // it will run once, then self-destruct
                RenderCameraToImageSelfDestruct renderCameraScript = cameraObject.AddComponent<RenderCameraToImageSelfDestruct>();

                // specify the path for the camera capture
                renderCameraScript.filePath = UIGlobals.projectUIPath;

                // disable the camera to prevent performance issues
                camera.enabled = false;
            }
        }
    }

    // copies a component and all its settings from one GameObject to another
    static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    // define how to turn off the visibility of proxy assets
    public static void HideProxyObjects(string assetName)
    {
        // find the associated GameObject by this asset's name
        GameObject gameObjectByAsset = GameObject.Find(assetName);
        if (gameObjectByAsset)
        {
            var transformByAsset = gameObjectByAsset.transform;

            // for each of this asset's children, look for any whose name indicates they are proxies to be replaced
            foreach (Transform child in transformByAsset)
            {
                if (child.name.Contains("REPLACE") || (child.name.Contains("Camera")))
                {
                    GameObject gameObjectToBeReplaced = child.gameObject;
                    //Utils.DebugUtils.DebugLog("Found a proxy gameObject to be hide: " + gameObjectToBeReplaced);

                    // turn off the visibility of the object to be replaced
                    ToggleObjects.ToggleGameObjectOff(gameObjectToBeReplaced);
                }
            }
        }
      
    }

    // objects that should impact navigation should kick off a navigation mesh rebuild
    public static void RebuildNavMesh()
    {
        // only do this if the object is also going to be set as static
        if (doSetStatic)
        {
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        }

    }

    // runs when a texture/image asset is updated
    void OnPreprocessTexture()
    {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Utils.DebugUtils.DebugLog("Current time: " + Time.time);
        //Utils.DebugUtils.DebugLog("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            //Utils.DebugUtils.DebugLog("Skipping pre-processing the model again.");
            return;
        }

        ClearConsole();
        Utils.DebugUtils.DebugLog("START Texture PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        TextureImporter textureImporter = assetImporter as TextureImporter;
        String assetFilePath = textureImporter.assetPath.ToLower();
        Utils.DebugUtils.DebugLog("Modified file: " + assetFilePath);

        // make the asset path available globally
        globalAssetFilePath = assetFilePath;

        // get the file name + extension
        String assetFileNameAndExtension = Path.GetFileName(assetFilePath);
        globalAssetFileNameAndExtension = assetFileNameAndExtension;

        // get the file name only
        String assetFileName = assetFileNameAndExtension.Substring(0, assetFileNameAndExtension.Length - 4);
        globalAssetFileName = assetFileName;

        // get the file's directory only
        String assetFileDirectory = assetFilePath.Substring(0, assetFilePath.Length - assetFileNameAndExtension.Length);
        globalAssetFileDirectory = assetFileDirectory;

        //
        // whitelist of files to get modifications
        // only files explicitly mentioned below will get changed
        // each file should state its preference for all available pre- and post-processor flags as defined above
        //

        // all items in the UI folder should be converted to a sprite
        if (assetFilePath.Contains("resources/ui"))
        {
            // pre-processor option flags
            doSetTextureToSpriteImportSettings = true;

            // post-processor option flags
        }

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (doSetTextureToSpriteImportSettings)
        {
            SetTextureToSpriteImportSettings(textureImporter);
        }
    }

    // runs when an audio asset is updated
    void OnPreprocessAudio()
    {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Utils.DebugUtils.DebugLog("Current time: " + Time.time);
        //Utils.DebugUtils.DebugLog("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            //Utils.DebugUtils.DebugLog("Skipping pre-processing the model again.");
            return;
        }

        ClearConsole();
        Utils.DebugUtils.DebugLog("START Audio PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        AudioImporter audioImporter = assetImporter as AudioImporter;
        String assetFilePath = audioImporter.assetPath.ToLower();
        Utils.DebugUtils.DebugLog(assetFilePath);

        // make the asset path available globally
        globalAssetFilePath = assetFilePath;

        // get the file name + extension
        String assetFileNameAndExtension = Path.GetFileName(assetFilePath);
        globalAssetFileNameAndExtension = assetFileNameAndExtension;

        // get the file name only
        String assetFileName = assetFileNameAndExtension.Substring(0, assetFileNameAndExtension.Length - 4);
        globalAssetFileName = assetFileName;

        // get the file's directory only
        String assetFileDirectory = assetFilePath.Substring(0, assetFilePath.Length - assetFileNameAndExtension.Length);
        globalAssetFileDirectory = assetFileDirectory;

        //
        // whitelist of files to get modifications
        // only files explicitly mentioned below will get changed
        // each file should state its preference for all available pre- and post-processor flags as defined above
        //

        // all audio assets need to be compressed for performance
        if (assetFilePath.Contains(".m4a")
            || (assetFilePath.Contains(".mp3"))
            || (assetFilePath.Contains(".wav"))
            || (assetFilePath.Contains(".ogg")))
        {
            // pre-processor option flags
            doSetClipImportSettings = true;

            // post-processor option flags
        }

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (doSetClipImportSettings)
        {
            SetClipImportSettings(audioImporter);
        }

    }

    // runs when a model asset is updated
    void OnPreprocessModel()
    {
        // check if the pre-processor just ran, and if so, skip pre-processing
        //Utils.DebugUtils.DebugLog("Current time: " + Time.time);
        //Utils.DebugUtils.DebugLog("Previous time: " + prevTime);
        if (Time.time == prevTime)
        {
            //Utils.DebugUtils.DebugLog("Skipping pre-processing the model again.");
            return;
        }

        // check if there's a leftover .fbm folder, and if so, delete it
        CleanUpFBMDirectory(globalAssetFileDirectory, globalAssetFileName);

        ClearConsole();
        Utils.DebugUtils.DebugLog("START Model PreProcessing...");

        postProcessingHits.Clear();

        // get the file path of the asset that just got updated
        ModelImporter modelImporter = assetImporter as ModelImporter;
        String assetFilePath = modelImporter.assetPath.ToLower();
        Utils.DebugUtils.DebugLog("Modified file: " + assetFilePath);

        // make the asset path available globally
        globalAssetFilePath = assetFilePath;

        // get the file name + extension
        String assetFileNameAndExtension = Path.GetFileName(assetFilePath);
        globalAssetFileNameAndExtension = assetFileNameAndExtension;

        // get the file name only
        String assetFileName = assetFileNameAndExtension.Substring(0, assetFileNameAndExtension.Length - 4);
        globalAssetFileName = assetFileName;

        // get the file's directory only
        String assetFileDirectory = assetFilePath.Substring(0, assetFilePath.Length - assetFileNameAndExtension.Length);
        globalAssetFileDirectory = assetFileDirectory;


        /* TODO: use a JSON object to store and read the file import properties */
        /*
        // locate the import settings JSON file
        StreamReader reader = new StreamReader(importSettingsPath);
        string json = reader.ReadToEnd();
        object jsonObject = JsonUtility.FromJson<AssetImportUpdate>(json);
        Utils.DebugUtils.DebugLog("Import settings JSON found: " + json);
        //Utils.DebugUtils.DebugLog("Get file names: " + jsonObject.fbxFileNames);
        //return JsonUtility.FromJson<AssetImportUpdate>(json);
        */

        //
        // whitelist of files to get modifications
        // only files explicitly mentioned below will get changed
        // each file should state its preference for all available pre- and post-processor flags as defined above
        //

        if (assetFilePath.Contains("anchor-broadway.fbx")
            || assetFilePath.Contains("anchor-jcp.fbx")
            || assetFilePath.Contains("anchor-joslins.fbx")
            || assetFilePath.Contains("anchor-mgwards.fbx")
            || assetFilePath.Contains("anchor-denver.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = true;
        }

        if (assetFilePath.Contains("mall-doors-windows-exterior.fbx")
            || assetFilePath.Contains("mall-doors-windows-interior.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("mall-doors-windows-solid.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = true;
        }

        if (assetFilePath.Contains("mall-flags.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("mall-furniture.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = true;
        }

        if (assetFilePath.Contains("mall-floor-ceiling-vertical.fbx")
            || assetFilePath.Contains("mall-interior-detailing.fbx")
            || assetFilePath.Contains("mall-exterior-detailing.fbx")
            || assetFilePath.Contains("mall-exterior-walls.fbx")
            || assetFilePath.Contains("store-interior-detailing.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = true;
        }

        if (assetFilePath.Contains("mall-handrails.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("mall-wayfinding"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = true;
            doSetMaterialSmoothnessMetallic = true;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("mall-lights.fbx")
            || assetFilePath.Contains("mall-signage.fbx")
            || assetFilePath.Contains("site-lights.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = true;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("mall-structure.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        if (assetFilePath.Contains("proxy-cameras.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = false;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = true;
            doHideProxyObjects = true;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("proxy-people.fbx")
            || assetFilePath.Contains("proxy-trees.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = true;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = true;
            doHideProxyObjects = true;
            doRebuildNavMesh = false;
        }

        if (assetFilePath.Contains("site.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = true;
            doSetUVActiveAndConfigure = true;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = true;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = true;
        }

        if (assetFilePath.Contains("speakers.fbx") || assetFilePath.Contains("speakers-simple.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = true;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
            doRebuildNavMesh = false;
        }

        //
        // these are temporary fixes
        //
        if (assetFilePath.Contains("temp-fix.fbx"))
        {
            // pre-processor option flags
            doSetGlobalScale = true; // always true
            doInstantiateAndPlaceInCurrentScene = true;
            doSetColliderActive = false;
            doSetUVActiveAndConfigure = false;
            doDeleteReimportMaterialsTextures = true;
            doAddBehaviorComponents = false;

            // post-processor option flags
            doSetStatic = false;
            doSetMaterialEmission = false;
            doSetMaterialSmoothnessMetallic = false;
            doInstantiateProxyReplacements = false;
            doHideProxyObjects = false;
        }

        //
        // now execute all AssetImportUpdate PreProcessor option flags marked as true
        //

        if (doSetGlobalScale)
        {
            SetGlobalScale(modelImporter);
        }

        if (doInstantiateAndPlaceInCurrentScene)
        {
            InstantiateAndPlaceAssetAsGameObject(globalAssetFilePath, currentScene);
        }

        if (doSetColliderActive)
        {
            SetColliderActive(modelImporter);
        }

        if (doSetUVActiveAndConfigure)
        {
            SetUVActiveAndConfigure(modelImporter);
        }

        if (doDeleteReimportMaterialsTextures)
        {
            DeleteReimportMaterialsTextures(globalAssetFilePath);
        }

        if (doAddBehaviorComponents)
        {
            AddBehaviorComponentsByName(globalAssetFileName);
        }

        // since pre-processing is done, mark post-processing as required
        postProcessingRequired = true;
        proxyReplacementProcessingRequired = true;

        // mark the scene as dirty, in case something changed that needs to be saved
        // TODO: create a flag that's set to true when something is really changed, so we only mark dirty when necessary
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // runs after pre-processing the model
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // check if there's a leftover .fbm folder, and if so, delete it
        CleanUpFBMDirectory(globalAssetFileDirectory, globalAssetFileName);

        // if post processing isn't required, skip
        if (!postProcessingRequired)
        {
            Utils.DebugUtils.DebugLog("Skipping PostProcessing (not required)");
            return;
        }

        // it seems that a few post processing hits are needed to fully post-process everything
        // any further is probably not necessary
        if (!postProcessingRequired || postProcessingHits.Count >= globalMaxPostProcessingHits)
        {
            Utils.DebugUtils.DebugLog("Skipping PostProcessing (max allowed reached)");
            return;
        }

        Utils.DebugUtils.DebugLog("START PostProcessing...");

        // add to the list of post processing hits, so we know how many times we've been here
        postProcessingHits.Add(true);

        //
        // execute all AssetImportUpdate PostProcessor option flags marked as true
        //

        if (doSetStatic)
        {
            SetAssetAsStaticGameObject(globalAssetFileName);
        }

        if (doSetMaterialEmission)
        {
            SetMaterialEmissionByName();
        }

        if (doSetMaterialSmoothnessMetallic)
        {
            SetMaterialSmoothnessMetallicByName();
        }

        if (doInstantiateProxyReplacements)
        {
            InstantiateProxyReplacements(globalAssetFileName);
        }

        if (doHideProxyObjects)
        {
            HideProxyObjects(globalAssetFileName);
        }

        if (doRebuildNavMesh)
        {
            RebuildNavMesh();
        }

        // newly-instantiated objects need to be set as a child of the scene container
        // NOTE: this assumes each scene only has 1 top-level object: a "Container" that holds all Scene objects
        if (newlyInstantiated)
        {
            SetObjectAsChildOfSceneContainer(newlyInstantiatedPrefab);
        }

        Utils.DebugUtils.DebugLog("END PostProcessing");
    }
}