using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FootAnimation : MonoBehaviour
{
    public RawImage MotherHead;
    public RawImage MotherBody;
    public RawImage BoyHead;
    public RawImage BoyBody;
    public RawImage GirlHead;
    public RawImage GirlBody;
    public RawImage Dust;

    private TimeSpan time;
    private TimeSpan boyTime;
    private TimeSpan girlTime;

    private bool motherUp;
    private bool boyUp;
    private bool girlUp;

    void Update()
    {
        if (time < TimeSpan.FromSeconds(0.6))
        {
            time += TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (time >= TimeSpan.FromSeconds(0.6))
        {
            time = TimeSpan.Zero;

            MotherHead.transform.Rotate(new Vector3(0, 0, 1), motherUp ? 1f : -1f);
            MotherBody.transform.Rotate(new Vector3(0, 0, 1), motherUp ? -2f : 2f);
            motherUp = !motherUp;
        }

        if (girlTime < TimeSpan.FromSeconds(0.4))
        {
            girlTime += TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (girlTime >= TimeSpan.FromSeconds(0.4))
        {
            girlTime = TimeSpan.Zero;

            GirlHead.transform.Rotate(new Vector3(0, 0, 1), girlUp ? -1.4f : 1.4f);
            GirlBody.transform.Rotate(new Vector3(0, 0, 1), girlUp ? 2f : -2f);
            girlUp = !girlUp;
        }

        if (boyTime < TimeSpan.FromSeconds(0.3))
        {
            boyTime += TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (boyTime >= TimeSpan.FromSeconds(0.3))
        {
            boyTime = TimeSpan.Zero;

            BoyHead.transform.Rotate(new Vector3(0, 0, 1), boyUp ? -2f : 2f);
            BoyBody.transform.Rotate(new Vector3(0, 0, 1), boyUp ? 1f : -1f);
            boyUp = !boyUp;

            Dust.transform.Rotate(new Vector3(0, 0, 1), boyUp ? 2f : -2f);
        }
    }
}
