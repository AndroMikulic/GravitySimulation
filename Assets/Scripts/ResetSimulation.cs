using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResetSimulation : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction resetSimulation;

    void OnEnable()
    {
        resetSimulation.Enable();
    }

    void OnDisable()
    {
        resetSimulation.Disable();
    }

    void Update()
    {
        if (resetSimulation.triggered)
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }
}
