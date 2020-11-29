using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPanel : MonoBehaviour
{
    public void DecideWinner(PieceType winner)
    {
        gameObject.SetActive(true);
        GameObject resultText = transform.Find("Result Text").gameObject;
        resultText.GetComponent<Text>().text = $"{winner.ToString()} is the winner!";
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
