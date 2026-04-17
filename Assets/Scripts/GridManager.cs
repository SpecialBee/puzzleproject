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

    // 데이터(TileData)와 실제 화면의 슬롯(TileSlot)을 모두 기억합니다.
    public TileData[,] boardData;
    public TileSlot[,] slotGrid; // [추가됨] 타일을 지우기 위해 슬롯의 위치도 기억해야 합니다.

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        boardData = new TileData[width, height];
        slotGrid = new TileSlot[width, height]; // 배열 초기화
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
                    slotGrid[x, y] = slot; // 매니저가 각 슬롯의 물리적 위치를 기억해둡니다.
                }
            }
        }
    }

    public void PlaceTile(int x, int y, TileData data)
    {
        boardData[x, y] = data;

        // 타일이 놓일 때마다 이 자리를 기준으로 빙고가 터졌는지 검사합니다!
        CheckBingo(x, y);
    }

    // --- 빙고 판정 핵심 로직 ---
    // --- 빙고 판정 핵심 로직 (수정됨: 같은 타입만 빙고 인정) ---
    void CheckBingo(int targetX, int targetY)
    {
        // 방금 놓은 타일의 정보를 가져와서 '기준 타입'으로 삼습니다.
        TileData placedTile = boardData[targetX, targetY];
        if (placedTile == null) return;

        TileData.TileType targetType = placedTile.tileType; // 예: Attack(빨강)

        List<Vector2Int> tilesToDestroy = new List<Vector2Int>();

        // 1. 가로줄(Row) 검사
        bool isRowBingo = true;
        for (int x = 0; x < width; x++)
        {
            // 칸이 비어있거나, 기준 타입과 다르면 빙고 실패!
            if (boardData[x, targetY] == null || boardData[x, targetY].tileType != targetType)
            {
                isRowBingo = false;
                break; // 더 이상 검사할 필요 없이 중단
            }
        }
        if (isRowBingo)
        {
            Debug.Log($"[빙고!] 가로 {targetY}번째 줄 {targetType} 타입 완성!");
            for (int x = 0; x < width; x++) tilesToDestroy.Add(new Vector2Int(x, targetY));
        }

        // 2. 세로줄(Column) 검사
        bool isColBingo = true;
        for (int y = 0; y < height; y++)
        {
            if (boardData[targetX, y] == null || boardData[targetX, y].tileType != targetType)
            {
                isColBingo = false;
                break;
            }
        }
        if (isColBingo)
        {
            Debug.Log($"[빙고!] 세로 {targetX}번째 줄 {targetType} 타입 완성!");
            for (int y = 0; y < height; y++) tilesToDestroy.Add(new Vector2Int(targetX, y));
        }

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
            if (isDiag1Bingo)
            {
                Debug.Log($"[빙고!] ↘ 방향 대각선 {targetType} 타입 완성!");
                for (int i = 0; i < width; i++) tilesToDestroy.Add(new Vector2Int(i, i));
            }
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
            if (isDiag2Bingo)
            {
                Debug.Log($"[빙고!] ↙ 방향 대각선 {targetType} 타입 완성!");
                for (int i = 0; i < width; i++) tilesToDestroy.Add(new Vector2Int(i, width - 1 - i));
            }
        }

        // 빙고가 하나라도 터졌다면 파괴 및 스탯 뻥튀기 연출 시작
        if (tilesToDestroy.Count > 0)
        {
            StartCoroutine(ClearBingoTiles(tilesToDestroy));
        }
    }

    // 빙고된 타일들을 파괴하는 코루틴 (자연스러운 연출을 위해 약간의 딜레이를 줍니다)
    // 빙고된 타일들을 파괴하고 보너스 스탯을 지급하는 코루틴
    IEnumerator ClearBingoTiles(List<Vector2Int> tiles)
    {
        yield return new WaitForSeconds(0.3f);

        // [추가된 부분] 빙고로 얻을 추가 스탯을 계산하기 위한 바구니
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

                // [추가된 부분] 파괴될 타일들의 스탯을 바구니에 차곡차곡 모읍니다.
                if (data.tileType == TileData.TileType.Attack) bonusAttack += data.statValue;
                else if (data.tileType == TileData.TileType.Defense) bonusDefense += data.statValue;
                else if (data.tileType == TileData.TileType.Mana) bonusMana += data.statValue;

                // 1. 실제 화면의 UI 타일 파괴
                TileSlot slot = slotGrid[px, py];
                if (slot.transform.childCount > 0)
                {
                    Destroy(slot.transform.GetChild(0).gameObject);
                }

                // 2. 매니저의 두뇌(데이터) 비우기
                boardData[px, py] = null;
            }
        }

        // [추가된 부분] 모인 보너스 스탯을 플레이어에게 한방에 쏴줍니다! (기존 스탯에 1배수 추가 = 2배 효율)
        if (bonusAttack > 0) PlayerManager.Instance.AddStat(TileData.TileType.Attack, bonusAttack);
        if (bonusDefense > 0) PlayerManager.Instance.AddStat(TileData.TileType.Defense, bonusDefense);
        if (bonusMana > 0) PlayerManager.Instance.AddStat(TileData.TileType.Mana, bonusMana);

        Debug.Log($"[빙고 잭팟!] 추가 스탯 획득 - 공격력:{bonusAttack}, 방어력:{bonusDefense}, 마나:{bonusMana}");
    }
}