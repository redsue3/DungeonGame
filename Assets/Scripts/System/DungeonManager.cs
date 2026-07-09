using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { CharacterSelect, DungeonMap, Battle, Reward, Rest, Shop, Shrine, GameOver, Victory }

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    public GameState       CurrentState  { get; private set; }
    public PlayerCharacter Player        { get; private set; }
    public DungeonFloor    CurrentFloor  { get; private set; }
    public int             CurrentLayer  => currentLayer;

    private int            currentLayer = 1;
    private RoomInfo       currentRoom;   // 지금 상호작용 중인 방 (전투/휴식/상점/성소 공용)
    private BattleReward   pendingReward;
    private ShopSystem     shop = new ShopSystem();
    private bool           shopCardRemovePurchased = false;
    private bool           shopPurchasedThisVisit  = false;
    private ShrineOption[] pendingShrineOptions;

    void Awake() => Instance = this;

    public void StartRun(CharacterClass cls)
    {
        Player       = PlayerFactory.Create(cls);
        currentLayer = 1;
        CurrentFloor = FloorGenerator.Generate(currentLayer);
        Debug.Log($"[DungeonManager] 런 시작: {Player.characterName}  계층:{currentLayer}");
        TransitionTo(GameState.DungeonMap);
    }

    // 세이브 파일에서 이어하기 - CharacterSelectUI의 이어하기 버튼에서 호출
    // 맵 자체는 저장되지 않으므로(계층 번호만 저장) 해당 계층 던전을 새로 생성해서 입장 전 상태로 되돌린다.
    public bool LoadRun()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) return false;

        Player       = SaveSystem.RestorePlayer(data);
        currentLayer = data.currentLayer;
        CurrentFloor = FloorGenerator.Generate(currentLayer);
        Debug.Log($"[DungeonManager] 이어하기: {Player.characterName}  계층:{currentLayer}");
        TransitionTo(GameState.DungeonMap);
        return true;
    }

    // 플레이어 한 칸 이동 - 맵 UI에서 호출. 벽이면 무시, 바닥이면 이동 후 적 AI 한 스텝 + 조우 판정.
    public bool TryMove(int dx, int dy)
    {
        if (CurrentState != GameState.DungeonMap) return false;

        int nx = CurrentFloor.PlayerX + dx;
        int ny = CurrentFloor.PlayerY + dy;
        if (!CurrentFloor.IsWalkable(nx, ny)) return false;

        CurrentFloor.PlayerX = nx;
        CurrentFloor.PlayerY = ny;
        CurrentFloor.RevealAround(nx, ny, FloorGenerator.VisionRadius);

        HungerSystem.OnPlayerMove(Player);
        if (!Player.IsAlive)
        {
            TransitionTo(GameState.GameOver);
            return true;
        }

        if (TryEngageEnemyAt(nx, ny)) return true;

        StepEnemies();
        if (TryEngageEnemyAt(CurrentFloor.PlayerX, CurrentFloor.PlayerY)) return true;

        // 휴식/상점/성소는 아이콘이 표시되는 방 중심 타일에 직접 접촉했을 때만 발동 (방 전체 범위 아님)
        RoomInfo room = CurrentFloor.RoomAt(nx, ny);
        if (room != null && !room.isCleared && nx == room.CenterX && ny == room.CenterY)
        {
            if (room.roomType == TileType.Rest)
            {
                currentRoom = room;
                TransitionTo(GameState.Rest);
                return true;
            }
            if (room.roomType == TileType.Shop)
            {
                currentRoom = room;
                shop.Generate(currentLayer, Player);
                shopCardRemovePurchased = false;
                shopPurchasedThisVisit  = false;
                TransitionTo(GameState.Shop);
                return true;
            }
            if (room.roomType == TileType.Shrine)
            {
                currentRoom = room;
                pendingShrineOptions = ShrineSystem.GenerateOptions(Player);
                TransitionTo(GameState.Shrine);
                return true;
            }
        }

        return true;
    }

    private bool TryEngageEnemyAt(int x, int y)
    {
        EnemySpawn spawn = CurrentFloor.EnemyAt(x, y);
        if (spawn == null) return false;

        currentRoom = CurrentFloor.Rooms.FirstOrDefault(r => r.id == spawn.roomId);

        var enemyList = new List<Enemy>();
        foreach (EnemySpawn s in CurrentFloor.AliveEnemiesInRoom(spawn.roomId))
        {
            Enemy e = EnemyFactory.Create(s.enemyTemplateId);
            if (e != null) enemyList.Add(e);
        }

        BattleManager.Instance.StartBattle(Player, enemyList);
        TransitionTo(GameState.Battle);
        return true;
    }

    // 감지 반경 안에 들어온 적을 Chasing으로 전환하고, Chasing 상태인 적을 플레이어 쪽으로 한 칸씩 옮긴다.
    private void StepEnemies()
    {
        foreach (EnemySpawn e in CurrentFloor.Enemies)
        {
            if (e.isDead) continue;

            int dist2 = (e.x - CurrentFloor.PlayerX) * (e.x - CurrentFloor.PlayerX) +
                        (e.y - CurrentFloor.PlayerY) * (e.y - CurrentFloor.PlayerY);
            if (e.state == EnemyAiState.Idle && dist2 <= FloorGenerator.EnemyDetectRadius * FloorGenerator.EnemyDetectRadius)
                e.state = EnemyAiState.Chasing;

            if (e.state != EnemyAiState.Chasing) continue;

            StepTowardPlayer(e);
        }
    }

    private void StepTowardPlayer(EnemySpawn e)
    {
        int dx = System.Math.Sign(CurrentFloor.PlayerX - e.x);
        int dy = System.Math.Sign(CurrentFloor.PlayerY - e.y);

        if (TryStepEnemy(e, dx, dy)) return;
        if (dx != 0 && TryStepEnemy(e, dx, 0)) return;
        if (dy != 0 && TryStepEnemy(e, 0, dy)) return;
    }

    private bool TryStepEnemy(EnemySpawn e, int dx, int dy)
    {
        if (dx == 0 && dy == 0) return false;
        int nx = e.x + dx, ny = e.y + dy;
        if (!CurrentFloor.IsWalkable(nx, ny)) return false;
        if (CurrentFloor.EnemyAt(nx, ny) != null) return false;
        if (nx == CurrentFloor.PlayerX && ny == CurrentFloor.PlayerY) return false; // 실제 이동 대신 TryEngage에서 처리

        e.x = nx;
        e.y = ny;
        return true;
    }

    // BattleManager에서 호출
    public void OnBattleWon()
    {
        if (currentRoom != null)
        {
            currentRoom.isCleared = true;
            foreach (EnemySpawn s in CurrentFloor.AliveEnemiesInRoom(currentRoom.id))
                s.isDead = true;
        }

        int gold = RelicDatabase.ApplyGoldBonus(LootTable.RollGold(currentLayer), Player);
        Player.gold += gold;
        Debug.Log($"[DungeonManager] 전투 승리! 골드 +{gold}");

        string foodId = LootTable.RollFoodDrop(currentLayer);
        if (foodId != null)
        {
            Player.inventory.AddFood(foodId);
            Debug.Log($"[DungeonManager] 식료품 획득: {FoodDatabase.Get(foodId)?.displayName}");
        }

        bool isBoss  = currentRoom?.roomType == TileType.Boss;
        bool isElite = currentRoom?.roomType == TileType.EliteEnemy;
        int  cardCount = isBoss ? 3 : (isElite ? 2 : 1);

        string relicId = null;
        if (isBoss || isElite)
        {
            relicId = LootTable.RollRelicDrop(isBoss, Player);
            if (relicId != null)
            {
                Player.relics.Add(relicId);
                RelicDatabase.ApplyPassiveEffects(relicId, Player);
                Debug.Log($"[DungeonManager] 유물 획득: {RelicDatabase.Get(relicId)?.displayName}");
            }
        }

        pendingReward = new BattleReward
        {
            cardChoices = LootTable.RollCardRewards(currentLayer, cardCount),
            gold        = gold,
            foodId      = foodId,
            relicId     = relicId,
        };

        if (isBoss && currentLayer >= 3)
        {
            TransitionTo(GameState.Victory);
            return;
        }

        if (isBoss)
        {
            currentLayer++;
            CurrentFloor = FloorGenerator.Generate(currentLayer);
        }

        TransitionTo(GameState.Reward);
    }

    public void OnBattleLost() => TransitionTo(GameState.GameOver);

    // 카드 보상 선택 (UI에서 호출)
    public void ChooseCard(string cardId)
    {
        Card card = CardDatabase.Create(cardId);
        if (card != null)
        {
            Player.deck.AddCard(card);
            Debug.Log($"[DungeonManager] 카드 획득: {card.cardName}");
        }
        SaveSystem.Save(Player, currentLayer);
        TransitionTo(GameState.DungeonMap);
    }

    public void SkipCardReward()
    {
        SaveSystem.Save(Player, currentLayer);
        TransitionTo(GameState.DungeonMap);
    }

    // 휴식
    public void UseRestSite()
    {
        int heal = Mathf.RoundToInt(Player.maxHp * 0.3f);
        Player.Heal(heal);
        if (currentRoom != null) currentRoom.isCleared = true;
        SaveSystem.Save(Player, currentLayer);
        TransitionTo(GameState.DungeonMap);
    }

    // 휴식하지 않고 그냥 지나치기 - 모닥불을 소모하지 않으므로 나중에 다시 방문 가능
    public void LeaveRestSite()
    {
        TransitionTo(GameState.DungeonMap);
    }

    // 상점 구매 - UI에서 호출 (index: ShopSystem.Items 인덱스)
    public bool BuyShopItem(int index)
    {
        bool success = shop.TryBuy(index, Player);
        if (success)
        {
            ShopItem item = shop.Items[index];
            if (item.type == ShopItemType.RemoveCard) shopCardRemovePurchased = true;
            shopPurchasedThisVisit = true;
        }
        return success;
    }

    // 카드 제거 서비스 사용 - UI에서 카드 선택 후 호출
    public bool RemoveCardFromDeck(string cardId)
    {
        if (!shopCardRemovePurchased) return false;
        bool removed = shop.RemoveCard(cardId, Player);
        if (removed) shopCardRemovePurchased = false;
        return removed;
    }

    // 식료품 섭취 - 인벤토리 UI에서 호출
    public bool UseFoodItem(string foodId)
    {
        FoodData food = FoodDatabase.Get(foodId);
        if (food == null) return false;
        if (!Player.inventory.RemoveFood(foodId)) return false;

        Player.ChangeHunger(food.hungerRestore);
        Debug.Log($"[DungeonManager] {food.displayName} 섭취 → 배고픔 +{food.hungerRestore} (현재 {Player.hunger}/{Player.maxHunger})");
        SaveSystem.Save(Player, currentLayer);
        return true;
    }

    public ShopSystem GetShop() => shop;

    // 성소 3분기 중 카드 선택 - UI에서 호출 (index: 0=공격, 1=유틸, 2=방어)
    public ShrineOption[] GetPendingShrineOptions() => pendingShrineOptions;

    public bool ChooseShrineCard(int index)
    {
        if (pendingShrineOptions == null || index < 0 || index >= pendingShrineOptions.Length) return false;

        Card card = pendingShrineOptions[index].card;
        Player.deck.AddCard(card);
        Debug.Log($"[DungeonManager] 성소에서 카드 제작: {card.cardName}");

        if (currentRoom != null) currentRoom.isCleared = true;
        pendingShrineOptions = null;
        SaveSystem.Save(Player, currentLayer);
        TransitionTo(GameState.DungeonMap);
        return true;
    }

    // 아무것도 안 사고 나가면 상점을 소모하지 않는다 (나중에 다시 들러서 살 수 있음)
    public void LeaveShop()
    {
        if (currentRoom != null && shopPurchasedThisVisit) currentRoom.isCleared = true;
        SaveSystem.Save(Player, currentLayer);
        TransitionTo(GameState.DungeonMap);
    }

    public BattleReward GetPendingReward() => pendingReward;

    private void TransitionTo(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[DungeonManager] → {newState}");
        UIManager.Instance?.ShowPanel(newState);
    }
}
