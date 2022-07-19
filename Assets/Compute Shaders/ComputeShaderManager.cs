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
    public Vector3[] _output;

    private void Awake()
    {
        ComputeShaderManager.instance = this;
    }

    public void SetUpComputeShader()
    {
        instance = this;
        _kernel = physicsComputeShader.FindKernel("Forces");
        physicsComputeShader.GetKernelThreadGroupSizes(_kernel, out _threadGroupSize, out _, out _);
        _resultBuffer = new ComputeBuffer(BodyManager.instance.amountOfBodies, sizeof(float) * 3);
        _positionBuffer = new ComputeBuffer(BodyManager.instance.amountOfBodies, sizeof(float) * 3);
        _massBuffer = new ComputeBuffer(BodyManager.instance.amountOfBodies, sizeof(float) * 1);
        _output = new Vector3[BodyManager.instance.amountOfBodies];
        physicsComputeShader.SetInt("Amount", BodyManager.instance.amountOfBodies);
        physicsComputeShader.SetFloat("G", BodyManager.instance.G);
        physicsComputeShader.SetBuffer(_kernel, "Positions", _positionBuffer);
        physicsComputeShader.SetBuffer(_kernel, "Mass", _massBuffer);
    }

    public void CalculateGravity()
    {
        physicsComputeShader.SetBuffer(_kernel, "Result", _resultBuffer);
        var positions = new Vector3[BodyManager.instance.amountOfBodies];
        var mass = new float[BodyManager.instance.amountOfBodies];
        var i = 0;
        foreach (var body in BodyManager.instance.bodies)
        {
            positions[i] = body.position;
            mass[i] = body.mass;
            ++i;
        }

        _positionBuffer.SetData(positions);
        _massBuffer.SetData(mass);
        var threadGroups = (int) ((BodyManager.instance.amountOfBodies + (_threadGroupSize - 1)) / _threadGroupSize);
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
                Debug.Log(_output[j]);
            }
        }
    }
}