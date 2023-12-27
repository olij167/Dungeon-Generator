using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chance
{
    public static bool Odds(float maxOdds) // one out of maxOdds chance to be true - else false
    {
        float r = Random.Range(0, maxOdds);

        if (r == 0) return true;
        else return false;
    }

    public static bool CoinFlip() // 50 / 50 chance
    {
        float r = Random.Range(0, 2);

        if (r == 0) return true;
        else return false;
    }

    public static float NumberPicker(float maxNumber) // return a random number between 0 and maxNumber
    {
        return Random.Range(0, maxNumber);
    }

    public static bool Percentage(float chancePercentage) // get a random number between 0 & 100, if it is lower than chancePercentage return true, else return false
    {
        Mathf.Clamp(chancePercentage, 0f, 100f);

        if (Random.Range(0f, 100f) <= chancePercentage)
        {
            return true;
        }
        else return false;
    }
}
