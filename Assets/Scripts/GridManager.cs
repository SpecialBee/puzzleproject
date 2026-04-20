using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("그리드 설정")]
    public int width = 4;
    public int height = 4;
    public float spacing = 120f;

    [Header("필수 연결 요소")]
    public GameObject tileSlotPrefab;
    public Transform gridParent;

    public TileData[,] boardData;
    public TileSlot[,] slotGrid;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        boardData = new TileData[width, height];
        slotGrid = new TileSlot[width, height];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPosition = new Vector2((x * spacing) - offsetX, (-y * spacing) + offsetY);
                GameObject spawnedSlot = Instantiate(tileSlotPrefab, gridParent);
                spawnedSlot.GetComponent<RectTransform>().anchoredPosition = spawnPosition;
                spawnedSlot.name = $"Slot_{x}_{y}";

                TileSlot slot = spawnedSlot.GetComponent<TileSlot>();
                if (slot != null)
                {
                    slot.gridX = x;
                    slot.gridY = y;
                    slotGrid[x, y] = slot;
                }
            }
        }
    }

    public void PlaceTile(int x, int y, TileData data)
    {
        boardData[x, y] = data;
        CheckBingo(x, y);
    }

    // --- 빙고 판정 로직 (같은 타입만 빙고) ---
    void CheckBingo(int targetX, int targetY)
    {
        TileData placedTile = boardData[targetX, targetY];
        if (placedTile == null) return;

        TileData.TileType targetType = placedTile.tileType;
        List<Vector2Int> tilesToDestroy = new List<Vector2Int>();

        // 1. 가로줄 검사
        bool isRowBingo = true;
        for (int x = 0; x < width; x++)
        {
            if (boardData[x, targetY] == null || boardData[x, targetY].tileType != targetType)
            {
                isRowBingo = false;
                break;
            }
        }
        if (isRowBingo) for (int x = 0; x < width; x++) tilesToDestroy.Add(new Vector2Int(x, targetY));

        // 2. 세로줄 검사
        bool isColBingo = true;
        for (int y = 0; y < height; y++)
        {
            if (boardData[targetX, y] == null || boardData[targetX, y].tileType != targetType)
            {
                isColBingo = false;
                break;
            }
        }
        if (isColBingo) for (int y = 0; y < height; y++) tilesToDestroy.Add(new Vector2Int(targetX, y));

        // 3. ↘ 방향 대각선 검사
        if (targetX == targetY)
        {
            bool isDiag1Bingo = true;
            for (int i = 0; i < width; i++)
            {
                if (boardData[i, i] == null || boardData[i, i].tileType != targetType)
                {
                    isDiag1Bingo = false;
                    break;
                }
            }
            if (isDiag1Bingo) for (int i = 0; i < width; i++) tilesToDestroy.Add(new Vector2Int(i, i));
        }

        // 4. ↙ 방향 대각선 검사
        if (targetX + targetY == width - 1)
        {
            bool isDiag2Bingo = true;
            for (int i = 0; i < width; i++)
            {
                if (boardData[i, width - 1 - i] == null || boardData[i, width - 1 - i].tileType != targetType)
                {
                    isDiag2Bingo = false;
                    break;
                }
            }
            if (isDiag2Bingo) for (int i = 0; i < width; i++) tilesToDestroy.Add(new Vector2Int(i, width - 1 - i));
        }

        if (tilesToDestroy.Count > 0)
        {
            StartCoroutine(ClearBingoTiles(tilesToDestroy));
        }
        else
        {
            // 빙고가 터지지 않았을 때도 혹시 보드가 가득 찼는지 검사합니다!
            CheckBoardFull();
        }
    }

    // --- 빙고 타일 파괴 및 보너스 지급 (통합됨) ---
    IEnumerator ClearBingoTiles(List<Vector2Int> tiles)
    {
        yield return new WaitForSeconds(0.3f);

        int bonusAttack = 0;
        int bonusDefense = 0;
        int bonusMana = 0;

        foreach (Vector2Int pos in tiles)
        {
            int px = pos.x;
            int py = pos.y;

            if (boardData[px, py] != null)
            {
                TileData data = boardData[px, py];

                if (data.tileType == TileData.TileType.Attack) bonusAttack += data.statValue;
                else if (data.tileType == TileData.TileType.Defense) bonusDefense += data.statValue;
                else if (data.tileType == TileData.TileType.Mana) bonusMana += data.statValue;

                TileSlot slot = slotGrid[px, py];
                if (slot.transform.childCount > 0)
                {
                    Destroy(slot.transform.GetChild(0).gameObject);
                }

                boardData[px, py] = null;
            }
        }

        if (bonusAttack > 0) PlayerManager.Instance.AddStat(TileData.TileType.Attack, bonusAttack);
        if (bonusDefense > 0) PlayerManager.Instance.AddStat(TileData.TileType.Defense, bonusDefense);
        if (bonusMana > 0) PlayerManager.Instance.AddStat(TileData.TileType.Mana, bonusMana);

        yield return new WaitForSeconds(0.1f);
        CheckBoardFull(); // 파괴 후 남은 칸이 있는지 체크
    }

    // --- 다음 스테이지 준비용 보드판 청소 ---
    public void ClearAllTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                boardData[x, y] = null;
                TileSlot slot = slotGrid[x, y];
                if (slot.transform.childCount > 0)
                {
                    Destroy(slot.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    // --- 보드가 가득 찼는지(게임오버) 체크 ---
    private void CheckBoardFull()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (boardData[x, y] == null) return; // 빈 칸이 하나라도 있으면 안전함!
            }
        }

        Debug.Log("보드판이 가득 찼습니다! 게임 오버.");
        // PlayerManager에 있는 GameOver(또는 TakeDamage로 죽는 로직) 호출
        if (PlayerManager.Instance.gameOverPanel != null)
        {
            PlayerManager.Instance.gameOverPanel.SetActive(true);
        }
    }
}