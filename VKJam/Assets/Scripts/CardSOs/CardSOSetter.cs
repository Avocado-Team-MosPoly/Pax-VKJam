using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSOs
{
    public class CardSOSetter : MonoBehaviour
    {
        [SerializeField] private ResourceLoadSO resourceLoadSO;
        
        public void SetCardTextures(string id, List<Texture2D> textures)
        {
            // get all packs
            var allCardsPack = PackManager.Instance.All;
            
            // foreach pack
            foreach (var pack in allCardsPack)
            {
                // get each card system
                // and foreach card system
                foreach (var cardSystem in pack.CardInPack)
                {
                    // get card and search for card with ID
                    if (cardSystem.Card.id == id)
                    {
                        var card = cardSystem.Card;

                        SetTextures(card, textures);
                    }
                }
            }
            
        }

        public IEnumerator LoadAllCards()
        {
            // get all packs
            var allCardsPack = PackManager.Instance.All;
            
            List<Texture2D> textures = null;
            Action<List<Texture2D>> onCompleteLoad = (response) => { textures = response;};
            
            // foreach pack
            foreach (var pack in allCardsPack)
            {
                // get each card system
                // and foreach card system
                foreach (var cardSystem in pack.CardInPack)
                {
                    // get each card system
                    var card = cardSystem.Card;
                    
                    // load all textures
                    yield return StartCoroutine(Php_Connect.Request_MonsterTextures(resourceLoadSO.GetURLsByCardID(card.Id),
                        onCompleteLoad)); 
                    
                    // set textures
                    SetTextures(card, textures);
                }
            }
        }

        private void SetTextures(CardSO card, List<Texture2D> textures)
        {
            card.cardTexture = textures[0];
            card.monsterInBestiarySprite = Sprite.Create(textures[1], new Rect(0,0,textures[1].width,textures[1].height), new Vector2(0.5f,0.5f));
            card.monsterTexture = textures[2];
        }
    }
}
