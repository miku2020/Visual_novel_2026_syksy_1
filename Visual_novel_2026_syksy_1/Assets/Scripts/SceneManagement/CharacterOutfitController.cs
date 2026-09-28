using UnityEngine;

public class CharacterOutfitController : MonoBehaviour
{
    public static CharacterOutfitController Instance { get; private set; }

    [SerializeField] private SpriteRenderer characterSpriteRenderer;
    [SerializeField] private Sprite[] outfits;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (characterSpriteRenderer == null)
            characterSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetOutfit(int outfitIndex)
    {
        if (characterSpriteRenderer == null)
        {
            Debug.LogError("CharacterOutfitController: SpriteRenderer-viitettä ei ole asetettu.");
            return;
        }

        if (outfits == null || outfitIndex < 0 || outfitIndex >= outfits.Length)
        {
            Debug.LogError($"CharacterOutfitController: outfit-indeksi {outfitIndex} on outfits-taulukon ulkopuolella.");
            return;
        }

        characterSpriteRenderer.sprite = outfits[outfitIndex];
    }

    public void SetOutfit(Sprite outfit)
    {
        if (characterSpriteRenderer == null)
        {
            Debug.LogError("CharacterOutfitController: SpriteRenderer-viitettä ei ole asetettu.");
            return;
        }

        characterSpriteRenderer.sprite = outfit;
    }
}

/*=== README ===

HAHMON SPRITEN VAIHTO TORI-SCENESTA

Ongelma: Tori Scene ladataan additive-tilassa Hahmo Scenen paalle, eika Torin
painike voi suoraan viitata Hahmo Scenen GameObjectiin Inspectorissa
(eri Scenet eivat voi viitata toisiinsa serialisoiduilla kentilla).
Ratkaisu on sama static Instance -silta, jota BootstrapSceneManager ja
TaskHolder jo kayttavat.

1. Lisaa tama skripti hahmon GameObjectiin Hahmo Scenessa (samaan, jossa
   hahmon SpriteRenderer on).
2. Aseta characterSpriteRenderer-kenttaan hahmon SpriteRenderer, jos sita
   ei loydy automaattisesti samasta GameObjectista.
3. Taytä outfits-taulukko: yksi Sprite jokaista asua/vaihtoehtoa varten.
4. Koska Hahmo Scene ladataan ennen Tori Scenea eika sita puriteta Torin
   ollessa auki, Instance pysyy voimassa koko ajan kun Tori on avoinna.
5. Tori Scenessa kaytetaan ToriShopController-skriptia (katso
   ToriShopController.cs), jonka BuyOutfit(int)-metodi kutsuu
   CharacterOutfitController.Instance.SetOutfit(outfitIndex).
6. Torin Buttonin OnClick-listaan lisataan ToriShopController.BuyOutfit
   ja parametriksi outfits-taulukon indeksi (0, 1, 2, ...).

=== End ===*/
