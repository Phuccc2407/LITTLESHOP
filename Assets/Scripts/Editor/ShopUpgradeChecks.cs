using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public static class ShopUpgradeChecks
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Shop upgrade check failed: " + message);
    }

    [MenuItem("Little Shop/Check shop upgrades")]
    public static void Run()
    {
        GameObject temporary = new GameObject("Calendar test");
        try
        {
            ShopDayCycle clock = temporary.AddComponent<ShopDayCycle>();
            int summaries = 0;
            int datesChanged = 0;
            clock.DayEnded += day =>
            {
                summaries++;
                Check(clock.DayNumber == day, "summary must read the completed date before advancing");
            };
            clock.DayChanged += day => datesChanged++;
            Check(clock.CurrentDate == new DateTime(2026, 1, 1), "initial date");
            Check(clock.DateLabel == "Thursday, 01/01/2026", "English weekday and date");
            Check(clock.EndDay(), "early end succeeds");
            Check(clock.CurrentDate == new DateTime(2026, 1, 2), "advance one day");
            Check(clock.CurrentHour == 9 && clock.State == ShopDayState.Closed, "next day starts closed at 09:00");
            Check(!clock.EndDay() && summaries == 1, "summary guards duplicate day ending");
            clock.DismissSummary();
            Check(clock.EndDay() && summaries == 2 && datesChanged == 2, "next day can end after dismissing summary");
        }
        finally { UnityEngine.Object.DestroyImmediate(temporary); }

        ComputerReviewsPanel reviews = UnityEngine.Object.FindAnyObjectByType<ComputerReviewsPanel>(FindObjectsInactive.Include);
        Check(reviews != null, "reviews component is saved in the scene");
        SerializedObject reviewData = new SerializedObject(reviews);
        Check(reviewData.FindProperty("panel").objectReferenceValue != null, "reviews panel is saved");
        Check(reviewData.FindProperty("text").objectReferenceValue != null, "review text is saved");
        Button reviewsButton = (Button)reviewData.FindProperty("reviewsButton").objectReferenceValue;
        Check(reviewsButton != null && reviewsButton.gameObject.activeSelf, "reviews tab button exists");
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)reviewsButton.transform.parent);
        Check(((RectTransform)reviewsButton.transform).rect.width > 50f, "review button has a usable width");

        foreach (EndDayDoor door in UnityEngine.Object.FindObjectsByType<EndDayDoor>(FindObjectsInactive.Include))
        {
            Check(new SerializedObject(door).FindProperty("shopDayCycle").objectReferenceValue != null, "door clock is wired");
            Check(door.GetComponentInChildren<Collider>(true) != null, "door has an interaction collider");
        }

        int machines = 0;
        foreach (ArcadeMachine machine in UnityEngine.Object.FindObjectsByType<ArcadeMachine>(FindObjectsInactive.Include))
        {
            machines++;
            ArcadeMachineNavigation navigation = machine.GetComponent<ArcadeMachineNavigation>();
            Check(navigation != null, "machine navigation component is saved");
            navigation.Configure();
            NavMeshObstacle obstacle = machine.GetComponent<NavMeshObstacle>();
            Check(obstacle != null && obstacle.carving && obstacle.enabled, "machine carves NavMesh");
            machine.SetPlacementPreview(true);
            Check(!obstacle.enabled && machine.State == ArcadeMachineState.Inactive, "preview does not carve or accept customers");
            machine.SetPlacementPreview(false);
            Check(obstacle.enabled, "placed machine resumes carving");
            ArcadeMachineMedia media = machine.GetComponent<ArcadeMachineMedia>();
            Check(media != null, "machine media component exists");
            SerializedProperty screens = new SerializedObject(media).FindProperty("screenRenderers");
            Check(screens.arraySize > 0 && screens.GetArrayElementAtIndex(0).objectReferenceValue != null, "machine screen is wired");
        }
        Check(machines > 0, "scene contains arcade machines");

        CoinSaleView sale = UnityEngine.Object.FindAnyObjectByType<CoinSaleView>(FindObjectsInactive.Include);
        Check(sale != null, "coin sale UI exists");
        TMP_InputField input = (TMP_InputField)new SerializedObject(sale).FindProperty("coinInput").objectReferenceValue;
        Check(input != null && string.IsNullOrEmpty(input.text), "coin input starts empty");
        Check(UnityEngine.Object.FindAnyObjectByType<AudioManager>() != null, "audio manager is saved in scene");
        Check(UnityEngine.Object.FindAnyObjectByType<ShopDayLighting>() != null, "day lighting is saved in scene");
        Debug.Log("LITTLE_SHOP_CHECKS_OK: calendar rollover, duplicate guard, saved reviews UI, manual coins, door, navigation previews, media, audio and lighting.");
    }
}
