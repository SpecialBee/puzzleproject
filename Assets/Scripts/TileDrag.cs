using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // UI 터치 기능을 쓰기 위해 꼭 필요합니다!

// IBeginDragHandler, IDragHandler, IEndDragHandler는 유니티가 제공하는 드래그 전용 마법 주문서입니다.
public class TileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("타일 데이터")]
    public TileData myTileData;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    // 드래그를 취소했을 때 원래 자리로 돌아가기 위해 기억해두는 변수들
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 1. 마우스로 타일을 꽉! 쥐었을 때 (드래그 시작)
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 돌아갈 원래 자리와 부모(HandArea)를 기억합니다.
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // 드래그하는 동안 다른 타일들에 가려지지 않도록 임시로 캔버스(가장 바깥쪽)로 끄집어냅니다.
        transform.SetParent(transform.root);
        transform.SetAsLastSibling(); // 화면의 맨 앞으로 가져오기

        // 타일이 마우스를 가리지 않도록 '유령(터치 무시)' 상태로 만듭니다. 그래야 아래에 있는 슬롯을 감지합니다.
        canvasGroup.blocksRaycasts = false;
    }

    // 2. 마우스를 쥐고 이리저리 움직일 때 (드래그 중)
    public void OnDrag(PointerEventData eventData)
    {
        // 타일의 위치가 마우스 커서 위치를 그대로 따라가게 합니다.
        transform.position = eventData.position;
    }

    // 3. 마우스에서 손을 뗐을 때 (드래그 끝)
    public void OnEndDrag(PointerEventData eventData)
    {
        // 다시 클릭할 수 있도록 유령 상태를 풉니다.
        canvasGroup.blocksRaycasts = true;

        // 만약 슬롯에 제대로 들어가지 못했다면 (부모가 여전히 캔버스라면), 원래 자리(HandArea)로 쫓아냅니다.
        if (transform.parent == transform.root)
        {
            ReturnToHand();
        }
    }

    // 원래 자리(손패)로 되돌려보내는 함수
    public void ReturnToHand()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}