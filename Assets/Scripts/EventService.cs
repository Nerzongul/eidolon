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
    public class ElemJSON
    {        
        public string type;
        public string data;
    }

    [Serializable]
    public class JSON
    {
        public List<ElemJSON> events = new List<ElemJSON>();
    }
        
    private IEnumerator coroutine;
    private bool CR_running = false;
    private JSON eventsJSON = new JSON();

    private IEnumerator WaitAndSend()
    {
        CR_running = true;
        yield return new WaitForSeconds(cooldownBeforeSend);
        string json = JsonUtility.ToJson(eventsJSON);
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
                    eventsJSON.events.Clear();
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
        ElemJSON myJSON = new ElemJSON();
        myJSON.type = type;
        myJSON.data = data;

        eventsJSON.events.Add(myJSON);       
        
        if (!CR_running)
        {
            coroutine = WaitAndSend();
            StartCoroutine(coroutine);
        }
    }
}
