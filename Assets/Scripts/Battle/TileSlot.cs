using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TileSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("슬롯 좌표")]
    public int gridX;
    public int gridY;

    [Header("기믹 상태")]
    public int lockTurnsLeft = 0;
    public bool isLocked => lockTurnsLeft > 0;

    private Image slotImage;
    private Color defaultColor;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null) defaultColor = slotImage.color;
    }

    public void SetLock(int duration)
    {
        lockTurnsLeft = duration;
        UpdateVisuals();
    }

    public void PassTurn()
    {
        if (lockTurnsLeft > 0)
        {
            lockTurnsLeft--;
            UpdateVisuals();
            if (lockTurnsLeft == 0)
                Debug.Log($"🔓 슬롯 ({gridX},{gridY}) 잠금 해제!");
        }
    }

    private void UpdateVisuals()
    {
        if (slotImage != null)
            slotImage.color = isLocked ? new Color(0.3f, 0f, 0f, 0.8f) : defaultColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SkillManager.Instance != null && SkillManager.Instance.currentTargetingSkill != null)
        {
            SkillData activeSkill = SkillManager.Instance.currentTargetingSkill;
            if (PlayerManager.Instance.UseMana(activeSkill.manaCost))
                SkillManager.Instance.ExecuteSkill(activeSkill, this);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;
        if (transform.childCount > 0) return;
        if (eventData.pointerDrag == null) return;

        TileDrag tileDrag = eventData.pointerDrag.GetComponent<TileDrag>();
        if (tileDrag == null) return;

        // [수정 - D-03] 슬롯에 안착했음을 TileDrag에 알림 (wasDropped = true)
        tileDrag.NotifyDropped();
        tileDrag.isPlaced = true;

        eventData.pointerDrag.transform.SetParent(this.transform);
        RectTransform rt = eventData.pointerDrag.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localPosition = Vector3.zero;

        GridManager.Instance.PlaceTile(gridX, gridY, tileDrag.myTileData);
    }
}