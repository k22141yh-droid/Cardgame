#nullable enable

using Common.MasterData;
using Common.Result;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Player
{
    public class PlayerPresenter
    {
        public int Hp { get; set; }
        public List<Card> Cards { get; private set; } = new();

        public PlayerPresenter(int hp)
        {
            Hp = hp;
        }

        public Result<Card, SetCurrentCardError> SetCurrentCard(string id)
        {
            if (id == string.Empty)
            {
                return SetCurrentCardError.EmptyId;
            }
            else if (Cards.Any(card => card.Id == id))
            {
                return SetCurrentCardError.DuplicateError;
            }
            else if (!MasterDataDB.DB.CardTable.TryFindById(id, out var card))
            {
                return SetCurrentCardError.IncorrectId;
            }
            else
            {
                Cards.Add(card);
                return card;
            }
        }
    }

    public enum SetCurrentCardError
    {
        EmptyId,
        DuplicateError,
        IncorrectId
    }
}