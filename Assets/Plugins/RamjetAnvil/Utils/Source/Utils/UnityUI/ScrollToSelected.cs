using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Courtesy of: http://emmanuelsalvacruz.com/scrollrect-and-unity-ui/
/// </summary>
[RequireComponent(typeof (ScrollRect))]
public class ScrollToSelected : MonoBehaviour {
    [SerializeField] private RectTransform m_ContentRectTransform;
    private RectTransform m_RectTransform;
    private ScrollRect m_ScrollRect;
    private RectTransform _selectedTransform;

    private void Awake() {
        m_ScrollRect = GetComponent<ScrollRect>();
        m_RectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        UpdateScrollToSelected();
    }

    private void UpdateScrollToSelected() {
        var selected = EventSystem.current.currentSelectedGameObject;

        bool isScrollingRequired = false;
        if (selected != null) {
            var isCacheInvalid = _selectedTransform == null ||
                                 selected != _selectedTransform.gameObject;
            if (isCacheInvalid && selected.transform.IsChildOf(m_ContentRectTransform.transform)) {
                _selectedTransform = selected.GetComponent<RectTransform>();
                isScrollingRequired = true;
            }
        }

        if (_selectedTransform != null && isScrollingRequired) {
            var viewPortRange = new Range(
                start: m_ContentRectTransform.anchoredPosition.y,
                end: m_ContentRectTransform.anchoredPosition.y + m_RectTransform.rect.height);

            float localContentPosition = m_ContentRectTransform.InverseTransformPoint(_selectedTransform.position).y;
            // Convert from center to top-left
            localContentPosition = localContentPosition - _selectedTransform.rect.height / 2;
            var contentRange = new Range(
                start: Mathf.Abs(localContentPosition + _selectedTransform.rect.height),
                end: Mathf.Abs(localContentPosition));

//            Debug.Log("viewport range: " + viewPortRange + ", content range " + contentRange + ", diff: " + AmountOutOfRange(viewPortRange, contentRange) 
//                + " viewport height " + m_RectTransform.rect.height + " content height " + m_ContentRectTransform.rect.height);

            var amountOutOfRange = AmountOutOfRange(viewPortRange, contentRange);
            if (Mathf.Abs(amountOutOfRange) > float.Epsilon && m_ContentRectTransform.rect.height > m_RectTransform.rect.height) {
                var contentHeightDifference = m_ContentRectTransform.rect.height - m_RectTransform.rect.height;
                m_ScrollRect.verticalNormalizedPosition += amountOutOfRange / contentHeightDifference;
            }
        }
    }

    /// <summary>
    /// Calculates how much of the given content range is outside of the viewport range.
    /// </summary>
    private static float AmountOutOfRange(Range viewportRange, Range contentRange) {
        if (contentRange.Start < viewportRange.Start) {
            return viewportRange.Start - contentRange.Start;
        } else if (contentRange.End > viewportRange.End) {
            return viewportRange.End - contentRange.End;
        } else {
            return 0f;
        }
    }

    private struct Range {
        public readonly float Start;
        public readonly float End;

        public Range(float start, float end) {
            Start = start;
            End = end;
        }

        public override string ToString() {
            return string.Format("Start: {0}, End: {1}", Start, End);
        }
    }

}