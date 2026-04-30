using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    // [수정 - S-01] 싱글톤 중복 체크 추가
    public static HandManager Instance;

    [Header("타일 뽑기 설정")]
    public List<TileData> availableTiles;
    public Transform handArea;

    private bool isTurnProcessing = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        DrawTiles(5);
    }

    public void DrawTiles(int amount)
    {
        foreach (Transform child in handArea)
            Destroy(child.gameObject);

        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, availableTiles.Count);
            TileData selectedData = availableTiles[randomIndex];

            GameObject newTile = Instantiate(selectedData.tilePrefab, handArea);
            newTile.transform.localScale = Vector3.one;
            newTile.GetComponent<TileDrag>().myTileData = selectedData;
        }
    }

    public void OnEndTurnButtonClicked()
    {
        if (isTurnProcessing) return;

        // [수정 - ③] 손패에 타일이 남아있으면 턴 종료 불가
        if (handArea.childCount > 0)
        {
            Debug.LogWarning("⚠️ 모든 타일을 보드에 배치해야 턴을 마칠 수 있습니다!");
            return;
        }

        // [추가 - ③] 보드에도 배치된 타일이 하나도 없으면 턴 종료 불가
        // 손패를 전부 보드에 내려놓아야 하므로, 이 조건은 정상 플레이에서 항상 통과됩니다.
        // (손패 5장 → 보드 배치 → 빙고로 일부 파괴 → 보드에 타일 있음)
        // 단, 전부 빙고로 터진 경우는 예외적으로 허용합니다.
        bool hasAnyTileOnBoard = false;
        if (GridManager.Instance != null)
        {
            for (int x = 0; x < GridManager.Instance.width; x++)
            {
                for (int y = 0; y < GridManager.Instance.height; y++)
                {
                    if (GridManager.Instance.boardData[x, y] != null)
                    {
                        hasAnyTileOnBoard = true;
                        break;
                    }
                }
                if (hasAnyTileOnBoard) break;
            }
        }

        // 손패를 전혀 배치하지 않은 상태(보드도 비어있음)는 차단
        // 단, 보드에 이전 턴부터 남은 타일이 있을 수 있으므로
        // 이번 턴에 뽑은 손패를 단 한 장도 안 놓은 것인지를 명확히 하려면
        // 추후 '이번 턴 배치 횟수' 카운터를 추가하는 것을 권장합니다.
        // 현재는 보드가 비어있을 때만 차단합니다.
        if (!hasAnyTileOnBoard)
        {
            Debug.LogWarning("⚠️ 보드가 비어 있습니다. 타일을 배치한 후 턴을 종료하세요!");
            return;
        }

        StartCoroutine(TurnPhaseSequence());
    }

    private IEnumerator TurnPhaseSequence()
    {
        isTurnProcessing = true;

        // ── 페이즈 1: 플레이어 공격 ──
        Debug.Log("--- [페이즈 1: 플레이어 턴] ---");
        if (MonsterManager.Instance != null && PlayerManager.Instance != null)
            MonsterManager.Instance.TakeDamage(PlayerManager.Instance.attack);

        if (MonsterManager.Instance.currentHp <= 0)
        {
            isTurnProcessing = false;
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        // ── 페이즈 2: 몬스터 공격 ──
        Debug.Log("--- [페이즈 2: 몬스터 턴] ---");
        if (MonsterManager.Instance != null)
            MonsterManager.Instance.PerformMonsterTurn();

        if (PlayerManager.Instance.hp <= 0)
        {
            isTurnProcessing = false;
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        // ── 페이즈 3: 턴 결산 ──
        Debug.Log("--- [페이즈 3: 턴 결산] ---");

        // [수정 - B-05/⑥] SkipMonsterTurn 스킬 지원을 위한 플래그 처리 (SkillManager에 위임)
        PlayerManager.Instance.ResetTurnStats();
        GridManager.Instance.ProcessTurnEndForGrid();
        DrawTiles(5);

        isTurnProcessing = false;
    }
}