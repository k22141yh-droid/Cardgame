using MasterMemory;
using MessagePack;

namespace Common.MasterData
{
    [MemoryTable("card"), MessagePackObject(true)]
    public sealed class Card : IValidatable<Card>
    {
        [PrimaryKey] public string Id { get; private set; }
        public int Power { get; private set; }
        public CardHand Hand { get; private set; }
        public CardType Type { get; private set; }
        public string ImageId { get; private set; }

        public Card(string id, int power, CardHand hand, CardType type, string imageId)
        {
            Id = id;
            Power = power;
            Hand = hand;
            Type = type;
            ImageId = imageId;
        }

        void IValidatable<Card>.Validate(IValidator<Card> validator)
        {
            if (Power < 0)
            {
                validator.Fail("Powerは0より大きい必要があります");
            }
            if (ImageId == "")
            {
                validator.Fail("ImageIdは入力必須です");
            }
        }
    }

    public enum CardHand
    {
        Rock,
        Scissors,
        Paper,
        Yoshimoto
    }

    public enum CardType
    {
        Fire,
        Water,
        Grass,
        Yoshimoto
    }
}