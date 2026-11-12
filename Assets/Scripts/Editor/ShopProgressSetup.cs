using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ShopProgressSetup
{
    [MenuItem("Little Shop/Refresh save item catalog")]
    public static void RefreshCatalog()
    {
        const string path = "Assets/Resources/ShopItemCatalog.asset";
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        ShopItemCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopItemCatalog>(path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ShopItemCatalog>();
            AssetDatabase.CreateAsset(catalog, path);
        }
        List<ItemDefinition> items = new List<ItemDefinition>();
        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (string guid in AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/ScriptableObjects" }))
        {
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId)) throw new InvalidOperationException("Every saveable item requires an Item ID.");
            if (!ids.Add(item.ItemId)) throw new InvalidOperationException("Duplicate Item ID: " + item.ItemId);
            items.Add(item);
        }
        items.Sort((a, b) => string.CompareOrdinal(a.ItemId, b.ItemId));
        SerializedObject serialized = new SerializedObject(catalog);
        SerializedProperty array = serialized.FindProperty("items");
        array.arraySize = items.Count;
        for (int i = 0; i < items.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        Debug.Log("Save item catalog updated: " + items.Count + " definitions.");
    }
}
