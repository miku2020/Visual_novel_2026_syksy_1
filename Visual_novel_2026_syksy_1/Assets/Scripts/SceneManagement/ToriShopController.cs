using UnityEngine;

public class ToriShopController : MonoBehaviour
{
    public void BuyOutfit(int outfitIndex)
    {
        if (CharacterOutfitController.Instance == null)
        {
            Debug.LogError("CharacterOutfitController is not available. Varmista, että Hahmo Scene on ladattuna.");
            return;
        }

        CharacterOutfitController.Instance.SetOutfit(outfitIndex);
    }
}
