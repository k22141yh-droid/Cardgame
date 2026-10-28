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
            SetImage(image1, restCards[0]);
            SetImage(image2, restCards[1]);
            SetImage(image3, restCards[2]);
            if (restCards.Length > 3)
            {
                SetImage(image4, restCards[3]);
            }
            else
            {
                image4.gameObject.SetActive(false);
            }
            if (restCards.Length > 4)
            {
                SetImage(image5, restCards[4]);
            }
            else
            {
                image5.gameObject.SetActive(false);
            }
        }

        // 手の形の画像を、そのカードの属性の色で塗る(白い画像に色を掛ける)
        private void SetImage(RawImage image, Card card)
        {
            image.texture = GetHandTexture(card.Hand);
            image.color = GetTypeColor(card.Type);
        }

        private static Color GetTypeColor(CardType type)
        {
            return type switch
            {
                CardType.Fire => new Color(1f, 0.36f, 0.24f),
                CardType.Water => new Color(0.25f, 0.62f, 1f),
                CardType.Grass => new Color(0.32f, 0.85f, 0.4f),
                _ => new Color(1f, 0.84f, 0.2f)
            };
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