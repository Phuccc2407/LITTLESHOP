using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CustomerReview
{
    public string Visitor;
    public int Stars;
    public int Plays;
    public string Comment;
}

// Stored independently of scene objects; callers can subscribe without knowing the UI.
public static class CustomerReviews
{
    [Serializable] private sealed class SaveData { public List<CustomerReview> Reviews = new List<CustomerReview>(); }
    private const string SaveKey = "LittleShop.CustomerReviews.v1";
    private static SaveData data;
    public static event Action Changed;
    public static IReadOnlyList<CustomerReview> All { get { EnsureLoaded(); return data.Reviews.AsReadOnly(); } }
    public static float AverageStars
    {
        get
        {
            EnsureLoaded();
            if (data.Reviews.Count == 0) return 3f;
            float total = 0f;
            foreach (CustomerReview review in data.Reviews) total += review.Stars;
            return total / data.Reviews.Count;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { data = null; Changed = null; }

    private static void EnsureLoaded()
    {
        if (data != null) return;
        try { data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey, "")); }
        catch (Exception) { data = null; }
        if (data == null) data = new SaveData();
        if (data.Reviews == null) data.Reviews = new List<CustomerReview>();
        data.Reviews.RemoveAll(r => r == null);
    }

    public static void Add(string visitor, float satisfaction, int plays, string reason)
    {
        EnsureLoaded();
        data.Reviews.Insert(0, new CustomerReview
        {
            Visitor = visitor, Stars = Mathf.Clamp(Mathf.CeilToInt(satisfaction / 20f), 1, 5),
            Plays = plays, Comment = reason
        });
        if (data.Reviews.Count > 100) data.Reviews.RemoveAt(data.Reviews.Count - 1);
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static void RestoreProgress(List<CustomerReview> reviews)
    {
        data = new SaveData();
        if (reviews != null)
            foreach (CustomerReview review in reviews)
                if (review != null && data.Reviews.Count < 100) data.Reviews.Add(review);
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
