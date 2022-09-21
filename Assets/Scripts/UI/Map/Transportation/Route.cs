using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Route : MonoBehaviour
{
    public GameObject attachedMarker;
    public Sprite untraveledRoute; // Untraveled, but discovered; grey
    public Sprite currentRoute; // Red
    public Sprite traveledRoute; // Brown
    public Sprite waterRoute; // Blue

}
