using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class LittleShopUpgradeInstaller
{
    private const string GameScene = "Assets/PolygonShops/Scenes/Demo_01.unity";

    [MenuItem("Little Shop/Install shop upgrades")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before installing upgrades.");
        Scene scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        ShopDayCycle clock = UnityEngine.Object.FindAnyObjectByType<ShopDayCycle>();
        if (clock == null) throw new InvalidOperationException("The shop clock is missing from the game scene.");

        AudioManager audio = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (audio == null) audio = new GameObject("AudioManager").AddComponent<AudioManager>();
        if (audio.GetComponent<ShopAudio>() == null) audio.gameObject.AddComponent<ShopAudio>();
        if (clock.GetComponent<ShopDayLighting>() == null) clock.gameObject.AddComponent<ShopDayLighting>();

        int doors = 0;
        foreach (CustomerDoor door in UnityEngine.Object.FindObjectsByType<CustomerDoor>(FindObjectsInactive.Include))
        {
            EndDayDoor endDay = door.GetComponent<EndDayDoor>();
            if (endDay == null) endDay = door.gameObject.AddComponent<EndDayDoor>();
            SetObject(endDay, "shopDayCycle", clock);
            // Keep the interaction target reachable through the existing door mesh collider.
            if (door.GetComponentInChildren<Collider>(true) == null)
            {
                BoxCollider collider = door.gameObject.AddComponent<BoxCollider>();
                collider.center = Vector3.up;
                collider.size = new Vector3(1f, 2f, 0.2f);
            }
            doors++;
        }
        if (doors == 0) throw new InvalidOperationException("No customer entrance door was found.");

        foreach (ComputerView computer in UnityEngine.Object.FindObjectsByType<ComputerView>(FindObjectsInactive.Include))
        {
            ComputerReviewsPanel reviews = computer.GetComponent<ComputerReviewsPanel>();
            if (reviews == null) reviews = computer.gameObject.AddComponent<ComputerReviewsPanel>();
            reviews.Build(computer.transform);
            EditorUtility.SetDirty(reviews);
        }

        foreach (TMP_InputField input in UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include))
        {
            if (input.GetComponentInParent<CoinSaleView>() == null) continue;
            input.text = string.Empty;
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            if (input.placeholder is TMP_Text placeholder) placeholder.text = "Enter requested coin amount";
        }
        foreach (TMP_Text label in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            if (label.text == "New Text") label.text = string.Empty;
            if (label.text.StartsWith("DAY ") || label.text.StartsWith("Day ")) label.text = "Thursday, 01/01/2026";
        }

        Material screenMaterial = GetScreenMaterial();
        int prefabCount = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Items" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                ArcadeMachine machine = root.GetComponentInChildren<ArcadeMachine>(true);
                if (machine == null) continue;
                ConfigureMachine(machine, screenMaterial);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                prefabCount++;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        foreach (ArcadeMachine machine in UnityEngine.Object.FindObjectsByType<ArcadeMachine>(FindObjectsInactive.Include))
            ConfigureMachine(machine, screenMaterial);

        ShopDaySummaryView summary = UnityEngine.Object.FindAnyObjectByType<ShopDaySummaryView>(FindObjectsInactive.Include);
        if (summary == null) throw new InvalidOperationException("The revenue summary UI is missing.");
        SetObject(summary, "shopDayCycle", clock);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        // The shipped build must open the actual shop scene rather than the empty sample scene.
        bool hasMenu = File.Exists(GameSession.MainMenuScene);
        EditorBuildSettings.scenes = (hasMenu
                ? new[] { new EditorBuildSettingsScene(GameSession.MainMenuScene, true), new EditorBuildSettingsScene(GameScene, true) }
                : new[] { new EditorBuildSettingsScene(GameScene, true) })
            .Concat(EditorBuildSettings.scenes.Where(s => s.path != GameScene && s.path != GameSession.MainMenuScene)
                .Select(s => new EditorBuildSettingsScene(s.path, false))).ToArray();
        AssetDatabase.SaveAssets();
        ShopUpgradeChecks.Run();
        Debug.Log("LITTLE_SHOP_UPGRADE_OK: installed reviews, audio, calendar, door, lighting and " + prefabCount + " machine prefabs.");
    }

    private static Material GetScreenMaterial()
    {
        const string folder = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Materials");
        const string path = folder + "/ArcadeScreen.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        material = new Material(shader);
        material.SetColor("_BaseColor", new Color(0.015f, 0.025f, 0.04f));
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void ConfigureMachine(ArcadeMachine machine, Material material)
    {
        ArcadeMachineNavigation navigation = machine.GetComponent<ArcadeMachineNavigation>();
        if (navigation == null) navigation = machine.gameObject.AddComponent<ArcadeMachineNavigation>();
        navigation.Configure();
        ArcadeMachineMedia media = machine.GetComponent<ArcadeMachineMedia>();
        if (media == null) media = machine.gameObject.AddComponent<ArcadeMachineMedia>();
        Transform screen = machine.transform.Find("ArcadeScreen");
        if (screen == null)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "ArcadeScreen";
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            screen = quad.transform;
            screen.SetParent(machine.transform, false);
            Bounds bounds = navigation.LocalBounds;
            Vector3 towardsPlayer = machine.transform.InverseTransformPoint(machine.PlayPoint.position) - bounds.center;
            bool front = towardsPlayer.z >= 0f;
            screen.localPosition = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * 0.72f,
                front ? bounds.max.z + 0.015f : bounds.min.z - 0.015f);
            screen.localRotation = Quaternion.Euler(0f, front ? 180f : 0f, 0f);
            float width = Mathf.Clamp(bounds.size.x * 0.65f, 0.3f, 1.1f);
            screen.localScale = new Vector3(width, width * 0.5625f, 1f);
            Renderer renderer = quad.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        SerializedObject serialized = new SerializedObject(media);
        SerializedProperty screens = serialized.FindProperty("screenRenderers");
        if (screens.arraySize == 0)
        {
            screens.arraySize = 1;
            screens.GetArrayElementAtIndex(0).objectReferenceValue = screen.GetComponent<Renderer>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorUtility.SetDirty(navigation);
        EditorUtility.SetDirty(media);
        PrefabUtility.RecordPrefabInstancePropertyModifications(machine.gameObject);
    }

    private static void SetObject(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
