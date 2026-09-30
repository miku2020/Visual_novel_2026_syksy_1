using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DetailsWindowController : MonoBehaviour
{
    private enum OverlayType
    {
        Details,
        Tori
    }

    [Header("Details")]
    [SerializeField] private RectTransform detailsWindow;
    [SerializeField] private ScrollRect cardsScrollRect;
    [SerializeField] private OverlayType overlayType = OverlayType.Details;

    [Header("Slide animation")]
    [SerializeField, Min(0.01f)] private float slideDuration = 0.25f;
    [SerializeField] private bool startClosed = true;

    private Coroutine slideCoroutine;
    private Vector2 openPosition;
    private bool isOpen;
    private bool isAnimating;

    public bool IsOpen => isOpen || isAnimating;

    private void Awake()
    {
        if (detailsWindow == null)
        {
            detailsWindow = GetComponent<RectTransform>();
            if (detailsWindow == null || detailsWindow.name != "Details")
                detailsWindow = transform.Find("Details")?.GetComponent<RectTransform>();
        }

        if (detailsWindow == null)
        {
            Debug.LogError("DetailsWindowController needs a Details window RectTransform.", this);
            enabled = false;
            return;
        }

        openPosition = detailsWindow.anchoredPosition;
        FitScrollContentToChildren();
    }

    private void Start()
    {
        if (startClosed)
        {
            detailsWindow.gameObject.SetActive(false);
            OpenDetails();
        }
        else
        {
            isOpen = true;
        }
    }

    public void OpenDetails()
    {
        if (!enabled || isOpen || isAnimating)
            return;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        detailsWindow.gameObject.SetActive(true);
        detailsWindow.anchoredPosition = GetClosedPosition();
        slideCoroutine = StartCoroutine(SlideTo(openPosition, true));
    }

    public void CloseDetails()
    {
        if (!enabled || (!isOpen && !isAnimating)) return;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideTo(GetClosedPosition(), false));
    }

    public bool HandleEscape()
    {
        if (!IsOpen) return false;

        CloseDetails();
        return true;
    }

    // Kortit asetellaan vapaasti Editorissa; Content-alue mitoitetaan niiden todellisen sijainnin mukaan,
    // jotta ScrollRectin Clamped-vieritys pysyy juuri sisällön rajaamalla alueella.
    public void FitScrollContentToChildren()
    {
        if (cardsScrollRect == null || cardsScrollRect.content == null) return;

        RectTransform content = cardsScrollRect.content;

        if (content.childCount == 0) return;

        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, content);
        content.sizeDelta = new Vector2(bounds.size.x, bounds.size.y);
    }

    private IEnumerator SlideTo(Vector2 targetPosition, bool opening)
    {
        isAnimating = true;
        Vector2 startPosition = detailsWindow.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / slideDuration);
            progress = progress * progress * (3f - 2f * progress);
            detailsWindow.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, progress);
            yield return null;
        }

        detailsWindow.anchoredPosition = targetPosition;
        isAnimating = false;
        isOpen = opening;

        if (!opening)
            detailsWindow.gameObject.SetActive(false);

        slideCoroutine = null;
    }

    private Vector2 GetClosedPosition()
    {
        float width = detailsWindow.rect.width;
        return openPosition + Vector2.right * (width + 32f);
    }

    private void Update()
    {
        if (Keyboard.current == null || BootstrapSceneManager.Instance == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if (overlayType == OverlayType.Tori)
                BootstrapSceneManager.Instance.CloseToriUI();
            else
                BootstrapSceneManager.Instance.CloseDetailWindow();
        }
    }
}

/*=== README ===

DETAILS-IKKUNAN KAYTTOONOTTO

1. Details on oma Unity Scene, joka ladataan additive-tilassa.
2. Lisaa Details Scene Addressables-jarjestelmaan.
3. Aseta Details Scene BootstrapSceneManagerin
    Details Window Scene -AssetReference-kenttaan.
4. Aseta BootstrapSceneManagerin detailsWindowScene-taulukkoon
   yksi Addressable-Details Scene jokaista hahmoa varten.
5. Hahmon huoneen Buttonin OnClick-kutsuksi voidaan asettaa:
    RoomManager.OpenDetailsForCharacter(characterIndex)
   jossa characterIndex vastaa detailsWindowScene-taulukon indeksia.
6. Lisaa sama DetailsWindowController myos Tori Sceneen.
7. Aseta overlayType Inspectorissa: Details Details Sceneille ja Tori Tori Sceneille.
8. Aseta detailsWindow Details-paneeliin.
9. Lisaa Details-paneeliin ScrollRect (Viewport + Content) ja aseta sen
   Movement Type -asetukseksi Clamped. Aseta cardsScrollRect-kenttaan
   tama ScrollRect.
10. Aseta kortit Content-objektin lapsiksi ja asettele ne vapaasti,
    ilman GridLayoutGroupia. Skripti mitoittaa Content-alueen
    automaattisesti korttien todellisen sijainnin mukaan (Awakessa),
    jotta vieritys clamppautuu juuri siihen alueeseen jossa kortteja on.
11. Jos kortteja lisataan tai niiden nakyvyytta muutetaan ajon aikana,
    kutsu FitScrollContentToChildren() uudelleen paivittaaksesi
    vierityksen rajat.

Tämä skripti ei hallitse korttien tekstejä, painikkeita tai näkyvyyttä.
Tekstit, "Näytä lisää" -painike ja lisäsisällön näkyvyys asetetaan kokonaan
manuaalisesti Unity Editorissa. Skripti hallitsee vain Details-ikkunan avaamista,
sulkemista ja korttien vierityksen rajaamista sisällön mukaan.

Details-tilassa Escape tai Backspace sulkee Details Scenen.
Tori-tilassa Escape tai Backspace sulkee Tori Scenen.
Skripti valitsee sulkemismetodin overlayType-asetuksen perusteella.

=== End ===*/
