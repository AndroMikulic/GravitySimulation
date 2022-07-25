using System.IO;
using UnityEngine;
using TMPro;

public class SimulationParametersManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField amountOfBodiesInput;
    public TMP_InputField maxMassInput;
    public TMP_InputField sizeModifierInput;
    public TMP_InputField spreadModifierInput;
    public TMP_InputField velocityModifierInput;
    public TMP_InputField gInput;

    [Header("Data")]
    public SimulationParameters simulationParameters;

    string path;

    void Start()
    {
        path = Application.persistentDataPath + "/simulationParameters.json";
        try
        {
            LoadValues();
        }
        catch
        {
            simulationParameters = new SimulationParameters();
            SaveValues();
        }
        SetInputValues();
    }

    public void SaveValues()
    {
        string json = JsonUtility.ToJson(simulationParameters);
        File.WriteAllText(path, json);
    }

    public void LoadValues()
    {
        string json = File.ReadAllText(path);
        simulationParameters = JsonUtility.FromJson<SimulationParameters>(json);
    }

    public void SetInputValues()
    {
        try
        {
            amountOfBodiesInput.text = simulationParameters.amountOfBodies.ToString();
            maxMassInput.text = simulationParameters.maxMass.ToString();
            sizeModifierInput.text = simulationParameters.sizeModifier.ToString();
            spreadModifierInput.text = simulationParameters.spreadModifier.ToString();
            velocityModifierInput.text = simulationParameters.velocityModifier.ToString();
            gInput.text = simulationParameters.G.ToString();
        }
        catch
        {
            simulationParameters = new SimulationParameters();
            SaveValues();
            SetInputValues();
        }
    }

    public void ReadInputValues()
    {
        simulationParameters.amountOfBodies = Mathf.Max(int.Parse(amountOfBodiesInput.text), 2);
        simulationParameters.maxMass = Mathf.Max(float.Parse(maxMassInput.text), simulationParameters.minMass);
        simulationParameters.sizeModifier = Mathf.Max(float.Parse(sizeModifierInput.text), 0.1f);
        simulationParameters.spreadModifier = Mathf.Max(float.Parse(spreadModifierInput.text), 16);
        simulationParameters.velocityModifier = float.Parse(velocityModifierInput.text);
        simulationParameters.G = float.Parse(gInput.text);
        SaveValues();
    }

    public void StartSimulation()
    {
        BodyManager.instance.StartSimulation(simulationParameters);
    }

    public void ResetParameters()
    {
        simulationParameters = new SimulationParameters();
        SetInputValues();
        SaveValues();
    }
}
