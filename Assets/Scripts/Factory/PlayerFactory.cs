public static class PlayerFactory
{
    public static PlayerCharacter Create(CharacterClass cls)
    {
        PlayerData data = PlayerDatabase.Get(cls);
        if (data == null) return null;

        var player = new PlayerCharacter(cls);

        foreach (string cardId in data.starterDeckCardIds)
        {
            var card = CardDatabase.Create(cardId);
            if (card != null)
                player.deck.AddCard(card);
        }

        return player;
    }
}
