using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BodyManager : MonoBehaviour
{
    public static BodyManager instance;
    [Header("References")]
    public bool useComputeShader;
    public GameObject bodyPrefab;
    public SimulationParameters simulationParameters;
    public List<Rigidbody> bodies = new();

    [Header("UI")]
    public RectTransform centerOfMassIcon;
    public RectTransform canvas;



    private bool _simulationRunning = false;

    private void Awake()
    {
        BodyManager.instance = this;
    }


    public void StartSimulation(SimulationParameters parameters)
    {
        this.simulationParameters = parameters;
        for (int id = 0; id < simulationParameters.amountOfBodies; ++id)
        {
            SpawnBody(id);
        }

        ComputeShaderManager.instance.SetUpComputeShader();
        _simulationRunning = true;
    }

    private void FixedUpdate()
    {
        if (!_simulationRunning)
        {
            return;
        }
        if (useComputeShader)
        {
            ComputeShaderManager.instance.CalculateGravity();
        }
        else
        {
            CalculateGravity();
        }

        CalculateCenterOfMass();
    }


    private void CalculateGravity()
    {
        foreach (var body in bodies)
        {
            CalculateGravityForBody(body);
        }
    }

    private void CalculateGravityForBody(Rigidbody body)
    {
        Vector3 finalForce = new Vector3();
        foreach (var foreignBody in bodies)
        {
            Vector3 direction = foreignBody.position - body.position;
            var squareMagnitude = direction.sqrMagnitude;
            if (squareMagnitude == 0)
            {
                continue;
            }

            var magnitude = simulationParameters.G * body.mass * foreignBody.mass / squareMagnitude;
            finalForce += magnitude * direction.normalized;
        }

        SetGravityForBody(body, finalForce);
    }

    public void SetGravityForBody(Rigidbody body, Vector3 gravity)
    {
        try
        {
            body.AddForce(gravity);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void CalculateCenterOfMass()
    {
        float totalMass = 0;
        Vector3 position = new Vector3(0, 0, 0);
        foreach (var body in bodies)
        {
            totalMass += body.mass;
            position += body.position * body.mass;
        }

        position /= totalMass;
        Vector2 viewportPoint = CameraManager.instance.GetCamera().WorldToViewportPoint(position);
        viewportPoint.x *= canvas.rect.width;
        viewportPoint.y *= canvas.rect.height;
        centerOfMassIcon.anchoredPosition = viewportPoint;
    }

    private void SpawnBody(int id)
    {
        float posX = Random.Range(-1.0f, 1.0f);
        float posY = Random.Range(-1.0f, 1.0f);
        Vector3 position = new Vector3(posX, posY, 0) * simulationParameters.spreadModifier;

        GameObject body = Instantiate(bodyPrefab, position, Quaternion.identity);

        float velX = Random.Range(-1.0f, 1.0f);
        float velY = Random.Range(-1.0f, 1.0f);
        Vector3 velocity = new Vector3(velX, velY, 0) * simulationParameters.velocityModifier;

        Rigidbody rBody = body.GetComponent<Rigidbody>();
        rBody.velocity = velocity;

        float mass = Random.Range(simulationParameters.minMass, simulationParameters.maxMass);
        rBody.mass = mass;
        float massScale = (mass - simulationParameters.minMass) / (simulationParameters.maxMass - simulationParameters.minMass);
        body.transform.localScale = Vector3.one * (1 + massScale) * simulationParameters.sizeModifier;

        float green = 1 - massScale;
        Color color = new Color(1.0f, green, 0);
        body.GetComponent<MeshRenderer>().material.color = color;

        color.a = 0.5f;
        body.GetComponent<TrailRenderer>().material.color = color;

        body.transform.SetParent(this.transform);
        body.name = id.ToString();
        bodies.Add(rBody);
    }

    public bool IsSimulationRunning()
    {
        return _simulationRunning;
    }
}