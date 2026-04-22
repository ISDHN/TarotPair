using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    private const int RowCount = 4;
    private const int TotalAvailableFaceCount = 22;
    private const float CardFaceHeightPixels = 1920f;
    private const float PaddingRatio = 0.1f;

    void Start()
    {
        Camera mainCamera = Camera.main;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        int groupCount = ConfigManager.GetCardCount();
        if (groupCount > TotalAvailableFaceCount)
        {
            Debug.LogError($"Card group count ({groupCount}) exceeds available face count ({TotalAvailableFaceCount}).");
            return;
        }

        int totalCardCount = groupCount * 2;
        int columnCount = Mathf.CeilToInt(totalCardCount / (float)RowCount);
        List<Sprite> groupFaceSprites = BuildSelectedGroupFaceSprites(groupCount);
        List<int> shuffledPairIds = BuildShuffledPairIds(groupCount);

        float cellWidth = screenWidth * (1 - PaddingRatio * 2) / columnCount;
        float cellHeight = screenHeight / RowCount;
        float zDistance = -mainCamera.transform.position.z;
        float worldUnitsPerScreenPixel = mainCamera.orthographicSize * 2f / screenHeight;
        float targetCardWorldHeight = cellHeight * worldUnitsPerScreenPixel;

        for (int i = 0; i < totalCardCount; i++)
        {
            int row = i % RowCount;
            int col = i / RowCount;

            float screenX = (col + 0.5f) * cellWidth + screenWidth * PaddingRatio;
            float screenY = screenHeight - (row + 0.5f) * cellHeight;

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenX, screenY, zDistance));
            worldPosition.z = 0f;

            GameObject card = Instantiate(cardPrefab, worldPosition, Quaternion.identity);
            int groupId = shuffledPairIds[i];
            card.name = $"Card_{groupId}_{i}";
            card.GetComponent<CardController>().Initialize(groupId, groupFaceSprites[groupId]);

            float sourceCardWorldHeight = CardFaceHeightPixels / groupFaceSprites[groupId].pixelsPerUnit;
            float cardScale = targetCardWorldHeight / sourceCardWorldHeight * 0.9f;
            card.transform.localScale = Vector3.one * cardScale;
        }
    }

    private static List<Sprite> BuildSelectedGroupFaceSprites(int groupCount)
    {
        List<int> allFaceNumbers = new(TotalAvailableFaceCount);
        for (int i = 0; i < TotalAvailableFaceCount; i++)
        {
            allFaceNumbers.Add(i);
        }

        ShuffleList(allFaceNumbers);

        List<Sprite> selectedSprites = new(groupCount);
        for (int i = 0; i < groupCount; i++)
        {
            int faceNumber = allFaceNumbers[i];
            Sprite faceSprite = Resources.Load<Sprite>($"Images/Tarot/{faceNumber}");
            selectedSprites.Add(faceSprite);
        }

        return selectedSprites;
    }

    private static List<int> BuildShuffledPairIds(int groupCount)
    {
        List<int> pairIds = new(groupCount * 2);
        for (int i = 0; i < groupCount; i++)
        {
            pairIds.Add(i);
            pairIds.Add(i);
        }

        ShuffleList(pairIds);

        return pairIds;
    }

    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[swapIndex], list[i]) = (list[i], list[swapIndex]);
        }
    }
}
