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
            if (lockTurnsLeft == 0) Debug.Log($"슬롯 ({gridX},{gridY})의 잠금이 해제되었습니다!");
        }
    }

    private void UpdateVisuals()
    {
        if (slotImage != null)
        {
            slotImage.color = isLocked ? new Color(0.3f, 0f, 0f, 0.8f) : defaultColor;
        }
    }

    // [추가된 부분] 마우스로 이 슬롯을 클릭했을 때 실행됩니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 현재 스킬 매니저가 타겟팅을 기다리고 있는 상태라면?
        if (SkillManager.Instance != null && SkillManager.Instance.currentTargetingSkill != null)
        {
            SkillData activeSkill = SkillManager.Instance.currentTargetingSkill;

            // 마나를 먼저 소모한 뒤 (클릭에 성공했을 때만 마나가 닳도록)
            if (PlayerManager.Instance.UseMana(activeSkill.manaCost))
            {
                // 타겟팅 스킬 실행! (나 자신을 타겟으로 넘겨줍니다)
                SkillManager.Instance.ExecuteSkill(activeSkill, this);
            }
        }
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;
        if (transform.childCount > 0) return;

        if (eventData.pointerDrag != null)
        {
            GameObject droppedTile = eventData.pointerDrag;
            TileDrag tileDrag = droppedTile.GetComponent<TileDrag>();

            if (tileDrag != null)
            {
                // 🚨 [추가된 부분] 타일이 슬롯에 들어오는 순간, 자물쇠를 걸어버립니다!
                tileDrag.isPlaced = true;

                droppedTile.transform.SetParent(this.transform);
                RectTransform rt = droppedTile.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;

                GridManager.Instance.PlaceTile(gridX, gridY, tileDrag.myTileData);
            }
        }
    }
}