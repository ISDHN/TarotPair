using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CardController : MonoBehaviour
{
    private const float FlipDurationSeconds = 0.5f;
    private const float FlipBackDelaySeconds = 2f;
    private const float MatchedFadeDelaySeconds = 1f;
    private const float FadeDurationSeconds = 1f;

    private static readonly Dictionary<int, CardController> OpenCards = new();
    private int groupId;
    private bool isOpen = false;
    private bool isMatched = false;
    private bool isFading = false;

    private SpriteRenderer frontSpriteRenderer;
    private SpriteRenderer[] allSpriteRenderers;
    private Coroutine closeCoroutine;
    private Coroutine flipCoroutine;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isFlipping;

    private void Awake()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(closedRotation.eulerAngles.x, closedRotation.eulerAngles.y + 180f, closedRotation.eulerAngles.z);
        frontSpriteRenderer = transform.Find("Front").GetComponent<SpriteRenderer>();
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void Initialize(int id, Sprite frontSprite)
    {
        groupId = id;
        frontSpriteRenderer.sprite = frontSprite;
        isOpen = false;
        isMatched = false;
        isFading = false;
        isFlipping = false;
        SetAlpha(1f);
        transform.rotation = closedRotation;
        if (OpenCards.TryGetValue(groupId, out CardController openCard) && openCard == this)
        {
            OpenCards.Remove(groupId);
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"Card clicked: {gameObject.name} (Group ID: {groupId})");
        if (isOpen || isMatched || isFading || isFlipping)
        {
            return;
        }

        OpenCard();
    }

    private void OpenCard()
    {
        isOpen = true;
        StartFlip();
        OpenCards.TryGetValue(groupId, out CardController matchedCard);

        if (matchedCard != null)
        {
            isMatched = true;
            matchedCard.isMatched = true;

            StopCloseCoroutine();
            matchedCard.StopCloseCoroutine();

            OpenCards.Remove(groupId);

            StartCoroutine(FadeOutAfterDelay());
            matchedCard.StartCoroutine(matchedCard.FadeOutAfterDelay());
            return;
        }

        OpenCards[groupId] = this;
        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(FlipDurationSeconds + FlipBackDelaySeconds);

        if (!isMatched && !isFading)
        {
            isOpen = false;
            if (OpenCards.TryGetValue(groupId, out CardController openCard) && openCard == this)
            {
                OpenCards.Remove(groupId);
            }
            StartFlip();
        }

        closeCoroutine = null;
    }

    private IEnumerator FadeOutAfterDelay()
    {
        isFading = true;
        yield return new WaitForSeconds(MatchedFadeDelaySeconds);

        float elapsed = 0f;
        while (elapsed < FadeDurationSeconds)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - (elapsed / FadeDurationSeconds));
            yield return null;
        }

        SetAlpha(0f);
        TryActivateWinTextWhenLastCard();
        gameObject.SetActive(false);
    }

    private void StartFlip()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }
        flipCoroutine = StartCoroutine(FlipToRotation());
    }

    private IEnumerator FlipToRotation()
    {
        isFlipping = true;
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < FlipDurationSeconds)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, Mathf.Clamp01(elapsed / FlipDurationSeconds));
            yield return null;
        }

        transform.rotation = targetRotation;
        isFlipping = false;
        flipCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = allSpriteRenderers[i];
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private static void TryActivateWinTextWhenLastCard()
    {

        if (FindObjectsByType<CardController>().Length != 1)
        {
            return;
        }

        GameObject.Find("Canvas").transform.Find("WinText").gameObject.SetActive(true);
    }

    private void StopCloseCoroutine()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }
    }

    private void StopFlipCoroutine()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        isFlipping = false;
    }

    private void OnDisable()
    {
        if (OpenCards.TryGetValue(groupId, out CardController openCard) && openCard == this)
        {
            OpenCards.Remove(groupId);
        }
        StopCloseCoroutine();
        StopFlipCoroutine();
    }
}
