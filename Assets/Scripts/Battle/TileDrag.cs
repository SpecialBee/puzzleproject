using UnityEngine;
using UnityEngine.EventSystems;

public class TileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("타일 데이터")]
    public TileData myTileData;

    public bool isPlaced = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    // [수정 - D-03] transform.root 비교 대신 명확한 플래그 사용
    private bool wasDropped = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        wasDropped = false; // 드래그 시작 시 초기화
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced)
        {
            canvasGroup.blocksRaycasts = false;
            this.enabled = false;
            return;
        }

        canvasGroup.blocksRaycasts = true;

        // [수정 - D-03] wasDropped 플래그로 슬롯 안착 여부 판단 (Canvas 구조에 독립적)
        if (!wasDropped)
        {
            ReturnToHand();
        }
    }

    // TileSlot.OnDrop에서 정상 안착 시 호출됩니다.
    public void NotifyDropped()
    {
        wasDropped = true;
    }

    public void ReturnToHand()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}