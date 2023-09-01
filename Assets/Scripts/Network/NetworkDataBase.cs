using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.Json;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;

public class NetworkDataBase : MonoBehaviour
{

    public class ServerStats 
    {
        public int      ServerId;
        public int      PlayersOnline;
        public int      MaxPlayers;
        public int      Uptime;
        public string   Scene;
        public int      InGame;
        public int      Port;

        public ServerStats()
        {
            this.PlayersOnline = CSNetworkManager.instance.numPlayers;
            this.MaxPlayers    = 4;
            this.Uptime        = Mathf.FloorToInt(Time.time);
            this.Scene         = SceneManager.GetActiveScene().name;
            this.InGame        = CSNetworkManager.instance.IngameStatus();
            this.Port          = CSNetworkManager.instance.GetPort();
            this.ServerId      = CSNetworkManager.instance.GetPort() - 7776;
        }
    }

    public void StartDatabase()
    {
        

        if (!CSNetworkManager.instance.DeployingAsServer) return;
        if (CSNetworkManager.instance.ignorePort) return;

        StartCoroutine(UpdateServerDBValues());
    }

    IEnumerator UpdateServerDBValues()
    {
        while (true)
        {
            string API_KEY = "123";

            #if UNITY_SERVER

            UnityWebRequest req = new UnityWebRequest();
            API_KEY = "ToJa0okT0YRjwIkDAKhM2r0OWAW0Pfx6";

            try
            {
                
                ServerStats payloadObj = new ServerStats();
                var options = new JsonSerializerOptions { IncludeFields = true };
                string      payloadStr = JsonSerializer.Serialize(payloadObj, options);
                string      url        = "http://143.198.22.120/gameapi.php?method=updateServerStats&params=" + payloadStr;
                req.SetRequestHeader("X-API-KEY", API_KEY);
                req.url                = url;
                req.SendWebRequest();
                Debug.Log("Server is sending data to the database");
            }
            catch (Exception ex)
            {
                Debug.Log("Error has occurred: " + ex.Message);
                Debug.Log($"Lost Connection to database");
            }

            #endif

            
            yield return new WaitForSeconds(5);
        }
    }
}
