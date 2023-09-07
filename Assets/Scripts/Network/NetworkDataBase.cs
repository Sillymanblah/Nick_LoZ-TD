using System.Collections;
using UnityEngine;
using System.Text.Json;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;


public class NetworkDataBase : MonoBehaviour
{
    public static NetworkDataBase instance;

    public class ServerStats 
    {
        public int      ServerID;
        public int      PlayersOnline;
        public int      MaxPlayers;
        public int      Uptime;
        public string   Scene;
        public int      InGame;
        public int      Port;
        public string   Version;

        public ServerStats(int _serverID, int _playersOnline, int _maxPlayers, int _Uptime, string _scene, int _ingame, int _port, string _version) 
        {
            this.PlayersOnline = _playersOnline;
            this.MaxPlayers    = _maxPlayers;
            this.Uptime        = _Uptime;
            this.Scene         = _scene;
            this.InGame        = _ingame;
            this.Port          = _port;
            this.ServerID      = _serverID;
            this.Version       = _version;
        }

        /*
        this.PlayersOnline = CSNetworkManager.instance.numPlayers;
        this.MaxPlayers    = 4;
        this.Uptime        = Mathf.FloorToInt(Time.time);
        this.Scene         = SceneManager.GetActiveScene().name;
        this.InGame        = CSNetworkManager.instance.IngameStatus();
        this.Port          = CSNetworkManager.instance.GetPort();
        this.ServerID      = CSNetworkManager.instance.GetPort() - 7776;
        */
    }

    List<ServerSlotsUI> serverList = new List<ServerSlotsUI>();

    private void Start()
    {
        instance = this;
        RefreshServers();
    }

    public void StartDatabase()
    {
        if (!CSNetworkManager.instance.DeployingAsServer) return;
        //if (CSNetworkManager.instance.ignorePort) return;

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
                
                ServerStats payloadObj = new ServerStats(CSNetworkManager.instance.GetPort() - 7776, CSNetworkManager.instance.numPlayers, 4, Mathf.FloorToInt(Time.time), SceneManager.GetActiveScene().name, CSNetworkManager.instance.IngameStatus(), CSNetworkManager.instance.GetPort(), CSNetworkManager.instance.gameVersion);
                var options = new JsonSerializerOptions { IncludeFields = true };
                string      payloadStr = System.Text.Json.JsonSerializer.Serialize(payloadObj, options);
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

            Debug.Log($"bruh");

            yield return new WaitForSeconds(5);
        }
    }

    private void OnApplicationQuit()
    {
        #if UNITY_SERVER

        if (!CSNetworkManager.instance.DeployingAsServer) return;

        string API_KEY = "123";

        UnityWebRequest req = new UnityWebRequest();
        API_KEY = "ToJa0okT0YRjwIkDAKhM2r0OWAW0Pfx6";

        try
        {
            
            ServerStats payloadObj = new ServerStats(CSNetworkManager.instance.GetPort() - 7776, 0, 0, 0, "NA", 0, CSNetworkManager.instance.GetPort(), CSNetworkManager.instance.gameVersion);
            var options = new JsonSerializerOptions { IncludeFields = true };
            string      payloadStr = System.Text.Json.JsonSerializer.Serialize(payloadObj, options);
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
    }
    
    public void RefreshServers()
    {
        //Debug.Log(GetServerData().);
        StartCoroutine(GetServerData(6));
    }

    IEnumerator GetServerData(int numberOfServers)
    {
        Debug.Log($"Refreshing server list");

        for (int i = 0; i < numberOfServers; i++)
        {
            

            UnityWebRequest req = new UnityWebRequest();

            string      url        = "http://143.198.22.120/gameapi.php?method=getServerStats&params=" + (i + 1);
            req.url                = url;
            req.downloadHandler = new DownloadHandlerBuffer();
            float timeToReceive = Time.time;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(req.error);
            }
            else
            {
                Debug.Log(Time.time - timeToReceive);

                ServerStats newStats = JsonConvert.DeserializeObject<ServerStats>(req.downloadHandler.text);

                MainMenuUIManager.instance.serverSlotsList[i].SetServerStats(newStats);
            }
        }
    }
}
