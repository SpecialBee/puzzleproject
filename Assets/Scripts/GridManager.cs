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
    void CheckBingo(int targetX, int targetY)
    {
        // 파괴해야 할 타일들의 좌표를 담아둘 리스트입니다.
        List<Vector2Int> tilesToDestroy = new List<Vector2Int>();

        // 1. 가로줄(Row) 검사: targetY 줄이 모두 찼는지 확인
        bool isRowBingo = true;
        for (int x = 0; x < width; x++)
        {
            if (boardData[x, targetY] == null) isRowBingo = false; // 하나라도 비어있으면 실패
        }
        if (isRowBingo)
        {
            Debug.Log($"[빙고!] 가로 {targetY}번째 줄 완성!");
            for (int x = 0; x < width; x++) tilesToDestroy.Add(new Vector2Int(x, targetY));
        }

        // 2. 세로줄(Column) 검사: targetX 줄이 모두 찼는지 확인
        bool isColBingo = true;
        for (int y = 0; y < height; y++)
        {
            if (boardData[targetX, y] == null) isColBingo = false;
        }
        if (isColBingo)
        {
            Debug.Log($"[빙고!] 세로 {targetX}번째 줄 완성!");
            for (int y = 0; y < height; y++) tilesToDestroy.Add(new Vector2Int(targetX, y));
        }

        // 3. ↘ 방향 대각선 검사 (x와 y가 같은 타일들)
        if (targetX == targetY)
        {
            bool isDiag1Bingo = true;
            for (int i = 0; i < width; i++)
            {
                if (boardData[i, i] == null) isDiag1Bingo = false;
            }
            if (isDiag1Bingo)
            {
                Debug.Log("[빙고!] ↘ 방향 대각선 완성!");
                for (int i = 0; i < width; i++) tilesToDestroy.Add(new Vector2Int(i, i));
            }
        }

        // 4. ↙ 방향 대각선 검사 (x + y 가 끝번호인 타일들)
        if (targetX + targetY == width - 1)
        {
            bool isDiag2Bingo = true;
            for (int i = 0; i < width; i++)
            {
                if (boardData[i, width - 1 - i] == null) isDiag2Bingo = false;
            }
            if (isDiag2Bingo)
            {
                Debug.Log("[빙고!] ↙ 방향 대각선 완성!");
                for (int i = 0; i < width; i++) tilesToDestroy.Add(new Vector2Int(i, width - 1 - i));
            }
        }

        // 빙고가 하나라도 터졌다면 파괴 연출을 시작합니다.
        if (tilesToDestroy.Count > 0)
        {
            StartCoroutine(ClearBingoTiles(tilesToDestroy));
        }
    }

    // 빙고된 타일들을 파괴하는 코루틴 (자연스러운 연출을 위해 약간의 딜레이를 줍니다)
    IEnumerator ClearBingoTiles(List<Vector2Int> tiles)
    {
        // 플레이어가 타일을 놓자마자 바로 사라지면 타격감이 없으므로 0.3초 대기합니다.
        yield return new WaitForSeconds(0.3f);

        foreach (Vector2Int pos in tiles)
        {
            int px = pos.x;
            int py = pos.y;

            // 중복 파괴를 방지하기 위해 데이터가 있는지 확인
            if (boardData[px, py] != null)
            {
                // [이곳에 버프 수치 적용 로직이 들어갈 예정입니다]

                // 1. 실제 화면의 UI 타일 파괴
                TileSlot slot = slotGrid[px, py];
                if (slot.transform.childCount > 0)
                {
                    Destroy(slot.transform.GetChild(0).gameObject); // 자식(UI 타일)을 삭제
                }

                // 2. 매니저의 두뇌(데이터) 비우기
                boardData[px, py] = null;
            }
        }
        Debug.Log("빙고 타일 파괴 및 빈칸 확보 완료!");
    }
}