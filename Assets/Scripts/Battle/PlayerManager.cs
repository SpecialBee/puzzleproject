using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // 글자를 다루기 위해 필요한 마법 주문입니다.

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("플레이어 스탯")]
    public int maxHp = 100; // 보상 한도치 및 연동을 위해 추가
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
    public GameObject victoryPanel;  // 승리 시 띄울 팝업창 (아이템 3개 포함)
    public GameObject gameOverPanel; // 죽었을 때 띄울 팝업창

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // [추가됨] 전투 시작 시, DataManager 금고에서 내 실제 스탯을 꺼내옵니다!
        if (DataManager.Instance != null)
        {
            hp = DataManager.Instance.currentHp;
            maxHp = DataManager.Instance.maxHp;
            attack = DataManager.Instance.baseAttack; // 보상으로 얻은 기본 공격력
            mana = DataManager.Instance.currentMana;
        }
        else
        {
            // 씬을 거치지 않고 바로 전투 씬을 단독 테스트할 때를 위한 기본값
            hp = maxHp;
        }

        UpdateUI(); // 게임 시작 시 글자를 한 번 새로고침 합니다.
    }

    // 타일이 놓이거나 빙고가 터졌을 때 불려갈 함수입니다.
    public void AddStat(TileData.TileType type, int value)
    {
        switch (type)
        {
            case TileData.TileType.Attack:
                attack += value;
                break;
            case TileData.TileType.Defense:
                defense += value;
                break;
            case TileData.TileType.Mana:
                mana += value;
                break;
        }

        UpdateUI(); // 스탯이 올랐으니 화면의 글자도 바로바로 바꿔줍니다.
        Debug.Log($"스탯 상승! 현재 공격력:{attack}, 방어력:{defense}, 마나:{mana}");
    }

    // 화면의 텍스트를 최신 상태로 바꿔주는 함수
    public void UpdateUI()
    {
        if (hpText != null) hpText.text = $"HP : {hp} / {maxHp}";
        if (attackText != null) attackText.text = $"ATTACK : {attack}";
        if (defenseText != null) defenseText.text = $"SHIELD : {defense}";
        if (manaText != null) manaText.text = $"MANA : {mana}";
    }

    // 몬스터에게 맞았을 때 데미지를 계산하는 함수 (기존 방어력 로직 + 새 사망 로직)
    public void TakeDamage(int enemyAttack)
    {
        // 1. 방어력으로 데미지를 먼저 깎아냅니다.
        int actualDamage = enemyAttack - defense;

        // 2. 방어력이 적의 공격력보다 높으면 데미지는 0이 됩니다.
        if (actualDamage < 0) actualDamage = 0;

        // 3. 실제 데미지만큼 플레이어의 체력을 깎습니다.
        hp -= actualDamage;
        if (hp <= 0)
        {
            hp = 0;
            GameOver(); // 체력이 0이 되면 게임 오버 실행!
        }

        UpdateUI();
        Debug.Log($"[전투] 플레이어가 {actualDamage}의 데미지를 입었습니다! (적 공격력:{enemyAttack} - 내 방어력:{defense}) 남은 체력: {hp}");
    }

    // 턴이 끝났을 때 스탯을 기본치로 되돌리는 함수
    public void ResetTurnStats()
    {
        // 영구 공격력(baseAttack)을 얻었다면 0이 아니라 해당 수치로 돌아가야 합니다.
        if (DataManager.Instance != null) attack = DataManager.Instance.baseAttack;
        else attack = 0;

        defense = 0;
        // 마나는 스킬을 모아서 쓰기 위해 초기화하지 않고 남겨둡니다.

        UpdateUI();
    }

    // ----------------------------------------------------
    // 게임 오버 및 씬 이동 관련 함수들
    // ----------------------------------------------------
    private void GameOver()
    {
        Debug.Log("💀 플레이어 사망... 게임 오버!");

        // 게임 오버 패널 띄우기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // [패널 버튼용] 게임 오버 패널의 '메인 메뉴로' 버튼에 연결할 함수
    public void OnClickReturnToMainMenu()
    {
        // 1. 얄짤없이 DataManager의 데이터를 초기화합니다.
        if (DataManager.Instance != null)
        {
            DataManager.Instance.ResetDataForNewRun();
        }

        // 2. 메인 메뉴 씬으로 쫓아냅니다.
        SceneManager.LoadScene("MainMenu");
    }

    // [패널 버튼용] 전투 승리 후 보상을 고르고 맵으로 돌아갈 때 호출할 함수
    public void SaveAndReturnToMap()
    {
        if (DataManager.Instance != null)
        {
            // 현재 내 스탯을 금고(DataManager)에 안전하게 저장합니다.
            DataManager.Instance.currentHp = hp;
            DataManager.Instance.maxHp = maxHp;
            DataManager.Instance.currentMana = mana;
            DataManager.Instance.baseAttack = attack; // 상승한 기본 공격력 저장

            // 스테이지 선택 씬으로 귀환!
            DataManager.Instance.LoadStageSelectScene();
        }
        else
        {
            Debug.LogWarning("DataManager가 없습니다! 씬 이동을 생략합니다.");
        }
    }

    // ----------------------------------------------------
    // [추가] 마나 스킬용 자원 소모 함수
    // ----------------------------------------------------
    public bool UseMana(int amount)
    {
        // 현재 마나가 충분하다면 마나를 깎고 true를 반환합니다.
        if (mana >= amount)
        {
            mana -= amount;
            UpdateUI(); // 변경된 마나 UI 갱신
            return true;
        }

        // 마나가 부족하면 튕겨냅니다.
        return false;
    }


    // 현재 씬 재시작 (테스트용)
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}