using UnityEngine;
using TMPro; // 글자를 다루기 위해 필요한 마법 주문입니다.

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("플레이어 현재 스탯")]
    public int hp = 100;
    public int attack = 0;
    public int defense = 0;
    public int mana = 0;

    [Header("UI 연결 (TextMeshPro)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI manaText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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
        if (hpText != null) hpText.text = $"HP : {hp}";
        if (attackText != null) attackText.text = $"ATTACK : {attack}";
        if (defenseText != null) defenseText.text = $"SHIELD : {defense}";
        if (manaText != null) manaText.text = $"MANA : {mana}";
    }

    // [추가할 함수 1] 몬스터에게 맞았을 때 데미지를 계산하는 함수
    public void TakeDamage(int enemyAttack)
    {
        // 1. 방어력으로 데미지를 먼저 깎아냅니다.
        int actualDamage = enemyAttack - defense;

        // 2. 방어력이 적의 공격력보다 높으면 데미지는 0이 됩니다.
        if (actualDamage < 0) actualDamage = 0;

        // 3. 실제 데미지만큼 플레이어의 체력을 깎습니다.
        hp -= actualDamage;
        if (hp < 0) hp = 0;

        UpdateUI();
        Debug.Log($"[전투] 플레이어가 {actualDamage}의 데미지를 입었습니다! (적 공격력:{enemyAttack} - 내 방어력:{defense}) 남은 체력: {hp}");

        if (hp == 0)
        {
            Debug.Log("💀 플레이어 사망... (패배 로직 발동 예정)");
        }
    }

    // [추가할 함수 2] 턴이 끝났을 때 스탯을 0으로 되돌리는 함수
    public void ResetTurnStats()
    {
        attack = 0;
        defense = 0;
        // 마나는 스킬을 모아서 쓰기 위해 초기화하지 않고 남겨둡니다!

        UpdateUI();
    }
}