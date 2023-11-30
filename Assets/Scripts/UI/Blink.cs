using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
    [SerializeField]
    private Image target;
    [SerializeField]
    private Color targetColor = Color.black;
    [SerializeField]
    private AnimationCurve curve = new AnimationCurve(new Keyframe[]
    {
        new Keyframe(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f),
        new Keyframe(0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f),
        new Keyframe(1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f)
    });
    [SerializeField]
    private float amplitude = 0.25f;
    [SerializeField]
    private float multiplier = 1.0f;
    [SerializeField]
    private bool autoStart = false;

    private float timer = -1.0f;
    private Color startColor;

    public bool IsRunning
    {
        get { return timer >= 0.0f; }
        set 
        { 
            timer = value ? 0.0f : -1.0f; 
            if(!value && target != null)
            {
                target.color = startColor;
            }
        }
    }

    private void Awake()
    {
        if(target == null)
        {
            target = GetComponent<Image>();
        }

        startColor = target.color;

        if(autoStart)
        {
            IsRunning = true;
        }
    }

    private void Update()
    {
        if(timer >= 0.0f)
        {
            timer += Time.deltaTime * multiplier;
            if (timer > 1.0f)
            {
                timer %= 1.0f;
            }

            float value = curve.Evaluate(timer) * amplitude;
            Color current = Color.LerpUnclamped(startColor, targetColor, value);
            target.color = current;
        }
    }
}
