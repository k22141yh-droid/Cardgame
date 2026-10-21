#nullable enable

using InGame.Player;

namespace InGame
{
    public class GameSettings
    {
        public PlayerPresenter Player { get; private set; }
        public PlayerPresenter OpponentPlayer { get; private set; }
        public int MaxHp { get; private set; }
        public int MaxTurn { get; private set; }

        public GameSettings()
        {
            MaxHp = 3000;
            MaxTurn = 3;

            Player = new(MaxHp);
            OpponentPlayer = new(MaxHp);
        }
    }
}