using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDiaryPage
{
    void SetData(DiaryEntryData entryData, DiaryPageData data);
    IEnumerable<ElementAnimator> CreateAnimators();
    List<Vector2> GetTextFieldWeights() { return new List<Vector2>(); }
}
