using System;
using UnityEngine;

public class ComputeShaderManager : MonoBehaviour
{
    public static ComputeShaderManager instance;
    public ComputeShader physicsComputeShader;
    ComputeBuffer _resultBuffer;
    ComputeBuffer _positionBuffer;
    ComputeBuffer _massBuffer;
    int _kernel;
    uint _threadGroupSize;
    Vector3[] _output;

    private void Awake()
    {
        ComputeShaderManager.instance = this;
    }

    void OnDisable()
    {
        try
        {
            _massBuffer.Release();
            _resultBuffer.Release();
            _positionBuffer.Release();
        }
        catch { }
    }

    public void SetUpComputeShader()
    {
        instance = this;
        _kernel = physicsComputeShader.FindKernel("Forces");
        physicsComputeShader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSize, out _, out _);
        _resultBuffer = new ComputeBuffer(BodyManager.instance.simulationParameters.amountOfBodies, sizeof(float) * 3);
        _positionBuffer = new ComputeBuffer(BodyManager.instance.simulationParameters.amountOfBodies, sizeof(float) * 3);
        _massBuffer = new ComputeBuffer(BodyManager.instance.simulationParameters.amountOfBodies, sizeof(float) * 1);
        _output = new Vector3[BodyManager.instance.simulationParameters.amountOfBodies];
        physicsComputeShader.SetInt("Amount", BodyManager.instance.simulationParameters.amountOfBodies);
        physicsComputeShader.SetFloat("G", BodyManager.instance.simulationParameters.G);
        physicsComputeShader.SetBuffer(_kernel, "Positions", _positionBuffer);
        physicsComputeShader.SetBuffer(_kernel, "Mass", _massBuffer);
    }

    public void CalculateGravity()
    {
        physicsComputeShader.SetBuffer(_kernel, "Result", _resultBuffer);
        var positions = new Vector3[BodyManager.instance.simulationParameters.amountOfBodies];
        var mass = new float[BodyManager.instance.simulationParameters.amountOfBodies];
        var i = 0;
        foreach (var body in BodyManager.instance.bodies)
        {
            positions[i] = body.position;
            mass[i] = body.mass;
            ++i;
        }

        _positionBuffer.SetData(positions);
        _massBuffer.SetData(mass);
        var threadGroups = (int)((BodyManager.instance.simulationParameters.amountOfBodies + (_threadGroupSize - 1)) / _threadGroupSize);
        physicsComputeShader.Dispatch(_kernel, threadGroups, 1, 1);
        _resultBuffer.GetData(_output);
        for (var j = 0; j < BodyManager.instance.bodies.Count; ++j)
        {
            try
            {
                BodyManager.instance.SetGravityForBody(BodyManager.instance.bodies[j], _output[j]);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}