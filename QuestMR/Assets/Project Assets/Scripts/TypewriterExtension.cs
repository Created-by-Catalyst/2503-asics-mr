using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public static class TypewriterExtensions
{    public static async UniTask TypeTextAsync(
        TMP_Text tmp,
        string fullText,
        float delayPerChar = 0.005f,
        CancellationToken cancellationToken = default)
    {
        if (tmp == null || string.IsNullOrEmpty(fullText))
            return;

        tmp.text = string.Empty;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (fullText[i] == '<')
            {
                int closingIndex = fullText.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    tmp.text += fullText.Substring(i, closingIndex - i + 1);
                    i = closingIndex;
                    continue;
                }
            }

            tmp.text += fullText[i];

            try
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(delayPerChar),
                    cancellationToken: cancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                return;
            }
        }
    }
}
