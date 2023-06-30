using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Used for animations on the map, that are playing once and then destroyed (e.g. fishes in the sea).
 * Instantate a OneTimeAnimator prefab to the Animations object of the map in the IngameDiary.
 * Select the prefab that you want to spawn.
 * The prefab must have the OneTimeAnimation component.
 * It should play an animation, and the animation should have an event calling Anim_OnFinished() of the OneTimeAnimation component.
 * Here you can change how often these one time animations should be instantiated.
 * You can also select a zoom range, so they only get spawned if the user zooms in a specific range.
 * Then place empty game objects as children of this OneTimeAnimator object.
 * These serve as places to spawn the animations. 
 * Animations are spawned as a child of the selected empty game object.
 * It is then occupied and another animation won't spawn there until the animation is finished and destroyed.
 */
public class OneTimeAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private Vector2 spawnTimeRange = new Vector2(1, 2);
    [SerializeField]
    private bool useZoomLevelRange = true;
    [SerializeField]
    private Vector2 zoomLevelRange = new Vector2(2, 3);
    [SerializeField]
    private float animationSpeed = 1.0f;

    private float timeToSpawnNext = -1.0f;
    private MapZoom mapZoom;

    private void Start()
    {
        mapZoom = GetComponentInParent<MapZoom>();
        timeToSpawnNext = UnityEngine.Random.Range(spawnTimeRange.x, spawnTimeRange.y);
    }

    private void Update()
    {
        if(timeToSpawnNext > 0.0f)
        {
            timeToSpawnNext -= Time.deltaTime;
            if (timeToSpawnNext <= 0.0f)
            {
                Spawn();
                timeToSpawnNext = UnityEngine.Random.Range(spawnTimeRange.x, spawnTimeRange.y);
            }
        }
    }

    private void Spawn()
    {
        if(useZoomLevelRange && (mapZoom.ZoomLevel < zoomLevelRange.x || mapZoom.ZoomLevel > zoomLevelRange.y))
        {
            // Not in the correct zoom range.
            return;
        }

        if(transform.childCount == 0)
        {
            Debug.LogWarning($"{name} has no children to spawn animation");
            return;
        }

        List<Transform> children = new();
        for(int i = 0; i <  transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            if(child.childCount == 0)
            {
                children.Add(child);
            }
        }

        if(children.Count == 0)
        {
            // No empty gameobject to spawn
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, children.Count);
        GameObject go = Instantiate(prefab, children[randomIndex]);
        OneTimeAnimation anim = go.GetComponent<OneTimeAnimation>();
        anim.Speed = animationSpeed;
        anim.OnAnimationFinished += (anim) =>
        {
            Destroy(anim.gameObject);
        };
    }
}
