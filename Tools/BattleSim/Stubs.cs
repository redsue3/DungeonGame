// 전투 코드가 부르는 게임 쪽 관리자들의 대역. 진짜 DungeonManager/UIManager는 씬·UI에 묶여 있어 같이 컴파일하지 않는다.
using System.Collections.Generic;

public class DungeonManager
{
    public static DungeonManager Instance;

    public int CurrentLayer = 1;

    // 전투가 끝날 때 BattleManager.OnBattleEnd가 정확히 한 번 부르는지 확인하려고 센다
    public int    EndCalls;
    public string LastResult;

    public void OnBattleWon(List<Enemy> defeatedEnemies) { EndCalls++; LastResult = "Win"; }
    public void OnBattleFled(DungeonFloor battleFloor, List<Enemy> battleEnemies) { EndCalls++; LastResult = "Fled"; }
    public void OnBattleLost() { EndCalls++; LastResult = "Lose"; }
}

public class UIManager
{
    public static UIManager Instance;
    public void RefreshBattle() { }
}
