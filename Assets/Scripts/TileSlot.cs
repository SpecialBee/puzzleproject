using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileSlot : MonoBehaviour, IDropHandler
{
    // [추가된 부분] GridManager가 부여해 줄 자신의 좌표
    public int gridX;
    public int gridY;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedTile = eventData.pointerDrag;

        if (droppedTile != null)
        {
            TileDrag draggableTile = droppedTile.GetComponent<TileDrag>();

            if (draggableTile != null && transform.childCount == 0)
            {
                droppedTile.transform.SetParent(transform);
                droppedTile.GetComponent<RectTransform>().localPosition = Vector3.zero;

                // [추가된 부분] 스냅이 성공적으로 끝났다면, 매니저(GridManager)에게 데이터를 보고합니다!
                GridManager.Instance.PlaceTile(gridX, gridY, draggableTile.myTileData);

                // [이 부분 추가!] 플레이어 매니저에게 내 속성과 수치를 전달해서 스탯을 올립니다.
                PlayerManager.Instance.AddStat(draggableTile.myTileData.tileType, draggableTile.myTileData.statValue);

                // (선택) 한 번 배치된 타일은 다시 집지 못하게 드래그 기능을 꺼버립니다.
                draggableTile.enabled = false;
            }
        }
    }
}