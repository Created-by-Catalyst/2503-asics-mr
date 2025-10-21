using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;

public class TagsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Image headerBackground;
    [SerializeField] private Image bodyBackground;
    [SerializeField] private TextMeshProUGUI[] texts;

    [Header("Settings")]
    [SerializeField] private bool useLocalSpace = true;
    [SerializeField] private float drawDuration = 0.25f;
    [SerializeField] private float delayToAnim = 0;

    private Tween drawTween;
    private bool isLineAnimating;

    void Awake()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = !useLocalSpace;
        }
        if (startPoint != null)
            startPoint.localScale = Vector3.zero;
        if (endPoint != null)
            endPoint.localScale = Vector3.zero;

        if (headerBackground != null)
            headerBackground.fillAmount = 0;

        if (bodyBackground != null)
            bodyBackground.fillAmount = 0;

        foreach (TextMeshProUGUI text in texts)
            text.enabled = false;
    }

    async void Start()
    {
        await AnimateLineAsync();
        await AnimateUIElements();
        await AnimateTextElements();
    }

    void LateUpdate()
    {
        if (!isLineAnimating && startPoint && endPoint)
            UpdateFullLine();
    }

    private void UpdateFullLine()
    {
        Vector3 startPos = useLocalSpace
            ? transform.InverseTransformPoint(startPoint.position)
            : startPoint.position;

        Vector3 endPos = useLocalSpace
            ? transform.InverseTransformPoint(endPoint.position)
            : endPoint.position;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    private async UniTask AnimateLineAsync()
    {
        if (!startPoint || !endPoint || !lineRenderer) return;

        await UniTask.Delay((int)(delayToAnim * 1000));

        await startPoint.DOScale(1,0.2f);

        lineRenderer.enabled = true;
        isLineAnimating = true;

        Vector3 startPos = useLocalSpace
            ? transform.InverseTransformPoint(startPoint.position)
            : startPoint.position;

        Vector3 endPos = useLocalSpace
            ? transform.InverseTransformPoint(endPoint.position)
            : endPoint.position;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, startPos);

        float progress = 0f;

        drawTween?.Kill();
        drawTween = DOTween.To(() => progress, x =>
        {
            progress = x;
            Vector3 currentEnd = Vector3.Lerp(startPos, endPos, progress);
            lineRenderer.SetPosition(1, currentEnd);
        },
        1f, drawDuration);

        await drawTween.AsyncWaitForCompletion();

        await endPoint.DOScale(1, 0.2f);

        lineRenderer.SetPosition(1, endPos);
        isLineAnimating = false;
    }

    private async UniTask AnimateUIElements()
    {
        if (headerBackground != null && bodyBackground != null)
        {
            headerBackground.fillAmount = 0f;
            await headerBackground.DOFillAmount(1, drawDuration);

            bodyBackground.fillAmount = 0f;
            await bodyBackground.DOFillAmount(1, drawDuration);
        }
    }

    private async UniTask AnimateTextElements()
    {
        List<UniTask> texttasks = new List<UniTask>();
        foreach (TextMeshProUGUI text in texts)
        {
            string copy = text.text;
            text.text = "";
            text.enabled = true;
            texttasks.Add(TypewriterExtensions.TypeTextAsync(text, copy));
        }
        await UniTask.WhenAll(texttasks);
    }
}
