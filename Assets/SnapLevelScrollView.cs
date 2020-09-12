using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SnapLevelScrollView : MonoBehaviour, IDragHandler, IEndDragHandler
{
    float[] positions;
    float distance;
    [SerializeField]
    private Scrollbar scrollbar;
    [SerializeField]
    private float snapGravity;
    [SerializeField]
    private IntUnityEvent onSelectedItemChanged;

    private RectTransform rectTransform;
    private int lastChildCount;
    private float lastScrollPosition;
    private int currentSelectedItem;

    void Awake()
    {
        // initialise positions
        positions = new float[0];
        distance = 0;
        currentSelectedItem = 0;
        scrollbar.value = 0;
        lastScrollPosition = 0;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // if child count changes, update the positions array.
        if (lastChildCount == transform.childCount) return;

        positions = new float[transform.childCount];
        if (positions.Length > 1)
        {
            distance = 1f / (positions.Length - 1f);
            for (var i = 0; i < positions.Length; i++)
                positions[i] = distance * i;
        }
        else
        {
            distance = 0;
            positions[0] = 0;
        }

        currentSelectedItem = 0;
        scrollbar.value = 0;
        lastScrollPosition = 0;
        lastChildCount = transform.childCount;
        onSelectedItemChanged?.Invoke(0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var deltaX = eventData.delta.x;
        var screenDeltaX = deltaX / rectTransform.sizeDelta.x;

        // invert the delta
        scrollbar.value -= screenDeltaX;
        Mathf.Clamp(scrollbar.value, 0, 1);
        lastScrollPosition = scrollbar.value;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        for (var i = 0; i < positions.Length; i++)
        {
            if (DragIsBeforeLeftEnd(i) && DragIsBeforeRightEnd(i))
            {
                var tween = DOTween.To(() => scrollbar.value, (x) => scrollbar.value = x, positions[i], snapGravity);
                tween.SetEase(Ease.OutBack);

                if (i != currentSelectedItem)
                    onSelectedItemChanged?.Invoke(i);
            }
        }
    }

    bool DragIsBeforeLeftEnd(int index)
    {
        return lastScrollPosition < (positions[index] + (distance / 2));
    }

    bool DragIsBeforeRightEnd(int index)
    {
        return lastScrollPosition > (positions[index] - (distance / 2));
    }
}
