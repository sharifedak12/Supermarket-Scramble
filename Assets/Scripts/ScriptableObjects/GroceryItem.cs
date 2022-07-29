using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Grocery Item", menuName = "Grocery Item", order = 1)]
public class GroceryItem : ScriptableObject {
    public string objectName = "Grocery Item";
    public Sprite icon = null;

    public string itemName {
        get {
            return objectName;
        }
    }

    public Sprite image {
        get {
            return icon;
        }
    }
}