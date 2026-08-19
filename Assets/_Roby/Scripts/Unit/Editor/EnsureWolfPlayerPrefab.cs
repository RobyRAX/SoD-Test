using Animancer;
using RAXY.Animation;
using RAXY.Movement;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class EnsureWolfPlayerPrefab
{
    const string SourcePrefabPath = "Assets/_Roby/_Content_0.0.0/Character/Hero/Shameel/Wolf_Model_Prefabed.prefab";
    const string PrefabPath = "Assets/_Roby/_Content_0.0.0/Character/Hero/Shameel/Wolf_Player.prefab";
    const string ClipsPath = "Assets/_Roby/_Content_0.0.0/Character/Hero/Shameel/PlayerAnimationClips.asset";
    const string ConfigPath = "Assets/_Roby/Scripts/Unit/Brain/Active Unit Brain/ActiveUnitBrainConfig.asset";

    static EnsureWolfPlayerPrefab()
    {
        EditorApplication.delayCall += Ensure;
    }

    [MenuItem("RAXY/Unit/Ensure Wolf Player Prefab")]
    static void Ensure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            return;

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
        if (source == null)
        {
            Debug.LogWarning("[EnsureWolfPlayerPrefab] Missing " + SourcePrefabPath);
            return;
        }

        var clips = AssetDatabase.LoadAssetAtPath<PlayerAnimationClipsSO>(ClipsPath);
        var config = AssetDatabase.LoadAssetAtPath<ActiveUnitBrainConfigSO>(ConfigPath);

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = "Wolf_Player";

        if (instance.GetComponent<Animator>() == null)
            instance.AddComponent<Animator>();
        if (instance.GetComponent<AnimancerComponent>() == null)
            instance.AddComponent<AnimancerComponent>();
        if (instance.GetComponent<AnimancerController>() == null)
            instance.AddComponent<AnimancerController>();
        if (instance.GetComponent<UnitMovement>() == null)
            instance.AddComponent<UnitMovement>();
        if (instance.GetComponent<CharacterController>() == null)
            instance.AddComponent<CharacterController>();
        if (instance.GetComponent<GroundChecker>() == null)
            instance.AddComponent<GroundChecker>();
        if (instance.GetComponent<PlayerUnitController>() == null)
            instance.AddComponent<PlayerUnitController>();

        var cc = instance.GetComponent<CharacterController>();
        cc.height = 1.8f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.radius = 0.35f;

        var ground = instance.GetComponent<GroundChecker>();
        ground.GroundCheckLayers = ~0;

        var movement = instance.GetComponent<UnitMovement>();
        movement.enableRotation = true;

        var playerSo = new SerializedObject(instance.GetComponent<PlayerUnitController>());
        playerSo.FindProperty("animationClips").objectReferenceValue = clips;
        playerSo.FindProperty("brainType").enumValueIndex = (int)BrainType.ActiveUnit;
        playerSo.FindProperty("activeUnitBrainConfig").objectReferenceValue = config;
        playerSo.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        Debug.Log("[EnsureWolfPlayerPrefab] Created " + PrefabPath);
    }
}
