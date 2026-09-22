using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DetailsWindowController : MonoBehaviour
{
    [System.Serializable]
    private class DetailCard
    {
        public string title = string.Empty;
        public TMP_Text titleText;
        public TMP_Dropdown dropdown;
    }

    [Header("Details")]
    [SerializeField] private RectTransform detailsWindow;
    [SerializeField] private GridLayoutGroup cardsGrid;
    [SerializeField] private DetailCard[] detailCards = new DetailCard[15];

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
        ConfigureGrid();
        ConfigureDetailCards();
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
        if (!enabled || (!isOpen && !isAnimating))
            return;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideTo(GetClosedPosition(), false));
    }

    public bool HandleEscape()
    {
        if (!IsOpen)
            return false;

        CloseDetails();
        return true;
    }

    private void ConfigureDetailCards()
    {
        if (detailCards == null)
            return;

        for (int cardIndex = 0; cardIndex < detailCards.Length; cardIndex++)
        {
            DetailCard card = detailCards[cardIndex];
            if (card == null)
                continue;

            if (card.titleText != null)
                card.titleText.text = card.title;

            if (card.dropdown == null)
            {
                Debug.LogWarning($"Details card {cardIndex + 1} has no TMP_Dropdown assigned.", this);
                continue;
            }

            card.dropdown.interactable = card.dropdown.options.Count > 0;
        }
    }

    private void ConfigureGrid()
    {
        if (cardsGrid == null)
            return;

        cardsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardsGrid.constraintCount = 3;
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
4. Buttonin ainoa OnClick-kutsu on:
    BootstrapSceneManager.OpenDetailWindow()
5. Lisaa Details Sceneen tama DetailsWindowController.
6. Aseta detailsWindow Details-paneeliin.
7. Aseta cardsGrid-kenttaan GridLayoutGroup, jossa on kolme saraketta.
8. Aseta detailCards-taulukkoon 15 korttia.

Jokaisella kortilla on oma:
- title
- titleText
- dropdown

Dropdownin vaihtoehdot asetetaan suoraan kortin prefabissa olevan
TMP_Dropdown-komponentin Options-listaan. Erillista options-taulukkoa
ei tarvita.

Esimerkiksi yhdella kortilla voi olla vaihtoehdot:
Musta, Puna, Vihree

Ja toisella:
Musta, Crimson-red, Dark-electric-blue, Forest-green

Details Scene suljetaan BootstrapSceneManager.CloseDetailWindow()-metodilla.
Esc tai Backspace sulkee Details Scenen. Seuraava painallus sulkee huoneen.

=== End ===*/
