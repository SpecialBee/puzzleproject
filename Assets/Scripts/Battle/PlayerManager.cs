using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    // [수정 - S-01] 싱글톤 중복 체크 추가
    public static PlayerManager Instance;

    [Header("플레이어 스탯")]
    public int maxHp = 100;
    public int hp = 100;
    public int attack = 0;
    public int defense = 0;
    public int mana = 0;

    [Header("UI 텍스트 연결 (TextMeshPro)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI manaText;

    [Header("UI 패널 연결")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        // [수정 - S-01] 씬 재로드 시 인스턴스 중복 방지
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (DataManager.Instance != null)
        {
            hp = DataManager.Instance.currentHp;
            maxHp = DataManager.Instance.maxHp;
            attack = DataManager.Instance.baseAttack;
            defense = DataManager.Instance.baseDefense; // [추가 - ①] 영구 방어력 복원
            mana = DataManager.Instance.currentMana;
        }
        else
        {
            hp = maxHp;
        }

        UpdateUI();
    }

    // 타일 배치 또는 빙고 보너스 시 스탯 증가
    public void AddStat(TileData.TileType type, int value)
    {
        switch (type)
        {
            case TileData.TileType.Attack: attack += value; break;
            case TileData.TileType.Defense: defense += value; break;
            case TileData.TileType.Mana: mana += value; break;
        }
        UpdateUI();
        Debug.Log($"스탯 상승! 공격력:{attack} 방어력:{defense} 마나:{mana}");
    }

    // [추가 - S-02] 보드 축소 기믹 스탯 역산용. AddStat의 반대 함수.
    public void RemoveStat(TileData.TileType type, int value)
    {
        switch (type)
        {
            case TileData.TileType.Attack: attack = Mathf.Max(0, attack - value); break;
            case TileData.TileType.Defense: defense = Mathf.Max(0, defense - value); break;
            case TileData.TileType.Mana: mana = Mathf.Max(0, mana - value); break;
        }
        UpdateUI();
        Debug.Log($"스탯 역산! 공격력:{attack} 방어력:{defense} 마나:{mana}");
    }

    public void UpdateUI()
    {
        if (hpText != null) hpText.text = $"HP : {hp} / {maxHp}";
        if (attackText != null) attackText.text = $"ATTACK : {attack}";
        if (defenseText != null) defenseText.text = $"SHIELD : {defense}";
        if (manaText != null) manaText.text = $"MANA : {mana}";
    }

    public void TakeDamage(int enemyAttack)
    {
        int actualDamage = Mathf.Max(0, enemyAttack - defense);
        hp -= actualDamage;
        if (hp <= 0)
        {
            hp = 0;
            GameOver();
        }
        UpdateUI();
        Debug.Log($"[전투] 플레이어 피해 {actualDamage} (적:{enemyAttack} - 방어:{defense}) 잔여 HP:{hp}");
    }

    // 턴 종료 시 일회성 스탯 초기화
    public void ResetTurnStats()
    {
        // [수정 - ①] attack은 baseAttack으로, defense는 baseDefense로 복원
        if (DataManager.Instance != null)
        {
            attack = DataManager.Instance.baseAttack;
            defense = DataManager.Instance.baseDefense; // [추가 - ①] 영구 방어력 기준값으로 복원
        }
        else
        {
            // [수정 - D-05] DataManager 없을 때 현재값 유지 (0으로 날리지 않음)
            // attack, defense 그대로 유지
        }
        UpdateUI();
    }

    private void GameOver()
    {
        Debug.Log("💀 플레이어 사망... 게임 오버!");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void OnClickReturnToMainMenu()
    {
        if (DataManager.Instance != null) DataManager.Instance.ResetDataForNewRun();
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveAndReturnToMap()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentHp = hp;
            DataManager.Instance.maxHp = maxHp;
            DataManager.Instance.currentMana = mana;
            DataManager.Instance.baseAttack = attack;
            DataManager.Instance.baseDefense = defense; // [추가 - ①] 영구 방어력 저장
            DataManager.Instance.LoadStageSelectScene(); // [B-01] 내부에서 currentStage++ 처리
        }
        else
        {
            Debug.LogWarning("DataManager가 없습니다! 씬 이동을 생략합니다.");
        }
    }

    public bool UseMana(int amount)
    {
        if (mana >= amount)
        {
            mana -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}