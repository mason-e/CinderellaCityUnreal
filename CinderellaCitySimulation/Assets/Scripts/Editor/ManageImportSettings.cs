using UnityEditor;

/// <summary>
/// Contains import settings for various importable files, used by AssetImportPipeline to apply certain settings to certain files
/// </summary>

// all import params
public class ModelImportParams
{
    // pre-processor option flags
    public bool doSetGlobalScale = false;
    public bool doInstantiateAndPlaceInCurrentScene = false;
    public bool doSetColliderActive = false;
    public bool doSetUVActiveAndConfigure = false;
    public bool doDeleteReimportMaterialsTextures = false;
    public bool doAddBehaviorComponents = false;

    // post-processor option flags
    public bool doSetMaterialEmission = false;
    public bool doSetMaterialSmoothnessMetallic = false;
    public bool doInstantiateProxyReplacements = false;
    public bool doHideProxyObjects = false;

    // these import params are deprecated, for now,
    // because these operations don't fire reliably in the post-processor
    // so they are manually executed from the CCP Menu
    // leaving here for now in case they can be added back to the import process in the future
    public bool doSetStaticFlags = false;
    public bool doRebuildNavMesh = false;
    public bool doSetCustomLightmapSettings = false;
}

public class TextureImportParams
{
    public bool doSetTextureToSpriteImportSettings = false;
}

public class AudioImportParams
{
    public bool doSetClipImportSettings = false;
}

// return import settings based on the asset name
public class ManageImportSettings
{
    public static ModelImportParams GetModelImportParamsByName(string name)
    {
        ModelImportParams ImportParams = new ModelImportParams();

        switch (name)
        {
            /// typical building or site elements
            /// will receive all material and collider treatments
            
            case string assetOrModelName when 
            // main mall and anchors
            assetOrModelName.Contains("ceilings")
            || assetOrModelName.Contains("detailing-interior")
            || assetOrModelName.Contains("floors-vert")
            || assetOrModelName.Contains("light-shrouds")
            || assetOrModelName.Contains("roof")
            || assetOrModelName.Contains("structure")
            || assetOrModelName.Contains("walls-detailing-exterior")
            || assetOrModelName.Contains("walls-interior")
            // stores
            || assetOrModelName.Contains("store-ceilings")
            || assetOrModelName.Contains("store-detailing")
            || assetOrModelName.Contains("store-floors")
            // site
            || assetOrModelName.Contains("site-context-buildings")
            || assetOrModelName.Contains("site-curb-gutter-sidewalk-vert")
            || assetOrModelName.Contains("site-detailing")
            || assetOrModelName.Contains("site-parking-surface")
            || assetOrModelName.Contains("site-roads"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// special-cased building elements

            case string assetOrModelName when assetOrModelName.Contains("doors-exterior")
            || assetOrModelName.Contains("doors-windows-interior")
            || assetOrModelName.Contains("vents")
            || assetOrModelName.Contains("windows-exterior"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("doors-windows-solid"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-flags"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("mall-furniture"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("store-fixtures"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("handrails"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("lights")
            || assetOrModelName.Contains("signage")
            || assetOrModelName.Contains("wayfinding"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// proxy objects which either get replaced or augmented on import

            case string assetOrModelName when assetOrModelName.Contains("proxy-cameras"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-people"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-trees-veg"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-water"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = true;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("proxy-blocker-npc"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = true;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("speakers") 
            || assetOrModelName.Contains("speakers-simple"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = true;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// temporary files or experimental models for testing

            case string assetOrModelName when assetOrModelName.Contains("temp-fix"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = true;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            case string assetOrModelName when assetOrModelName.Contains("experimental"):
                // pre-processor option flags
                ImportParams.doSetGlobalScale = true; // always true
                ImportParams.doInstantiateAndPlaceInCurrentScene = true;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = true;
                ImportParams.doDeleteReimportMaterialsTextures = true;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = true;
                ImportParams.doSetMaterialSmoothnessMetallic = true;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;

            /// if the name wasn't mentioned here, we do nothing
            /// files need to be specified above to have any special rules applied

            default:
                // pre-processor option flags
                ImportParams.doSetGlobalScale = false;
                ImportParams.doInstantiateAndPlaceInCurrentScene = false;
                ImportParams.doSetColliderActive = false;
                ImportParams.doSetUVActiveAndConfigure = false;
                ImportParams.doDeleteReimportMaterialsTextures = false;
                ImportParams.doAddBehaviorComponents = false;
                // post-processor option flags
                ImportParams.doSetMaterialEmission = false;
                ImportParams.doSetMaterialSmoothnessMetallic = false;
                ImportParams.doInstantiateProxyReplacements = false;
                ImportParams.doHideProxyObjects = false;
                return ImportParams;
        }
    }

    public static TextureImportParams GetTextureImportParamsByPath(string texturePath)
    {
        TextureImportParams importParams = new TextureImportParams();

        switch (texturePath)
        {
            case string path when path.Contains("resources/ui"):

                importParams.doSetTextureToSpriteImportSettings = true;

                return importParams;
            default:
                return importParams;
        }
    }

    public static AudioImportParams GetAudioImportParamsByPath(string audioPath)
    {
        AudioImportParams importParams = new AudioImportParams();

        switch (audioPath)
        {
            case string path when (audioPath.Contains(".m4a")
            || (audioPath.Contains(".mp3"))
            || (audioPath.Contains(".wav"))
            || (audioPath.Contains(".ogg"))):

                importParams.doSetClipImportSettings = true;

                return importParams;
            default:
                return importParams;
        }
    }

    // get the appropriate static editor flags given an asset name
    public static StaticEditorFlags GetStaticFlagsByName(string assetName)
    {
        switch (assetName)
        {
            // no static editor flags
            case string name when (name.Contains("light-shrouds")
            || name.Contains("mall-vents")
            || name.Contains("store-fixtures")
            || name.Contains("structure-concealed")
            || name.Contains("speakers")
            || name.Contains("trees")
            || name.Contains("water")):
                return 0;
            // only navigation static
            case string name when (name.Contains("handrails")
            || name.Contains("furniture")
            || name.Contains("wayfinding")
            || name.Contains("proxy-blocker-npc")) 
            && !name.Contains("solid"):
                return StaticEditorFlags.NavigationStatic;
            // lightmap static only
            case string name when (name.Contains("doors-exterior")
            || name.Contains("Environment")):
                return StaticEditorFlags.LightmapStatic;
            // if not specified, default to all static editor flags
            default:
                return StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OffMeshLinkGeneration | StaticEditorFlags.ReflectionProbeStatic;
        }
    }

    // get the correct shadow map resolution multiplier given an asset name
    // this is a multiplier of the current global Lightmap Resolution value
    public static float GetShadowMapResolutionMultiplierByName(string assetName)
    {
        // assumes a global resolution of 3.2808 (1 texel per foot)
        switch (assetName)
        {
            case string name when name.Contains("mall-lights")
            || name.Contains("signage"):
                return 10f;
            case string name when name.Contains("detailing-interior")
            || name.Contains("detailing-exterior"):
                return 5f;
            case string name when name.Contains("mall-ceilings")
            || name.Contains("store-detailing"):
                return 4f;
            case string name when name.Contains("floors-vert")
            || name.Contains("site-curb-gutter-sidewalk-vert")
            || name.Contains("site-detailing")
            || name.Contains("site-structure")
            || name.Contains("structure-exposed"):
                return 3f;
            case string name when name.Contains("doors")
            || name.Contains("windows"):
                return 2f;
            case string name when name.Contains("walls-interior")
            || name.Contains("store-ceilings")
            || name.Contains("store-floors")
            || name.Contains("store-lights")
            || name.Contains("site-parking-surface"):
                return 0.5f;
            case string name when name.Contains("roof") 
            || name.Contains("site-context-buildings")
            || name.Contains("site-roads"):
                return 0.1f;
            case string name when name.Contains("experimental-simple"):
                return 1f;
            // if not specified, the default is 1 (no change to global resolution for this asset)
            default:
                return 1f;
        }
    }

    // get the replacement proxy type based on an asset name
    public static string GetProxyTypeByName(string assetName)
    {
        switch (assetName)
        {
            case string name when name.Contains("proxy-trees-veg"):
                return "TreesVeg";
            case string name when name.Contains("cameras"):
                return "Cameras";
            case string name when name.Contains("proxy-people"):
                return "People";
            case string name when name.Contains("water"):
                return "Water";
            default:
                return null;
        }
    }

    // get the desired material emission value given a name or path
    public static float GetMaterialEmissionByName(string materialNameOrPath)
    {
        switch (materialNameOrPath)
        {
            // test case
            case string name when name.Contains("emission test"):
                return 1.0f;
            // general cases
            case string name when name.Contains("blue mall ceiling"):
                return 2.25f;
            case string name when name.Contains("blue mall columns"):
                return 3.8f;
            case string name when name.Contains("blue mall cove"):
                return 2.25f;
            case string name when name.Contains("blue mall fountain planter intense"):
                return 3.25f;
            case string name when name.Contains("blue mall hanging planter orange"):
                return -3.25f;
            case string name when name.Contains("blue mall illuminated ring"):
                return 0.75f;
            case string name when name.Contains("cinder alley incandescent"):
                return 3.75f;
            case string name when name.Contains("display case"):
                return 2.0f;
            case string name when name.Contains("exterior white"):
                return 1.0f;
            case string name when name.Contains("food court high intensity"):
                return 3.25f;
            case string name when name.Contains("food court incandescent"):
                return 2.0f;
            case string name when name.Contains("fluorescent panel"):
                return 2.0f;
            case string name when name.Contains("green fluor"):
                return 2.5f;
            case string name when name.Contains("high intensity sodium")
            && !name.Contains("very"):
                return 2.5f;
            case string name when name.Contains("high intensity white"):
                return 3.0f;
            case string name when name.Contains("low intensity black"):
                return -1.0f;
            case string name when name.Contains("low intensity yellow"):
                return 1.0f;
            case string name when name.Contains("low intensity white"):
                return 1.0f;
            case string name when name.Contains("mid-mod exterior fixture"):
                return 0f;
            case string name when name.Contains("very high intensity sodium"):
                return 3.5f;
            case string name when name.Contains("very low intensity white"):
                return -1.0f;
            case string name when name.Contains("wayfinding directory"):
                return 1.0f;
            // stores
            case string name when name.Contains("americana shop"):
                return -1.0f;
            case string name when name.Contains("the denver blue"):
                return -0.2f;
            case string name when name.Contains("funtastics signage"):
                return -1.0f;
            case string name when name.Contains("gart sports white"):
                return -1.0f;
            case string name when name.Contains("k-g men's gold"):
                return -2.0f;
            case string name when name.Contains("neusteters brown"):
                return -0.2f;
            case string name when name.Contains("penney's white"):
                return 0.7f;
            case string name when name.Contains("store yellowing"):
                return -0.50f;
            case string name when name.Contains("woolworth's red"):
                return -1.0f;
            // store sign graphics
            case string name when name.Contains("store rtc sign")
            || name.Contains("store fl runner")
            || name.Contains("rich burger icon"):
                return -1.0f;
            // artwork - gets a small amount of illumination so it doesn't appear dim
            case string name when name.Contains("artwork"):
                return -1.0f;
            // if not specified, return a specific number to indicate to calling functions 
            // that this material is not intended to have emission
            default:
                return 0.001f;
        }
    }

    // get the desired metallic for a given material or path
    public static float GetMaterialMetallicByName(string materialNameOrPath)
    {
        switch (materialNameOrPath)
        {
            // general rules
            case string name when name.Contains("metal"):
                return 0.5f;
            // specific materials
            case string name when name.Contains("blue mall hanging planter orange"):
                return 0.75f;
            case string name when name.Contains("mall - shamrock floor brick"):
                return 0.14f;
            // if not specified, return -1 to indicate to calling functions that
            // this material is not intended to have a custom specular value
            default:
                return -1;
        }
    }

    // get the desired smoothness for a given material or path
    public static float GetMaterialSmoothnessByName(string materialNameOrPath)
    {
        switch (materialNameOrPath)
        {
            // general rules
            case string name when name.Contains("glass")
            || name.Contains("mirror"):
                return 1.0f;
            case string name when name.Contains("glossy"):
                return 0.8f;
            case string name when name.Contains("metal"):
                return 0.5f;
            // specific materials
            case string name when name.Contains("blue mall hanging planter orange"):
                return 0f;
            case string name when name.Contains("bronzed glass"):
                return 0.2f;
            case string name when name.Contains("food court tile"):
                return 0.2f;
            case string name when name.Contains("generic floor concrete"):
                return 0.3f;
            case string name when name.Contains("mall - food court ceiling"):
                return 0.6f;
            case string name when name.Contains("mall - parquet floor"):
                return 0.5f;
            case string name when name.Contains("mall - polished concrete"):
                return 0.45f;
            case string name when name.Contains("mall - polished concrete cinder alley")
            || name.Contains("mall - cinder alley scored concrete"):
                return 0.27f;
            case string name when name.Contains("mall - polished concrete"):
                return 0.45f;
            case string name when name.Contains("mall - shamrock floor brick"):
                return 0.35f;
            case string name when name.Contains("mall - shamrock planter brick"):
                return 0.075f;
            case string name when name.Contains("mall - stair terrazzo"):
                return 0.2f;
            case string name when name.Contains("mall - terra cotta tile special"):
                return 0.2f;
            // if not specified, return -1 to indicate to calling functions that
            // this material is not intended to have a custom specular value
            default:
                return -1;
        }
    }

    // get the desired specular value (out of 255) for a given material name or path
    public static int GetMaterialSpecularByName(string materialNameOrPath)
    {
        switch (materialNameOrPath)
        {
            case string name when name.Contains("concrete - cast white")
            || name.Contains("concrete - cast unpainted")
            || name.Contains("concrete - sidewalk")
            || name.Contains("site-structure")
            || name.Contains("railing paint color"):
                return 0;
            case string name when name.Contains("brick - painted white aged")
            || name.Contains("concrete - foundation wall")
            || name.Contains("mall - cmu"):
                return 5;
            case string name when name.Contains("mall - upper asphalt"):
                return 20;
            case string name when name.Contains("acoustic tile")
            || name.Contains("anchor smooth accent")
            || name.Contains("anchor rough accent")
            || name.Contains("concrete - painted 60s")
            || name.Contains("concrete - painted 80s")
            || name.Contains("drywall")
            || name.Contains("mall - brick")
            || name.Contains("mall - stucco")
            || name.Contains("mall - precast panels")
            || name.Contains("mall - loading dock concrete")
            || name.Contains("store - stacked brick")
            || name.Contains("store - dark brown")
            || name.Contains("store - neusteters travertine"):
                return 30;
            case string name when name.Contains("concrete - garage painted ceiling")
            || name.Contains("drywall - exerior")
            || name.Contains("mall - CMU painted")
            || name.Contains("mall - lower asphalt"):
                return 35;
            // if not specified, return -1 to indicate to calling functions that
            // this material is not intended to have a custom specular value
            default:
                return -1;
        }
    }
}