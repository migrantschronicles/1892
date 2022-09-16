using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class TransportationAnimator : MonoBehaviour
{
    public RawImage TransportationImage;

    private Timer timer;

    private float rotationDelta = 1.5f;
    private float rotationStep = 0.5f;
    private int rotationDirection = -1;

    void Start()
    {
        timer = new Timer();
        timer.Interval = 1000;
        timer.Elapsed += Move;
        //timer.Start();
    }

    private void Move(object sender, ElapsedEventArgs e)
    {
        if(TransportationImage.transform.rotation.z > rotationDelta)
        {
            rotationDirection = -1;
        }
        else if(TransportationImage.transform.rotation.z < -rotationDelta)
        {
            rotationDirection = 1;
        }

        TransportationImage.transform.Rotate(new Vector3(0, 0, 1), TransportationImage.transform.rotation.z + rotationDirection * rotationStep);
    }

    void Update()
    {
    }
}
