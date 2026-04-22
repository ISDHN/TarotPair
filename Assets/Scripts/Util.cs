using System;
using UnityEngine;

public class Util : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void SwitchPanel(string current, string next)
    {
        gameObject.transform.Find(current).gameObject.SetActive(false);
        gameObject.transform.Find(next).gameObject.SetActive(true);
    }
}
