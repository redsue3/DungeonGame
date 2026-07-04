using System.Collections.Generic;
using UnityEngine;

public enum GameState { CharacterSelect, DungeonMap, Battle, Reward, Rest, Shop, GameOver, Victory }

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    public GameState       CurrentState  { get; private set; }
    public PlayerCharacter Player        { get; private set; }
    public DungeonMap      CurrentMap    { get; private set; }
    public int             CurrentLayer  => currentLayer;

    private int          currentLayer = 1;
    private MapTile      currentBattleTile;
    private BattleReward pendingReward;
    private ShopSystem   shop = new ShopSystem();
    private bool         shopCardRemovePurchased = false;

    void Awake() => Instance = this;

    public void StartRun(CharacterClass cls)
    {
        Player       = PlayerFactory.Create(cls);
        currentLayer = 1;
        CurrentMap   = MapGenerator.Generate(currentLayer);
        Debug.Log($"[DungeonManager] 런 시작: {Player.characterName}  계층:{currentLayer}");
        TransitionTo(GameState.DungeonMap);
    }

    // 플레이어 이동 - 맵 UI에서 호출 (dx, dy: -1/0/1)
    public void MovePlayer(int dx, int dy)
    {
        if (CurrentState != GameState.DungeonMap) return;

        MapTile tile = CurrentMap.TryMove(dx, dy);
        if (tile == null) return;

        HungerSystem.OnPlayerMove(Player);
        if (!Player.IsAlive)
        {
            TransitionTo(GameState.GameOver);
            return;
        }

        if (tile.HasEnemy)
        {
            currentBattleTile = tile;

            var enemyList = new List<Enemy>();
            foreach (string id in tile.enemyIds)
            {
                Enemy e = EnemyFactory.Create(id);
                if (e != null) enemyList.Add(e);
            }

            BattleManager.Instance.StartBattle(Player, enemyList);
            TransitionTo(GameState.Battle);
        }
        else if (tile.type == TileType.Rest && !tile.isCleared)
        {
            currentBattleTile = tile;
            TransitionTo(GameState.Rest);
        }
        else if (tile.type == TileType.Shop && !tile.isCleared)
        {
            currentBattleTile = tile;
            shop.Generate(currentLayer, Player);
            shopCardRemovePurchased = false;
            TransitionTo(GameState.Shop);
        }
    }

    // BattleManager에서 호출
    public void OnBattleWon()
    {
        if (currentBattleTile != null)
            currentBattleTile.isCleared = true;

        int gold = RelicDatabase.ApplyGoldBonus(LootTable.RollGold(currentLayer), Player);
        Player.gold += gold;
        Debug.Log($"[DungeonManager] 전투 승리! 골드 +{gold}");

        string foodId = LootTable.RollFoodDrop(currentLayer);
        if (foodId != null)
        {
            Player.inventory.AddFood(foodId);
            Debug.Log($"[DungeonManager] 식료품 획득: {FoodDatabase.Get(foodId)?.displayName}");
        }

        bool isBoss = currentBattleTile?.type == TileType.Boss;
        int  cardCount = isBoss ? 3 : (currentBattleTile?.type == TileType.EliteEnemy ? 2 : 1);

        pendingReward = new BattleReward
        {
            cardChoices = LootTable.RollCardRewards(currentLayer, cardCount),
            gold        = gold,
            foodId      = foodId,
        };

        if (isBoss && currentLayer >= 3)
        {
            TransitionTo(GameState.Victory);
            return;
        }

        if (isBoss)
        {
            currentLayer++;
            CurrentMap = MapGenerator.Generate(currentLayer);
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
        if (currentBattleTile != null) currentBattleTile.isCleared = true;
        SaveSystem.Save(Player, currentLayer);
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

    public void LeaveShop()
    {
        if (currentBattleTile != null) currentBattleTile.isCleared = true;
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
