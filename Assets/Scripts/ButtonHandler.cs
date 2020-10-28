using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public EventService eventService;

    public string eventType;
    public string eventData;

    public void SendInfoToTheServer() 
    {        
        eventService.TrackEvent(eventType, eventData);
    }
}
