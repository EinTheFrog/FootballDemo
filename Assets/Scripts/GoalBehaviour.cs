using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour
{
    private UIManagerBehaviour _goalText;
    private int _value = 0;
    private void Start()
    {
        _goalText = FindObjectOfType<UIManagerBehaviour>();
        _goalText.SetGoalText(_value);
    }

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponent<BallBehaviour>();
        if (ball == null) return;

        if (ball.isValid)
        {
            _value++;
            _goalText.SetGoalText(_value);
            ball.isValid = false;
        }
        
        
        //Debug.Log("GoalBehaviour: on trigger enter");
    }
}
