using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Gimmick", menuName = "BingoGame/Gimmick Data")]
public class GimmickData : ScriptableObject
{
    public enum GimmickType
    {
        Shuffle,
        DeleteTile,
        LockSlot,
        ExpandGrid
    }

    [Header("기믹 기본 설정")]
    public string gimmickName;
    [TextArea] public string description;
    public GimmickType type;

    [Header("효과 수치")]
    [Tooltip("파괴할 개수, 잠글 개수, 또는 확장할 그리드의 N x N 사이즈")]
    public int value = 1;

    [Tooltip("0 = 즉시 종료 / 1 이상 = 지정된 턴 수만큼 지속")]
    public int duration = 0;

    public void Execute()
    {
        switch (type)
        {
            case GimmickType.Shuffle:
                GridManager.Instance.ShuffleBoard();
                break;

            case GimmickType.DeleteTile:
                GridManager.Instance.DeleteRandomTile(value);
                break;

            case GimmickType.LockSlot:
                // [수정 - ⑤-C] 빈 슬롯이 없을 때 다른 기믹(Shuffle)으로 교체 발동
                bool lockSuccess = TryLockRandomSlots(value, duration);
                if (!lockSuccess)
                {
                    Debug.Log("🔒 LockSlot 발동 실패: 빈 슬롯 없음 → Shuffle 기믹으로 대체 발동!");
                    GridManager.Instance.ShuffleBoard();
                }
                break;

            case GimmickType.ExpandGrid:
                // ⑧ 정방형(NxN)만 지원
                GridManager.Instance.ChangeGridSize(value, value);
                break;
        }
        Debug.Log($"😈 몬스터 기믹 발동! [{gimmickName}] (지속: {duration}턴)");
    }

    // [수정 - ⑤-C] 반환값 추가: 슬롯을 실제로 잠갔으면 true, 빈 슬롯이 없으면 false
    private bool TryLockRandomSlots(int count, int lockDuration)
    {
        GridManager grid = GridManager.Instance;
        List<TileSlot> emptySlots = new List<TileSlot>();

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                TileSlot slot = grid.slotGrid[x, y];
                if (grid.boardData[x, y] == null && !slot.isLocked)
                    emptySlots.Add(slot);
            }
        }

        // 빈 슬롯이 하나도 없으면 실패 반환
        if (emptySlots.Count == 0) return false;

        for (int i = 0; i < count; i++)
        {
            if (emptySlots.Count == 0) break;
            int ri = Random.Range(0, emptySlots.Count);
            emptySlots[ri].SetLock(lockDuration);
            emptySlots.RemoveAt(ri);
        }
        return true;
    }
}