using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public GameObject energyPrefab;
    public GameObject droneAgentPrefab;
    [SerializeField] private int _numberOfEnergyOrbs;
    [SerializeField] private int _numberOfDroneAgents;
    private Vector3 _orbInitPosition;
    private Vector3 _droneAgentInitPosition;
    private bool _releaseDroneAllowed = true;
    private int _droneAgentCount = 0;
    private int _idNumber = 0;
    private List<DroneAgentBrain> _listOfDroneAgentBrains = new List<DroneAgentBrain>();
    private float _minFitness = 0;
    private int _numberOfTopNeuralNets = 10;
    private List<GeneticNeuralNetwork> _topNeuralNets = new List<GeneticNeuralNetwork>();
    private bool _isInitialDataCollected = false;
    private int _rateSwitcher = 1;
    private float _releaseDroneFreq = 2f;

    private void Awake()
    {
        //_numberOfTopNeuralNets = Mathf.Max(10, (int)(0.2f * _numberOfDroneAgents));
        //Debug.Log("TOP NEURAL NETS NUMBER: " + _numberOfTopNeuralNets.ToString());
        GenerateOrbs();
    }

    void Start()
    {
        InvokeRepeating("PrintFitness", 2f, 5f);
    }

    void SwitchDroneReleaseRate()
    {
        if (Input.GetKeyDown("q"))
        {
            _rateSwitcher *= -1;
            if (_rateSwitcher == 1)
            {
                Debug.Log("Current release freq: low ");
            }
            else
            {
                Debug.Log("Current release freq: high ");
            }
        }

        if (_rateSwitcher == 1)
        {
            _releaseDroneFreq = 2f;
        }
        else
        {
            _releaseDroneFreq = 0.1f;
        }
    }

    private void GenerateOrbs()
    {
        for ( int i =0; i < _numberOfEnergyOrbs; i++ )
        {
            GenerateSingleOrb();
        }
    }

    public void GenerateSingleOrb()
    {
        _orbInitPosition = new Vector3(Random.Range(-50f, 50f), Random.Range(22f, 25f), Random.Range(-50, 50f));
        Instantiate(energyPrefab, _orbInitPosition, Quaternion.identity);
    }

    void GenerateDroneAgents()
    {
        if (_releaseDroneAllowed  && _droneAgentCount < _numberOfDroneAgents)
        {
            ReleaseDrone();
            Invoke("AllowToReleaseDrone", _releaseDroneFreq);
        }
        
    }

    void AllowToReleaseDrone()
    {
        _releaseDroneAllowed = true;
    }

    void ReleaseDrone()
    {       
        Vector3 randomPosition = 40 * Random.onUnitSphere;
        randomPosition.y = Random.Range(22f, 27f);
        _droneAgentInitPosition =  randomPosition;
        GameObject _droneAgent = Instantiate(droneAgentPrefab, _droneAgentInitPosition, Quaternion.identity) as GameObject;
        DroneAgentBrain _droneAgentBrain = _droneAgent.GetComponentInChildren<DroneAgentBrain>();
        if (_isInitialDataCollected)
        {
            _droneAgentBrain.neuralBrain = _droneAgentBrain.neuralBrain.GeneticCross(_topNeuralNets[Random.Range(0, _topNeuralNets.Count - 1)], _topNeuralNets[Random.Range(0, _topNeuralNets.Count - 1)]);
            //Debug.Log(" new neuralnetwork from data created , fitness: " + _droneAgentBrain.neuralBrain.GetFitness().ToString());
        }
        _droneAgentBrain.DroneID = _idNumber;
        _listOfDroneAgentBrains.Add(_droneAgentBrain);
        _droneAgentCount++;
        _idNumber++;
        _releaseDroneAllowed = false;
    }

    void SortTopNeuralNets()
    {
        if (_topNeuralNets.Count > 1)
        {
            _topNeuralNets.Sort(delegate (GeneticNeuralNetwork a, GeneticNeuralNetwork b)
            {
                return (a.GetFitness()).CompareTo(b.GetFitness());
            });
        }
    }


    void StoreNeuralNet(GeneticNeuralNetwork neuralBrain)
    {
        
        if (_topNeuralNets.Count < _numberOfTopNeuralNets)
        {
            GeneticNeuralNetwork net = new GeneticNeuralNetwork(neuralBrain);
            net.SetFitness(neuralBrain.GetFitness());
            _topNeuralNets.Add(net);
            SortTopNeuralNets();
            _minFitness = _topNeuralNets[0].GetFitness();
        }

        else
        {
            if (neuralBrain.GetFitness() > _minFitness)
            {
                GeneticNeuralNetwork net = new GeneticNeuralNetwork(neuralBrain);
                net.SetFitness(neuralBrain.GetFitness());
                _topNeuralNets[0] = net;
                SortTopNeuralNets();
                _minFitness = _topNeuralNets[0].GetFitness();
                _isInitialDataCollected = true;
            }
        }
    }

    void UpdateDrones()
    {
        if (_listOfDroneAgentBrains.Count >0)
        {
            for (int i = _listOfDroneAgentBrains.Count - 1; i >= 0; i--)
            {
                if (!_listOfDroneAgentBrains[i].DroneIsAlive)
                {
                    StoreNeuralNet(_listOfDroneAgentBrains[i].neuralBrain);
                    _listOfDroneAgentBrains[i].AllowToDestroy = true;
                    _listOfDroneAgentBrains.RemoveAt(i);
                    _droneAgentCount--;
                }
            }
        }
     
    }

    void Debugger()
    {

        if (_topNeuralNets.Count >4)
        {

            Debug.Log(_topNeuralNets[0].GetFitness().ToString() + " and " + _topNeuralNets[1].GetFitness().ToString() + " and " + _topNeuralNets[_topNeuralNets.Count-3].GetFitness().ToString() + " and " + _topNeuralNets[_topNeuralNets.Count-2].GetFitness().ToString() + " and " + _topNeuralNets[_topNeuralNets.Count-1].GetFitness().ToString());
            //Debug.Log(_listOfDroneAgentBrains[0].neuralBrain.GetFitness().ToString() + " and " + _listOfDroneAgentBrains[1].neuralBrain.GetFitness().ToString() + " and " + _listOfDroneAgentBrains[2].neuralBrain.GetFitness().ToString());
        }
    }    
    void PrintFitness()
    {
        Debug.Log(" min top fitness is: " + _minFitness.ToString() + " and max top fitness is: " +  _topNeuralNets[_topNeuralNets.Count-1].GetFitness().ToString());
    }

    void Update()
    {
        SwitchDroneReleaseRate();
        GenerateDroneAgents();
        UpdateDrones();

        //Debugger();
    }
}
