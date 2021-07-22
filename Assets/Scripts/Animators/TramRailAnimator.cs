using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TramRailAnimator : MonoBehaviour
{
    public RawImage HorseOne;
    public RawImage HorseTwo;
    public RawImage Coach;
    public RawImage DustOne;
    public RawImage DustTwo;

    private TimeSpan time;
    private TimeSpan coachTime;

    private bool horseOneUp;
    private bool coachUp;
    private bool dustOneUp;

    void Update()
    {
        if (time < TimeSpan.FromSeconds(0.6))
        {
            time += TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (time >= TimeSpan.FromSeconds(0.6))
        {
            time = TimeSpan.Zero;

            HorseOne.transform.Rotate(new Vector3(0, 0, 1), horseOneUp ? -6f : 6f);
            HorseTwo.transform.Rotate(new Vector3(0, 0, 1), horseOneUp ? 6f : -6f);
            horseOneUp = !horseOneUp;

            DustOne.transform.Rotate(new Vector3(0, 0, 1), dustOneUp ? -0.4f : 0.4f);
            DustTwo.transform.Rotate(new Vector3(0, 0, 1), dustOneUp ? 0.4f : -0.4f);
            dustOneUp = !dustOneUp;
        }

        if (coachTime < TimeSpan.FromSeconds(0.2))
        {
            coachTime += TimeSpan.FromSeconds(Time.deltaTime);
        }

        if (coachTime >= TimeSpan.FromSeconds(0.2))
        {
            coachTime = TimeSpan.Zero;

            Coach.transform.Rotate(new Vector3(0, 0, 1), coachUp ? -0.2f : 0.2f);
            coachUp = !coachUp;
        }
    }
}
