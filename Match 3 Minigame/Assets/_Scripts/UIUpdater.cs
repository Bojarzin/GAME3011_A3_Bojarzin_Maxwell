using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIUpdater : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text movesText;
    public TMP_Text finalScoreText;
    public TMP_Text secondsText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + BoardManager.Instance.score;
        movesText.text = "Moves " + BoardManager.Instance.movesRemaining;

        finalScoreText.text = "Final Score: " + scoreText.text;

        secondsText.text = "Seconds Remaining: " + (int)BoardManager.Instance.seconds;
    }
}
