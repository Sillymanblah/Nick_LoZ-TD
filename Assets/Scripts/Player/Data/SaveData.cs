using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public static class SaveData
{
    public static void SaveInventory(UnitInventory inventory)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + @"/player.loz";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(inventory);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerData LoadInventory()
    {
        string path = Application.persistentDataPath + @"/player.loz";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogWarning($"Save file not found in " + path);
            return null;
        }
    }
}
