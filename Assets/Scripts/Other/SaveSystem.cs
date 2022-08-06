using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

public class SaveSystem : MonoBehaviour
{

private Save CreateSaveGameObject()
{
  Save save = new Save();
  save.level = GlobalVariables.nextLevel;
  return save;
}

public void SaveGame()
{
  Save save = CreateSaveGameObject();

  BinaryFormatter bf = new BinaryFormatter();
  FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
  bf.Serialize(file, save);
  file.Close();
  Debug.Log("Game Saved: " + GlobalVariables.nextLevel);
}

public void LoadGame()
{ 
  if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
  {
    BinaryFormatter bf = new BinaryFormatter();
    FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
    Save save = (Save)bf.Deserialize(file);
    file.Close();
    Debug.Log("Game Loaded");
    SceneManager.LoadScene(save.level);

}
  else
  {
    Debug.Log("No game saved!");
  }
}
}
