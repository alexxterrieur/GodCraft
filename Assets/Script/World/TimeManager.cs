using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("World Speed")]
    public int timeSpeed;
    public float monthDuration;
    private int currentYear;
    private int currentMonth;
    public TMP_Text timeDisplay;

    private void Start()
    {
        StartCoroutine(UpdateTime());
    }

    IEnumerator UpdateTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(monthDuration / timeSpeed);

            currentMonth++;
            if (currentMonth > 11)
            {
                currentMonth = 0;
                currentYear++;
            }

            UpdateTimeUI();
        }
    }

    void UpdateTimeUI()
    {
        timeDisplay.text = currentYear + " years  " + currentMonth + " months";
    }

    public void ChangeTimeSpeed(int _timeSpeed)
    {
        timeSpeed = _timeSpeed;
    }

    public (int year, int month) GetDate()
    {
        return (currentYear, currentMonth);
    }
}
