using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace paperScaleOfDifferentLeastCounts
{
    public class SlideUIController : MonoBehaviour
    {
        [System.Serializable]
        public class PageUIData
        {
            [Header("Page Number")]
            public int pageNumber;

            [Header("UI Text")]
            [TextArea(3, 10)]
            public string pageText;

            [Header("Optional Rect Transform Control")]
            public bool overrideRect = false;

            public Vector2 anchoredPosition;
            public Vector2 sizeDelta = new Vector2(600, 200);
        }

        [Header("References")]
        public SlideCameraController slideController;

        public TMP_Text pageTextUI;

        public RectTransform commonImageRect;

        [Header("Default Rect Settings")]
        public Vector2 defaultAnchoredPosition;
        public Vector2 defaultSizeDelta;
        public Vector2 defaultAnchorMin = new Vector2(0, 1);
        public Vector2 defaultAnchorMax = new Vector2(0, 1);
        public Vector2 defaultPivot = new Vector2(0, 1);

        [Header("Page UI Data")]
        public PageUIData[] pageUIData;

        int lastPage = -1;

        void Start()
        {
            if (slideController == null) return;

            Apply(slideController.CurrentPageNumber);
            lastPage = slideController.CurrentPageNumber;
        }

        void Update()
        {
            if (slideController == null) return;

            int page = slideController.CurrentPageNumber;

            if (page != lastPage)
            {
                Apply(page);
                lastPage = page;
            }
        }

        void Apply(int page)
        {
            PageUIData data = GetPageData(page);

            if (data == null)
            {
                ResetUI();
                return;
            }

            // TEXT UPDATE
            if (pageTextUI != null)
                pageTextUI.text = data.pageText;

            // RECT UPDATE
            if (commonImageRect != null)
            {
                commonImageRect.anchorMin = defaultAnchorMin;
                commonImageRect.anchorMax = defaultAnchorMax;
                commonImageRect.pivot = defaultPivot;

                if (data.overrideRect)
                {
                    commonImageRect.anchoredPosition = data.anchoredPosition;
                    commonImageRect.sizeDelta = data.sizeDelta;
                }
                else
                {
                    commonImageRect.anchoredPosition = defaultAnchoredPosition;
                    commonImageRect.sizeDelta = defaultSizeDelta;
                }
            }
        }

        PageUIData GetPageData(int page)
        {
            if (pageUIData == null) return null;

            foreach (var data in pageUIData)
            {
                if (data.pageNumber == page)
                    return data;
            }

            return null;
        }

        void ResetUI()
        {
            if (pageTextUI != null)
                pageTextUI.text = "";

            if (commonImageRect != null)
            {
                commonImageRect.anchoredPosition = defaultAnchoredPosition;
                commonImageRect.sizeDelta = defaultSizeDelta;
            }
        }
    }
}