using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("World Speed")]
    public int timeSpeed = 1;
    public float monthDuration = 30f;
    private int currentYear;
    private int currentMonth;
    public TMP_Text timeDisplay;

    private List<HumanTimeManager> humans = new List<HumanTimeManager>();

    private void Start()
    {
        StartCoroutine(UpdateTime());
    }

    public void RegisterHuman(HumanTimeManager human)
    {
        if (!humans.Contains(human))
        {
            humans.Add(human);
        }
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
        timeDisplay.text = $"{currentYear} years  {currentMonth} months";
    }

    public void ChangeTimeSpeed(int newTimeSpeed)
    {
        timeSpeed = newTimeSpeed;

        foreach (var human in humans)
        {
            human.SetHumanSpeed(newTimeSpeed);
        }
    }

    public (int year, int month) GetDate()
    {
        return (currentYear, currentMonth);
    }
}
