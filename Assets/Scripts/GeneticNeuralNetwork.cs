using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneticNeuralNetwork : IComparable<GeneticNeuralNetwork>
{
    private int[] _layersStructure;
    private float[][] _neurons;
    private float[][][] _weights;
    private float _fitnessValue;
    private float _randomGeneratorShift = .5f;
    private float _randomGeneratorScaling = 1f;
    //private float[] _mutationProbs = new float[] { 2, 4, 6, 8, 10 };
    private float[] _mutationProbs = new float[] { 15, 40, 60, 90, 150 };


    private float _neuronBias = 0.0f;

    public GeneticNeuralNetwork(int[] layersStructure)
    {
        this._layersStructure = new int[layersStructure.Length];

        for (int i = 0; i < layersStructure.Length; i++)
        {
            this._layersStructure[i] = layersStructure[i];
        }
        _fitnessValue = 0;
        InitNeurons();
        InitWeights();

    }


    public GeneticNeuralNetwork( GeneticNeuralNetwork originalNetwork)
    {
        this._layersStructure = new int[originalNetwork._layersStructure.Length];

        for (int i = 0; i < _layersStructure.Length; i++)
        {
            this._layersStructure[i] = originalNetwork._layersStructure[i];
        }

        _fitnessValue = 0;
        InitNeurons();
        InitWeights();
        CopyWeights(originalNetwork._weights);

    }

    public void CopyWeights(float[][][] weights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    _weights[i][j][k] = weights[i][j][k];
                }
            }
        }
    }


    public GeneticNeuralNetwork GeneticCross(GeneticNeuralNetwork net1, GeneticNeuralNetwork net2)
    {
        GeneticNeuralNetwork crossNetwork = new GeneticNeuralNetwork(net1._layersStructure);
        float fitness1 = net1.GetFitness();
        float fitness2 = net2.GetFitness();

        for (int i = 0; i < crossNetwork._weights.Length; i++)
        {
            for (int j = 0; j < crossNetwork._weights[i].Length; j++)
            {
                for (int k = 0; k < crossNetwork._weights[i][j].Length; k++)
                {
                    if (UnityEngine.Random.Range(0 , fitness1 + fitness2) < fitness1)
                    {
                        crossNetwork._weights[i][j][k] = net1._weights[i][j][k];
                    }

                    else
                    {
                        crossNetwork._weights[i][j][k] = net2._weights[i][j][k];
                    }
                    
                }
            }
        }

        crossNetwork.Mutate();

        return crossNetwork;

    }


    private void InitWeights()
    {
        List<float[][]> listOfLayerWeights = new List<float[][]>();

        for (int i = 1; i < _layersStructure.Length; i++)
        {
            List<float[]> layerWeights = new List<float[]>();
            int neuronsInPrevLayer = _layersStructure[i - 1];

            for (int j = 0; j < _layersStructure[i]; j++)
            {
                float[] weightsInOneNeuron = new float[neuronsInPrevLayer];
                
                for ( int k = 0; k < neuronsInPrevLayer; k++)
                {
                    weightsInOneNeuron[k] = _randomGeneratorScaling * ( UnityEngine.Random.Range(0f, 1f) -_randomGeneratorShift );
                }

                layerWeights.Add(weightsInOneNeuron);

            }

            listOfLayerWeights.Add(layerWeights.ToArray());

        }

        _weights = listOfLayerWeights.ToArray();

    }

    private void InitNeurons()
    {
        List<float[]> listOfNeurons = new List<float[]>();

        for (int i = 0; i < _layersStructure.Length; i++)
        {
            listOfNeurons.Add(new float[_layersStructure[i]]);
        }

        _neurons = listOfNeurons.ToArray();
    }

    public void Mutate()
    {
        for (int i = 0; i < _weights.Length; i++)
        {
            for (int j = 0; j < _weights[i].Length; j++)
            {
                for (int k = 0; k < _weights[i][j].Length; k++)
                {
                    float mutateWeight = _weights[i][j][k];
                    float randomMutation = UnityEngine.Random.Range(0, 1000);

                    if (randomMutation <= _mutationProbs[0])
                    {
                        mutateWeight *= -1;
                    }

                    else if (randomMutation <= _mutationProbs[1])
                    {
                        mutateWeight = UnityEngine.Random.Range(-1f, 1f);
                    }

                    else if (randomMutation <= _mutationProbs[2])
                    {
                        mutateWeight *= UnityEngine.Random.Range(1f, 2f); 
                    }

                    else if (randomMutation <= _mutationProbs[3])
                    {
                        mutateWeight *= UnityEngine.Random.Range(-.5f, .5f);
                    }

                    else if (randomMutation <= _mutationProbs[4])
                    {
                        mutateWeight *= UnityEngine.Random.Range(0, 1f);
                    }

                    _weights[i][j][k] = mutateWeight;

                }
            }
        }
    }


    public void ForwardPropagation(float[] inputData)
    {
        for (int i = 0; i < inputData.Length; i++)
        {
            _neurons[0][i] = inputData[i];
        }

        for (int i = 1; i < _layersStructure.Length; i++)
        {
            for (int j = 0; j < _neurons[i].Length; j++)
            {
                float excitationLevel = _neuronBias;

                for (int k = 0; k < _neurons[i-1].Length; k++ )
                {
                    excitationLevel += _weights[i-1][j][k] * _neurons[i-1][k];
                }

                _neurons[i][j] = (float) Math.Tanh(excitationLevel);

            }
        }

    }

    public float[] OutputSignal()
    {
        float[] output = new float[_layersStructure[_layersStructure.Length-1]];
        for (int i = 0; i<_neurons[_layersStructure.Length-1].Length; i++)
        {
            output[i] = _neurons[_layersStructure.Length-1][i];
        }

        return output;
    }

    public void SetFitness(float modify)
    {
        _fitnessValue = modify;
    }

    public float GetFitness()
    {
        return _fitnessValue;
    }

    public int CompareTo(GeneticNeuralNetwork otherNetwork)
    {
        if (otherNetwork == null)
        {
            return 1;
        }

        else if  (_fitnessValue > otherNetwork._fitnessValue)
        {
            return 1;
        }

        else if (_fitnessValue < otherNetwork._fitnessValue)
        {
            return -1;
        }

        else
        {
            return 0;
        }

    }
}
