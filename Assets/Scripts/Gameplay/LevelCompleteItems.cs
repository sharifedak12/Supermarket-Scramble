using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompleteItems : MonoBehaviour
{
    public static List<ScriptableObject> items = new List<ScriptableObject>();
    public static int CurrentSceneIndex;
    public static int NextSceneIndex;


    public static void AddItems(List <ScriptableObject> newItems)
    {
        items.AddRange(newItems);
    }

    public static void ClearItems()
    {
        items.Clear();
    }
}