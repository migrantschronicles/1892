using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICityMarker
{
    void DrawLabels();

    void DrawLabel(string cityName);

    void UpdateLabels();

    void ClearLabels();
}
