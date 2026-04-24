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

        // ----------------------------------------------------
        // [복구된 핵심 로직] 타일을 내려놓는 순간 기본 스탯을 즉시 추가합니다!
        // ----------------------------------------------------
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.AddStat(data.tileType, data.statValue);
        }

        // 스탯을 더한 후 빙고가 터졌는지 검사합니다.
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
                // 스테이지가 넘어가면 잠금 상태도 모두 풀어줍니다.
                slot.SetLock(0);
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

    // ---------------------------------------------------------
    // [몬스터 기믹용 유틸리티 함수들]
    // ---------------------------------------------------------

    // 1. 현재 보드의 모든 타일 위치를 무작위로 섞습니다.
    public void ShuffleBoard()
    {
        List<TileData> collectedData = new List<TileData>();
        List<GameObject> collectedObjects = new List<GameObject>();
        List<TileSlot> availableSlots = new List<TileSlot>();

        // 1단계: 보드 위의 모든 타일 수거 및 빈칸 색출
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (boardData[x, y] != null)
                {
                    // 🚨 [안전장치 추가됨!] 슬롯에 진짜 UI 자식이 있는지 확인
                    if (slotGrid[x, y].transform.childCount > 0)
                    {
                        collectedData.Add(boardData[x, y]);
                        collectedObjects.Add(slotGrid[x, y].transform.GetChild(0).gameObject);
                    }
                    boardData[x, y] = null; // 자리 비우기
                }

                // 잠기지 않은 슬롯만 배치 가능 목록에 추가
                if (!slotGrid[x, y].isLocked)
                {
                    availableSlots.Add(slotGrid[x, y]);
                }
            }
        }

        // 2단계: 빈칸 리스트 섞기 (Fisher-Yates 알고리즘)
        for (int i = 0; i < availableSlots.Count; i++)
        {
            TileSlot temp = availableSlots[i];
            int randomIndex = Random.Range(i, availableSlots.Count);
            availableSlots[i] = availableSlots[randomIndex];
            availableSlots[randomIndex] = temp;
        }

        // 3단계: 수거한 타일들을 무작위 빈칸에 다시 꽂아 넣기
        for (int i = 0; i < collectedData.Count; i++)
        {
            TileSlot targetSlot = availableSlots[i];

            // 데이터 재배치
            boardData[targetSlot.gridX, targetSlot.gridY] = collectedData[i];

            // 시각적 오브젝트(UI) 재배치
            GameObject tileObj = collectedObjects[i];
            tileObj.transform.SetParent(targetSlot.transform);
            tileObj.transform.localPosition = Vector3.zero;
        }

        Debug.Log("🌪️ 기믹 발동: 보드판 타일이 섞였습니다!");
    }

    // 2. 무작위 타일을 지정한 개수(count)만큼 파괴합니다.
    public void DeleteRandomTile(int count)
    {
        List<Vector2Int> occupiedSlots = new List<Vector2Int>();

        // 현재 타일이 있는 좌표만 긁어모읍니다.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (boardData[x, y] != null) occupiedSlots.Add(new Vector2Int(x, y));
            }
        }

        int destroyedCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (occupiedSlots.Count == 0) break; // 더 지울 게 없으면 중단

            int randomIndex = Random.Range(0, occupiedSlots.Count);
            Vector2Int target = occupiedSlots[randomIndex];

            // 데이터 비우기
            boardData[target.x, target.y] = null;

            // 🚨 [안전장치 추가됨!] 슬롯에 진짜 UI 자식이 있을 때만 파괴
            if (slotGrid[target.x, target.y].transform.childCount > 0)
            {
                Destroy(slotGrid[target.x, target.y].transform.GetChild(0).gameObject);
            }

            occupiedSlots.RemoveAt(randomIndex);
            destroyedCount++;
        }

        Debug.Log($"💥 기믹 발동: 타일 {destroyedCount}개가 무작위로 파괴되었습니다!");
    }

    // [추가] 턴이 종료될 때 모든 슬롯의 상태(남은 잠금 턴 수 등)를 업데이트합니다.
    public void ProcessTurnEndForGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                slotGrid[x, y].PassTurn();
            }
        }
    }

    // ---------------------------------------------------------
    // [추가] 4. 보드판 크기 실시간 변경 (대격변 기믹)
    // ---------------------------------------------------------
    public void ChangeGridSize(int newWidth, int newHeight)
    {
        // 1. 기존 데이터 백업
        TileData[,] oldBoard = boardData;
        int oldWidth = width;
        int oldHeight = height;

        // 2. 화면에 있는 기존 슬롯들 싹 다 지우기
        // (DetachChildren을 먼저 해야 유니티 내부에서 UI가 꼬이지 않습니다)
        int childCount = gridParent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(gridParent.GetChild(i).gameObject);
        }
        gridParent.DetachChildren();

        // 3. 새로운 크기로 변수 갱신 및 배열 리셋
        width = newWidth;
        height = newHeight;
        boardData = new TileData[width, height];
        slotGrid = new TileSlot[width, height];

        // 4. 새 보드판 그리기 (기존 GenerateGrid 함수 재활용!)
        GenerateGrid();

        // 5. 백업해둔 타일들을 새 보드판으로 이사시키기
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

                    // --------------------------------------------------------
                    // 🚨 [수정된 부분] 데이터 재주입과 동시에 영구 잠금 처리!
                    // --------------------------------------------------------
                    TileDrag tileDrag = newTile.GetComponent<TileDrag>();
                    if (tileDrag != null)
                    {
                        tileDrag.myTileData = data;
                        tileDrag.isPlaced = true;
                        tileDrag.enabled = false; // 대격변 후에도 꼼짝 못하게 전원 오프!
                        newTile.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    }
                }
            }
        }

        Debug.Log($"🌋 기믹 발동: 보드판이 {width}x{height} 크기로 대격변했습니다!");
    }

    // 3. 테스트용 키보드 입력 (나중에 지울 겁니다!)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) ShuffleBoard();
        if (Input.GetKeyDown(KeyCode.D)) DeleteRandomTile(1);

        // [수정] true가 아니라 2(턴)를 넣어주어 에러를 방지합니다.
        if (Input.GetKeyDown(KeyCode.L)) slotGrid[Random.Range(0, width), Random.Range(0, height)].SetLock(2);

        // [추가] E키를 누르면 5x5로 확장! R키를 누르면 다시 4x4로 축소!
        if (Input.GetKeyDown(KeyCode.E)) ChangeGridSize(5, 5);
        if (Input.GetKeyDown(KeyCode.R)) ChangeGridSize(4, 4);
    }
}