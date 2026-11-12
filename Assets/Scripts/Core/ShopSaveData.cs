using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ShopSaveData
{
    public string Format = "LittleShop.Save";
    public int Version = 1;
    public string SavedAtUtc;
    public int Cash, Revenue, Expenses, DailyRevenue, DailyExpenses;
    public int DayNumber = 1;
    public float TimeMinutes = 540f;
    public bool ShopOpen;
    public int CustomersSpawned, CustomersServed;
    public List<SavedItemStack> Inventory = new List<SavedItemStack>();
    public List<SavedMachine> Machines = new List<SavedMachine>();
    public List<SavedBox> Boxes = new List<SavedBox>();
    public List<SavedOrder> Orders = new List<SavedOrder>();
    public List<CustomerReview> Reviews = new List<CustomerReview>();
    public bool HasPlayer;
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation = Quaternion.identity;
    public float CameraPitch;
}

[Serializable] public sealed class SavedItemStack { public string ItemId; public int Quantity; }
[Serializable] public sealed class SavedMachine
{
    public string ItemId;
    public Vector3 Position;
    public Quaternion Rotation = Quaternion.identity;
    public Vector3 Scale = Vector3.one;
    public int Condition, CoinsPerPlay, Revenue;
    public bool Active;
}
[Serializable] public sealed class SavedBox
{
    public string ItemId;
    public Vector3 Position;
    public Quaternion Rotation = Quaternion.identity;
}
[Serializable] public sealed class SavedOrder { public string ItemId; public float RemainingSeconds; }
