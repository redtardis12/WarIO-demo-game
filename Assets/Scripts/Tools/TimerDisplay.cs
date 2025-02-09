using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public TMP_Text timerText; // Reference to the TextMeshPro UI Text
    private float startTime;

    void Start()
    {
        startTime = Time.time; // Record the start time
    }

    void Update()
    {
        // Calculate the elapsed time
        float elapsedTime = Time.time - startTime;

        // Update the text to display the elapsed time in seconds with 2 decimal places
        timerText.text = elapsedTime.ToString("F2");
    }
}