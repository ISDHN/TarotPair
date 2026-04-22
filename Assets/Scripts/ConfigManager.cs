using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigManager : MonoBehaviour
{
    private static int cardCount = 52;

    public static void StartGame(int cardCount)
    {
        Debug.Log("Starting game with " + cardCount + " cards.");
        ConfigManager.cardCount = cardCount;
        SceneManager.LoadScene("MainScene");
    }

    public static void ReturnToMenu()
    {
        SceneManager.LoadScene("StartScene");
    }

    public static int GetCardCount()
    {
        return cardCount;
    }
}
