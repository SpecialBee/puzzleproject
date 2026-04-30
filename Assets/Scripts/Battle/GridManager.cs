using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // [수정 - S-01] 싱글톤 중복 체크 추가
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
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
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
                Vector2 pos = new Vector2((x * spacing) - offsetX, (-y * spacing) + offsetY);
                GameObject spawnedSlot = Instantiate(tileSlotPrefab, gridParent);
                spawnedSlot.GetComponent<RectTransform>().anchoredPosition = pos;
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

        // 타일을 내려놓는 순간 기본 스탯 즉시 적용
        // [주석 - ②] 이 시점에서 1회, 빙고 성립 시 ClearBingoTiles에서 추가 1회 적용됩니다.
        //            즉 빙고 라인의 타일은 statValue가 총 2회 반영됩니다.
        //            의도된 설계인지 추후 확정 후 배율을 조정하세요.
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.AddStat(data.tileType, data.statValue);

        CheckBingo(x, y);
    }

    // ── 빙고 판정 (같은 타입만 빙고) ──────────────────────────
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
            { isRowBingo = false; break; }
        }
        if (isRowBingo)
            for (int x = 0; x < width; x++) tilesToDestroy.Add(new Vector2Int(x, targetY));

        // 2. 세로줄 검사
        bool isColBingo = true;
        for (int y = 0; y < height; y++)
        {
            if (boardData[targetX, y] == null || boardData[targetX, y].tileType != targetType)
            { isColBingo = false; break; }
        }
        if (isColBingo)
            for (int y = 0; y < height; y++) tilesToDestroy.Add(new Vector2Int(targetX, y));

        // [수정 - S-03] 대각선 판정을 diagSize(min) 기반으로 수정
        // ⑦ 직사각형 보드에서 대각선 빙고 미허용 – 정방형(width==height)일 때만 대각선 판정
        if (width == height)
        {
            int diagSize = width; // 정방형이므로 width == height

            // 3. ↘ 방향 대각선
            if (targetX == targetY)
            {
                bool isDiag1Bingo = true;
                for (int i = 0; i < diagSize; i++)
                {
                    if (boardData[i, i] == null || boardData[i, i].tileType != targetType)
                    { isDiag1Bingo = false; break; }
                }
                if (isDiag1Bingo)
                    for (int i = 0; i < diagSize; i++) tilesToDestroy.Add(new Vector2Int(i, i));
            }

            // 4. ↙ 방향 대각선
            if (targetX + targetY == diagSize - 1)
            {
                bool isDiag2Bingo = true;
                for (int i = 0; i < diagSize; i++)
                {
                    if (boardData[i, diagSize - 1 - i] == null || boardData[i, diagSize - 1 - i].tileType != targetType)
                    { isDiag2Bingo = false; break; }
                }
                if (isDiag2Bingo)
                    for (int i = 0; i < diagSize; i++) tilesToDestroy.Add(new Vector2Int(i, diagSize - 1 - i));
            }
        }

        if (tilesToDestroy.Count > 0)
            StartCoroutine(ClearBingoTiles(tilesToDestroy));
        else
            CheckBoardFull();
    }

    // ── 빙고 타일 파괴 및 보너스 지급 ────────────────────────
    IEnumerator ClearBingoTiles(List<Vector2Int> tiles)
    {
        yield return new WaitForSeconds(0.3f);

        int bonusAttack = 0;
        int bonusDefense = 0;
        int bonusMana = 0;

        foreach (Vector2Int pos in tiles)
        {
            if (boardData[pos.x, pos.y] != null)
            {
                TileData data = boardData[pos.x, pos.y];

                // [주석 - ②] 여기서 추가 보너스를 지급합니다 (배치 시 1회 + 여기서 1회 = 총 2회)
                if (data.tileType == TileData.TileType.Attack) bonusAttack += data.statValue;
                else if (data.tileType == TileData.TileType.Defense) bonusDefense += data.statValue;
                else if (data.tileType == TileData.TileType.Mana) bonusMana += data.statValue;

                TileSlot slot = slotGrid[pos.x, pos.y];
                if (slot.transform.childCount > 0)
                    Destroy(slot.transform.GetChild(0).gameObject);

                boardData[pos.x, pos.y] = null;
            }
        }

        if (bonusAttack > 0) PlayerManager.Instance.AddStat(TileData.TileType.Attack, bonusAttack);
        if (bonusDefense > 0) PlayerManager.Instance.AddStat(TileData.TileType.Defense, bonusDefense);
        if (bonusMana > 0) PlayerManager.Instance.AddStat(TileData.TileType.Mana, bonusMana);

        yield return new WaitForSeconds(0.1f);
        CheckBoardFull();
    }

    // ── 보드 전체 초기화 (스테이지 전환 시) ──────────────────
    public void ClearAllTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                boardData[x, y] = null;
                TileSlot slot = slotGrid[x, y];
                if (slot.transform.childCount > 0)
                    Destroy(slot.transform.GetChild(0).gameObject);
                slot.SetLock(0);
            }
        }
    }

    // ── 보드 가득 참 체크 (게임오버) ──────────────────────────
    private void CheckBoardFull()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (boardData[x, y] == null) return;

        Debug.Log("보드판이 가득 찼습니다! 게임 오버.");
        if (PlayerManager.Instance != null && PlayerManager.Instance.gameOverPanel != null)
            PlayerManager.Instance.gameOverPanel.SetActive(true);
    }

    // ── 기믹: 보드 셔플 ───────────────────────────────────────
    public void ShuffleBoard()
    {
        List<TileData> collectedData = new List<TileData>();
        List<GameObject> collectedObjects = new List<GameObject>();
        List<TileSlot> availableSlots = new List<TileSlot>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (boardData[x, y] != null)
                {
                    if (slotGrid[x, y].transform.childCount > 0)
                    {
                        collectedData.Add(boardData[x, y]);
                        collectedObjects.Add(slotGrid[x, y].transform.GetChild(0).gameObject);
                    }
                    boardData[x, y] = null;
                }
                if (!slotGrid[x, y].isLocked)
                    availableSlots.Add(slotGrid[x, y]);
            }
        }

        // Fisher-Yates 셔플
        for (int i = 0; i < availableSlots.Count; i++)
        {
            int r = Random.Range(i, availableSlots.Count);
            (availableSlots[i], availableSlots[r]) = (availableSlots[r], availableSlots[i]);
        }

        for (int i = 0; i < collectedData.Count; i++)
        {
            TileSlot targetSlot = availableSlots[i];
            boardData[targetSlot.gridX, targetSlot.gridY] = collectedData[i];
            GameObject tileObj = collectedObjects[i];
            tileObj.transform.SetParent(targetSlot.transform);
            tileObj.transform.localPosition = Vector3.zero;
        }

        Debug.Log("🌪️ 기믹 발동: 보드판 타일이 섞였습니다!");
    }

    // ── 기믹: 무작위 타일 파괴 ───────────────────────────────
    public void DeleteRandomTile(int count)
    {
        List<Vector2Int> occupied = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (boardData[x, y] != null) occupied.Add(new Vector2Int(x, y));

        int destroyed = 0;
        for (int i = 0; i < count; i++)
        {
            if (occupied.Count == 0) break;
            int ri = Random.Range(0, occupied.Count);
            Vector2Int target = occupied[ri];
            boardData[target.x, target.y] = null;
            if (slotGrid[target.x, target.y].transform.childCount > 0)
                Destroy(slotGrid[target.x, target.y].transform.GetChild(0).gameObject);
            occupied.RemoveAt(ri);
            destroyed++;
        }
        Debug.Log($"💥 기믹 발동: 타일 {destroyed}개 파괴!");
    }

    // ── 턴 종료 시 슬롯 잠금 턴수 차감 ──────────────────────
    public void ProcessTurnEndForGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                slotGrid[x, y].PassTurn();
    }

    // ── 기믹: 보드 크기 변경 (대격변) ────────────────────────
    // [수정 - S-02/④] 축소 시 범위 밖 타일 스탯 역산 후 이사 처리
    public void ChangeGridSize(int newWidth, int newHeight)
    {
        TileData[,] oldBoard = boardData;
        int oldWidth = width;
        int oldHeight = height;

        // [수정 - S-02/④] 새 범위 밖 타일의 스탯을 역산
        if (PlayerManager.Instance != null)
        {
            for (int x = 0; x < oldWidth; x++)
            {
                for (int y = 0; y < oldHeight; y++)
                {
                    if (oldBoard[x, y] != null && (x >= newWidth || y >= newHeight))
                    {
                        PlayerManager.Instance.RemoveStat(oldBoard[x, y].tileType, oldBoard[x, y].statValue);
                        Debug.Log($"📉 크기 변경으로 [{x},{y}] 타일 스탯 역산");
                    }
                }
            }
        }

        // 기존 슬롯 오브젝트 전체 파괴
        int childCount = gridParent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
        gridParent.DetachChildren();

        width = newWidth;
        height = newHeight;
        boardData = new TileData[width, height];
        slotGrid = new TileSlot[width, height];

        GenerateGrid();

        // 범위 내 타일 이사 (UI 재생성 + 잠금 처리)
        for (int x = 0; x < oldWidth; x++)
        {
            for (int y = 0; y < oldHeight; y++)
            {
                if (oldBoard[x, y] != null && x < width && y < height)
                {
                    TileData data = oldBoard[x, y];
                    boardData[x, y] = data;

                    GameObject newTile = Instantiate(data.tilePrefab, slotGrid[x, y].transform);
                    RectTransform rt = newTile.GetComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.zero;
                    rt.localPosition = Vector3.zero;

                    TileDrag tileDrag = newTile.GetComponent<TileDrag>();
                    if (tileDrag != null)
                    {
                        tileDrag.myTileData = data;
                        tileDrag.isPlaced = true;
                        tileDrag.enabled = false;
                        newTile.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    }
                }
            }
        }

        Debug.Log($"🌋 기믹 발동: 보드판이 {width}x{height}로 변경되었습니다!");
    }

    // [수정 - D-04] 테스트용 키보드 입력 – 에디터 전용으로 격리
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) ShuffleBoard();
        if (Input.GetKeyDown(KeyCode.D)) DeleteRandomTile(1);
        if (Input.GetKeyDown(KeyCode.L)) slotGrid[Random.Range(0, width), Random.Range(0, height)].SetLock(2);
        if (Input.GetKeyDown(KeyCode.E)) ChangeGridSize(5, 5);
        if (Input.GetKeyDown(KeyCode.R)) ChangeGridSize(4, 4);
    }
#endif
}