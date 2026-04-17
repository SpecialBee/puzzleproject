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
}