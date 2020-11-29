using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPanel : MonoBehaviour
{
    public void DecideWinner(WinResult winner)
    {
        gameObject.SetActive(true);
        GameObject resultText = transform.Find("Result Text").gameObject;
        string winText = $"{winner.ToString()} is the winner!";
        if (winner == WinResult.DRAW)
            winText = $"It is a draw!";
        resultText.GetComponent<Text>().text = winText;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
