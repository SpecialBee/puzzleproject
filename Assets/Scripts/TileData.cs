using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "BingoGame/Tile Data")]
public class TileData : ScriptableObject
{
    // 타일의 종류를 미리 정의해 둡니다.
    public enum TileType
    {
        Attack,  // 공격력 (빨강)
        Defense, // 방어력 (초록)
        Mana     // 마나 (파랑)
    }

    [Header("타일 기본 정보")] // 유니티 화면에서 보기 좋게 제목을 달아줍니다.

    [Tooltip("타일의 종류를 선택하세요.")]
    public TileType tileType;

    [Tooltip("이 타일이 부여할 수치(NN)를 입력하세요.")]
    public int statValue;

    [Tooltip("화면에 보여질 타일 이미지를 넣는 곳입니다.")]
    public Sprite tileSprite;
}