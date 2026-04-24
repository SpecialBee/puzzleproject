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
        ExpandGrid    // [추가됨] 그리드 확장 기믹
    }

    [Header("기믹 기본 설정")]
    public string gimmickName;
    [TextArea] public string description;
    public GimmickType type;

    [Header("효과 수치")]
    [Tooltip("파괴할 개수, 잠글 개수, 또는 확장할 그리드의 N x N 사이즈")]
    public int value = 1;

    [Tooltip("0 = 즉시 발동 후 종료 / 1 이상 = 지정된 턴 수만큼 지속")]
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
                LockRandomSlots(value, duration);
                break;
            case GimmickType.ExpandGrid: // [추가됨]
                GridManager.Instance.ChangeGridSize(value, value);
                break;
        }
        Debug.Log($"😈 몬스터 기믹 발동! [{gimmickName}] (지속: {duration}턴)");
    }

    private void LockRandomSlots(int count, int lockDuration)
    {
        GridManager grid = GridManager.Instance;
        List<TileSlot> emptySlots = new List<TileSlot>();

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                TileSlot slot = grid.slotGrid[x, y];
                if (grid.boardData[x, y] == null && !slot.isLocked)
                {
                    emptySlots.Add(slot);
                }
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (emptySlots.Count == 0) break;
            int randomIndex = Random.Range(0, emptySlots.Count);
            emptySlots[randomIndex].SetLock(lockDuration);
            emptySlots.RemoveAt(randomIndex);
        }
    }
}