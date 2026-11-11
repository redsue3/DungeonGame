public enum TileType
{
    Empty,
    NormalEnemy,   // 일반 조우 (적 1마리)
    GroupEnemy,    // 집단 조우 (적 3마리, 각자 1/3 능력치라 합치면 일반몹 1마리 몫)
    EliteEnemy,    // 엘리트 조우
    Boss,          // 보스
    Rest,          // 휴식
    Shop,          // 상점
    Shrine,        // 성소 - 캐릭터 이벤트, 카드 제작
}
