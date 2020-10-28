using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventService : MonoBehaviour
{
    public string serverUrl;
    public int cooldownBeforeSend;    

    [Serializable]
    public class MyJSON
    {
        public string type;
        public string data;
    }

    private List<string> buffer = new List<string>();
    private IEnumerator coroutine;
    private bool CR_running = false;

    private IEnumerator WaitAndSend()
    {
        CR_running = true;
        yield return new WaitForSeconds(cooldownBeforeSend);
        string json = "{\"events\": [";
        foreach (string elem in buffer)
        {
            json += elem + ",";
        }
        
        json = json.Substring(0, json.Length - 1) + "] }";
        print(json);

        try
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    buffer.Clear();
                    CR_running = false;
                }
                else
                {
                    coroutine = WaitAndSend();
                    StartCoroutine(coroutine);
                }
            }
        }
        catch (Exception ex)
        {
            coroutine = WaitAndSend();
            StartCoroutine(coroutine);
        }       
    }

    public void TrackEvent(string type, string data)
    {
        MyJSON myJSON = new MyJSON();
        myJSON.type = type;
        myJSON.data = data;
        string json = JsonUtility.ToJson(myJSON);        
        buffer.Add(json);
        if (!CR_running)
        {
            coroutine = WaitAndSend();
            StartCoroutine(coroutine);
        }
    }
}
