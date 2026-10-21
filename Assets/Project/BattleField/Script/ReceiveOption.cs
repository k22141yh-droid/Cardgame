using System.Collections;
using System.Collections.Generic;
using BattleField.Script.Janken;
using UnityEngine;

using Common.MasterData;

public class ReceiveOption : MonoBehaviour
{
    // Start is called before the first frame update
    public CardHand HandReceiver() {
        int random = Random.Range(0, 3);
        if (random == 0) return CardHand.Rock;
        if (random == 1) return CardHand.Scissors;
        return CardHand.Paper;
    }

    public CardType TypeReceiver() {
        int random = Random.Range(0, 3);
        if (random == 0) return CardType.Fire;
        if (random == 1) return CardType.Grass;
        return CardType.Water;
    }
}
