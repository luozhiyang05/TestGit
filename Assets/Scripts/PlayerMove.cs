using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMove : MonoBehaviour
{
    public event UnityAction moveEvent;
    
    private void Update()
    {
        moveEvent?.Invoke();
    }
}