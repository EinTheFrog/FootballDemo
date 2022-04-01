using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerBehaviour : MonoBehaviour
{
    [SerializeField] private Text textGoal;
    public void SetGoalText(int value)
    {
        textGoal.text = value.ToString();
    }
}
