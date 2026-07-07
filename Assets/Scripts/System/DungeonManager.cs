using System.Collections.Generic;
using UnityEngine;

public enum GameState { CharacterSelect, DungeonMap, Battle, Reward, Rest, Shop, Shrine, GameOver, Victory }

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    public GameState       CurrentState  { get; private set; }
    public PlayerCharacter Player        { get; private set; }
    public DungeonMap      CurrentMap    { get; private set; }
    public int             CurrentLayer  => currentLayer;

    private int            currentLayer = 1;
    private MapNode        currentBattleNode;
    private BattleReward   pendingReward;
    private ShopSystem     shop = new ShopSystem();
    private bool           shopCardRemovePurchased = false;
    private ShrineOption[] pendingShrineOptions;

    void Awake() => Instance = this;

    public void StartRun(CharacterClass cls)
    {
        Player       = PlayerFactory.Create(cls);
        currentLayer = 1;
        CurrentMap   = MapGenerator.Generate(currentLayer);
        Debug.Log($"[DungeonManager] 런 시작: {Player.characterName}  계층:{currentLayer}");
        TransitionTo(GameState.DungeonMap);
    }

    // 플레이어 이동 - 맵 UI에서 호출 (nodeId: 지금 위치에서 이어진 노드만 가능)
    public void MoveToNode(int nodeId)
    {
        if (CurrentState != GameState.DungeonMap) return;

        MapNode node = CurrentMap.TryMoveTo(nodeId);
        if (node == null) return;

        HungerSystem.OnPlayerMove(Player);
        if (!Player.IsAlive)
        {
            TransitionTo(GameState.GameOver);
            return;
        }

        if (node.HasEnemy)
        {
            currentBattleNode = node;

            var enemyList = new List<Enemy>();
            foreach (string id in node.enemyIds)
            {
                Enemy e = EnemyFactory.Create(id);
                if (e != null) enemyList.Add(e);
            }

            BattleManager.Instance.StartBattle(Player, enemyList);
            TransitionTo(GameState.Battle);
        }
        else if (node.type == TileType.Rest && !node.isCleared)
        {
            currentBattleNode = node;
            TransitionTo(GameState.Rest);
        }
        else if (node.type == TileType.Shop && !node.isCleared)
        {
            currentBattleNode = node;
            shop.Generate(currentLayer, Player);
            shopCardRemovePurchased = false;
            TransitionTo(GameState.Shop);
        }
        else if (node.type == TileType.Shrine && !node.isCleared)
        {
            currentBattleNode = node;
            pendingShrineOptions = ShrineSystem.GenerateOptions(Player);
            TransitionTo(GameState.Shrine);
        }
    }

    // BattleManager에서 호출
    public void OnBattleWon()
    {
        if (currentBattleNode != null)
            currentBattleNode.isCleared = true;

        int gold = RelicDatabase.ApplyGoldBonus(LootTable.RollGold(currentLayer), Player);
        Player.gold += gold;
        Debug.Log($"[DungeonManager] 전투 승리! 골드 +{gold}");

        string foodId = LootTable.RollFoodDrop(currentLayer);
        if (foodId != null)
        {
            Player.inventory.AddFood(foodId);
            Debug.Log($"[DungeonManager] 식료품 획득: {FoodDatabase.Get(foodId)?.displayName}");
        }

        bool isBoss  = currentBattleNode?.type == TileType.Boss;
        bool isElite = currentBattleNode?.type == TileType.EliteEnemy;
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
        if (currentBattleNode != null) currentBattleNode.isCleared = true;
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

    // 성소 3분기 중 카드 선택 - UI에서 호출 (index: 0=공격, 1=유틸, 2=방어)
    public ShrineOption[] GetPendingShrineOptions() => pendingShrineOptions;

    public bool ChooseShrineCard(int index)
    {
        if (pendingShrineOptions == null || index < 0 || index >= pendingShrineOptions.Length) return false;

        Card card = pendingShrineOptions[index].card;
        Player.deck.AddCard(card);
        Debug.Log($"[DungeonManager] 성소에서 카드 제작: {card.cardName}");

        if (currentBattleNode != null) currentBattleNode.isCleared = true;
        pendingShrineOptions = null;
        SaveSystem.Save(Player, currentLayer);
        TransitionTo(GameState.DungeonMap);
        return true;
    }

    public void LeaveShop()
    {
        if (currentBattleNode != null) currentBattleNode.isCleared = true;
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
