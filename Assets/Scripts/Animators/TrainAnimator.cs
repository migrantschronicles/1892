using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainAnimator : MonoBehaviour
{
    public RawImage One;
    public RawImage Two;
    public RawImage Three;

    public RawImage DustOne;
    public RawImage DustTwo;

    private TimeSpan time;

    private bool oneUp = true;

    void Update()
    {
        if (time < TimeSpan.FromSeconds(0.5))
        {
            time += TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (time >= TimeSpan.FromSeconds(0.5))
        {
            time = TimeSpan.Zero;

            One.transform.Rotate(new Vector3(0, 0, 1), oneUp ? -0.8f : 0.8f);
            Two.transform.Rotate(new Vector3(0, 0, 1), oneUp ? 0.8f : -0.8f);
            Three.transform.Rotate(new Vector3(0, 0, 1), oneUp ? -0.8f : 0.8f);
            DustOne.transform.Rotate(new Vector3(0, 0, 1), oneUp ? -1f : 1f);
            DustTwo.transform.Rotate(new Vector3(0, 0, 1), oneUp ? 1f : -1f);
            oneUp = !oneUp;
        }
    }
}
