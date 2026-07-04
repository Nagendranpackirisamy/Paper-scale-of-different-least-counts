using UnityEngine;
using System.Collections.Generic;

namespace paperScaleOfDifferentLeastCounts
{
    public class PageAssetController : MonoBehaviour
    {
        [System.Serializable]
        public class AssetRule
        {
            [Header("Page Number")]
            public int pageNumber;

            [Header("Objects To Enable On This Page")]
            public GameObject[] targetObjects;
        }

        [Header("Slide Controller")]
        public SlideCameraController slideController;

        [Header("Rules")]
        public List<AssetRule> rules;

        private Dictionary<GameObject, List<int>> objectToPages;
        private int lastPage = -1;

        void Start()
        {
            BuildMap();

            int startPage = slideController.CurrentPageNumber;
            Apply(startPage);
            lastPage = startPage;
        }

        void Update()
        {
            int currentPage = slideController.CurrentPageNumber;

            if (currentPage != lastPage)
            {
                Apply(currentPage);
                lastPage = currentPage;
            }
        }

        void BuildMap()
        {
            objectToPages = new Dictionary<GameObject, List<int>>();

            foreach (var rule in rules)
            {
                if (rule.targetObjects == null) continue;

                foreach (var obj in rule.targetObjects)
                {
                    if (obj == null) continue;

                    if (!objectToPages.ContainsKey(obj))
                        objectToPages[obj] = new List<int>();

                    objectToPages[obj].Add(rule.pageNumber);
                }
            }
        }

        void Apply(int page)
        {
            foreach (var pair in objectToPages)
            {
                GameObject obj = pair.Key;
                List<int> pages = pair.Value;

                if (obj == null) continue;

                // Active if this object belongs to current page
                obj.SetActive(pages.Contains(page));
            }
        }
    }
}