using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("타일 데이터")]
    public TileData myTileData;

    // 🚨 [핵심 추가] 보드판에 배치되었는지 확인하는 자물쇠 변수입니다.
    public bool isPlaced = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 🚨 자물쇠가 잠겼다면(배치 완료) 마우스 클릭을 무시하고 드래그를 막습니다!
        if (isPlaced) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 🚨 이미 배치된 타일은 마우스를 따라다니지 않습니다.
        if (isPlaced) return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 🚨 [완벽 봉인] 슬롯에 안착했다면, 스크립트 전원을 끄고 마우스 클릭을 영구 무시합니다!
        if (isPlaced)
        {
            canvasGroup.blocksRaycasts = false; // 이제 마우스를 갖다 대도 유령처럼 통과합니다.
            this.enabled = false;               // 드래그 기능 자체를 아예 꺼버립니다.
            return;
        }

        // 배치되지 않은 타일만 다시 클릭할 수 있게 살려둡니다.
        canvasGroup.blocksRaycasts = true;

        // 슬롯에 제대로 못 들어갔을 때 원래 자리로 돌아가기
        if (transform.parent == transform.root)
        {
            ReturnToHand();
        }
    }

    public void ReturnToHand()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}