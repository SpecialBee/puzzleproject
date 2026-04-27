using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TileSlot : MonoBehaviour, IDropHandler
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