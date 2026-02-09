using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LetterColumn : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IScrollHandler,
    IPointerExitHandler

{
    public RectTransform content;
    public GameObject letterBlockPrefab;
    public float letterHeight = 80f;
    public float verticalGap = 10f;

    [Header("Input")]
    public float scrollSensitivity = 120f;
    bool isScrolling = false;
    float lastScrollTime;
    public float scrollSnapDelay = 0.12f; // tweakable


    char[] letters;
    TextMeshProUGUI[] letterTexts;
    Image[] letterBackgrounds;


    float currentY;
    float minY;
    float maxY;

    int centerIndex;


    float dragY;
    int currentStep;

    int minStep;
    int maxStep;
    float baseY; // visual center offset
    float StepSize => letterHeight + verticalGap;

 
    public void Init(char correctLetter, int height)
    {
        if (content == null)
        {
            Debug.LogError("Content is NOT assigned in LetterColumn!", this);
            return;
        }

        // cleanup old letters
        foreach (Transform c in content)
            Destroy(c.gameObject);

        letters = new char[height];
        letterTexts = new TextMeshProUGUI[height];
        letterBackgrounds = new Image[height];

        centerIndex = Mathf.FloorToInt((height - 1) / 2f);

        // steps allowed
        minStep = -centerIndex;
        maxStep = letters.Length - 1 - centerIndex;
        content.anchoredPosition = new Vector2(0, baseY);

        // choose correct letter index (NOT center)
        int correctIndex;
        do
        {
            correctIndex = Random.Range(0, height);
        }
        while (correctIndex == centerIndex);

        // generate letters + UI
        for (int i = 0; i < height; i++)
        {
            letters[i] = (char)Random.Range('A', 'Z' + 1);

            GameObject block =
                Instantiate(letterBlockPrefab, content);

            RectTransform rt =
                block.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            float step = letterHeight + verticalGap;

            rt.anchoredPosition =
      new Vector2(0, (centerIndex - i) * StepSize);



            TextMeshProUGUI txt =
                block.GetComponentInChildren<TextMeshProUGUI>();

            Image bg =
                block.GetComponentInChildren<Image>();

            if (txt == null || bg == null)
            {
                Debug.LogError(
                    "LetterBlock prefab must contain TextMeshProUGUI and Image",
                    block
                );
                return;
            }

            txt.text = letters[i].ToString();
            txt.color = ColorSchemeManager.Current.Letter;
            letterTexts[i] = txt;
            letterBackgrounds[i] = bg;

            // default background color
            bg.color = ColorSchemeManager.Current.letterBG;
        }

        // force correct letter
        letters[correctIndex] = correctLetter;
        letterTexts[correctIndex].text = correctLetter.ToString();

        // movement bounds
        int maxOffset = letters.Length - 1 - centerIndex;
        int minOffset = -centerIndex;

        maxY = maxOffset * letterHeight;
        minY = minOffset * letterHeight;

        // 🔥 initial center highlight
        UpdateLetterColors();
    }

    void UpdateLetterColors()
    {
        if (isSolved)
            return; // 🔒 solved color stays

        //for (int i = 0; i < letterBackgrounds.Length; i++)
        //{
        //    if (i == centerIndex + currentStep)
        //        letterBackgrounds[i].color =
        //            ColorSchemeManager.Current.Letter;
        //    else
        //        letterBackgrounds[i].color =
        //            ColorSchemeManager.Current.letterBG;
        //}
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(isSolved) return;
        dragY = content.anchoredPosition.y;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (isSolved) return;
        dragY += eventData.delta.y;

        // clamp raw movement so column can't go too far
        float minY = minStep * StepSize;
        float maxY = maxStep * StepSize;

        dragY = Mathf.Clamp(dragY, minY, maxY);

        // move freely with mouse/finger
        content.anchoredPosition = new Vector2(0, dragY);
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        isScrolling = false; // cancel any scroll state
        SnapToNearestStep();
        //// determine nearest step
        //int targetStep =
        //    Mathf.RoundToInt(dragY / StepSize);

        //targetStep = Mathf.Clamp(targetStep, minStep, maxStep);

        //currentStep = targetStep;

        //float snappedY = currentStep * StepSize;

        //content.DOAnchorPosY(snappedY, 0.05f)
        //.SetEase(Ease.OutQuart);


        //dragY = snappedY;

        //UpdateLetterColors();
    }

    public void PlaySolvedSqueeze()
    {
        int idx = centerIndex + currentStep;
        idx = Mathf.Clamp(idx, 0, letterBackgrounds.Length - 1);

        RectTransform rt =
            letterBackgrounds[idx].rectTransform;

        rt.localScale = Vector3.one;

        rt.DOScaleY(0.5f, 0.12f)
     .SetEase(Ease.OutQuad)
     .OnComplete(() =>
     {
         rt.DOScaleY(1f, 0.14f)
           .SetEase(Ease.OutCubic);
     });

    }

    public char GetCenterLetter()
    {
        int index = centerIndex + currentStep;
        index = Mathf.Clamp(index, 0, letters.Length - 1);
        return letters[index];
    }

    public char[] GetAllLetters()
    {
        return letters;
    }
    bool isSolved = false;
    public void SetSolvedState()
    {
        isSolved = true;

        int idx = centerIndex + currentStep;
        idx = Mathf.Clamp(idx, 0, letterBackgrounds.Length - 1);

        letterBackgrounds[idx].color =
            ColorSchemeManager.Current.solvedCenterLetter;
    }
    void Update()
    {
        if (isSolved)
            return;

        if (isScrolling && Time.unscaledTime - lastScrollTime > scrollSnapDelay)
        {
            isScrolling = false;
            SnapToNearestStep();
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (isSolved)
            return;

        float scroll = eventData.scrollDelta.y;

        if (scroll == 0)
            return;

        // scroll up = next letter
        // scroll down = previous letter
        int direction = scroll > 0 ? 1 : -1;

        int targetStep = currentStep + direction;
        targetStep = Mathf.Clamp(targetStep, minStep, maxStep);

        if (targetStep == currentStep)
            return;

        currentStep = targetStep;

        float snappedY = currentStep * StepSize;

        content.DOAnchorPosY(snappedY, 0.05f)
               .SetEase(Ease.OutQuart);

        dragY = snappedY;

        UpdateLetterColors();
    }



    void SnapToNearestStep()
    {
        int targetStep =
            Mathf.RoundToInt(dragY / StepSize);

        targetStep = Mathf.Clamp(targetStep, minStep, maxStep);

        currentStep = targetStep;

        float snappedY = currentStep * StepSize;

        content.DOAnchorPosY(snappedY, 0.05f)
               .SetEase(Ease.OutQuart);

        dragY = snappedY;

        UpdateLetterColors();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSolved)
            return;

        SnapToNearestStep();
    }

}
