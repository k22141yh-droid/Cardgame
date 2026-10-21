#nullable enable

using System.Collections.Generic;
using System.Linq;
using Common.MasterData;
using UnityEngine;
using UnityEngine.UI;

namespace Project.InGame.Program.Scripts.Tehuda
{
    public class TehudaPresenter:MonoBehaviour
    {
        [SerializeField] private GameObject tehuda = null!;
        [SerializeField] private RawImage image1 = null!;
        [SerializeField] private RawImage image2 = null!;
        [SerializeField] private RawImage image3 = null!;
        [SerializeField] private RawImage image4 = null!;
        [SerializeField] private RawImage image5 = null!;
        [SerializeField] private Texture rock = null!;
        [SerializeField] private Texture scissors = null!;
        [SerializeField] private Texture paper = null!;

        public void SetImages(string[] cardIds, List<Card> cards)
        {
            tehuda.SetActive(true);
            var restCardIds = cardIds.Where((id)=>cards.All(card => card.Id != id)).ToArray();
            var restCards = restCardIds.Select((id) =>
            {
                MasterDataDB.DB.CardTable.TryFindById(id, out var card);
                return card;
            }).ToArray();
            image1.texture = GetHandTexture(restCards[0].Hand);
            image2.texture = GetHandTexture(restCards[1].Hand);
            image3.texture = GetHandTexture(restCards[2].Hand);
            if (restCards.Length > 3)
            {
                image4.texture = GetHandTexture(restCards[3].Hand);
            }
            else
            {
                image4.gameObject.SetActive(false);
            }
            if (restCards.Length > 4)
            {
                image5.texture = GetHandTexture(restCards[4].Hand);
            }
            else
            {
                image5.gameObject.SetActive(false);
            }
        }

        private Texture GetHandTexture(CardHand hand)
        {
            return hand switch
            {
                CardHand.Rock => rock,
                CardHand.Paper => paper,
                _ => scissors
            };
        }
    }
}