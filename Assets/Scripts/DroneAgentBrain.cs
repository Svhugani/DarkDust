using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAgentBrain : MonoBehaviour
{
    private Vector3 _velocity;
    private Rigidbody _rigidbody;
    private float _angularSpeed = 500;
    private bool _isMoving = false;
    private float _hRot = 0;
    private float _vRot = 0;
    private bool _thrustOn = false;
    private float _thrustForce = 0;
    private float _engineAcceleration = 50000;
    //private float _engineAcceleration = 5;
    //private float _droneEnergyLevel = 1000;
    private float _energyPerOrb = 100;
    private float _droneEnergyLevel = 100;
    private float _droneHealth = 100;
    private int _droneID;
    private bool _droneIsAlive;
    private int[] _neuralStructure;
    private float[] _brainInputData = new float[13]; // input datta see neuralMovement
    private float[] _brainOutputData;
    private float _AgeOfDrone;
    private float _initTime;
    private float _ignoreDistance = 2000f;
    DroneDetector _energyDetector;
    DroneDetector _collisionDetector;
    [HideInInspector] public GeneticNeuralNetwork neuralBrain;
    private bool _allowToDestroy = false;
    private float _maxVelocity = 1000f;  

    public bool AllowToDestroy
    {
        get { return _allowToDestroy; }
        set { _allowToDestroy = value; }
    }

    public bool DroneIsAlive
    {
        get {return _droneIsAlive; }
    }

    public int DroneID
    {
        get { return _droneID; }
        set { _droneID = value; }
    }

    public float DroneEnrgyLevel
    {
        get { return _droneEnergyLevel; }
        set { _droneEnergyLevel = value; }
    }

    public float DroneHealth
    {
        get { return _droneHealth; }
        set { _droneHealth = value; }
    }


    void Awake()
    {
        //_neuralStructure = new int[] { 14, 16, 32, 16, 8, 3 };
        _neuralStructure = new int[] { 13, 64,  3 };
        neuralBrain = new GeneticNeuralNetwork(_neuralStructure);
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _droneIsAlive = true;
    }

    void Start()
    {
        _initTime = Time.time;
        _isMoving = true;
        _energyDetector = this.transform.Find("Detector").GetComponent<DroneDetector>();
        _collisionDetector = this.transform.Find("CollisionDetector").GetComponent<DroneDetector>();
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        _velocity = _rigidbody.velocity;
        _thrustOn = false;
        _thrustForce = 0;
        _rigidbody.AddRelativeForce(Vector3.Reflect(_rigidbody.velocity.normalized * _velocity.magnitude * _velocity.magnitude * 100, collision.GetContact(0).normal));
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("EnergyParticlesLayer"))
        {
            Destroy(collision.collider.gameObject);
            _droneEnergyLevel += _energyPerOrb;
            GameObject.Find("GameManager").GetComponent<GameManagerScript>().GenerateSingleOrb();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _droneHealth -= 5;
        }
    }

    void manualDroneMovement()
    {
        _hRot = Input.GetAxis("Vertical");
        _vRot = Input.GetAxis("Horizontal");

        if (Input.GetKey("space"))
        {
            _thrustOn = true;
        }
        else
        {
            _thrustOn = false;
        }

        if (Input.GetButton("Horizontal") && Input.GetButton("Vertical") && !Input.GetKey("space"))
        {
            _isMoving = false;
        }
        else
        {
            _isMoving = true;
        }
    }


    void NeuralDroneMovement()
    {
        _brainInputData[0] = _rigidbody.position.x;
        _brainInputData[1] = _rigidbody.position.y;
        _brainInputData[2] = _rigidbody.position.z;

        _brainInputData[3] = _rigidbody.velocity.x;
        _brainInputData[4] = _rigidbody.velocity.y;
        _brainInputData[5] = _rigidbody.velocity.z;

        if (_energyDetector.TargetObject != null)
        {
            _brainInputData[6] = _energyDetector.TargetObject.transform.position.x;
            _brainInputData[7] = _energyDetector.TargetObject.transform.position.y;
            _brainInputData[8] = _energyDetector.TargetObject.transform.position.z;
        }

        else
        {
            _brainInputData[6] = _ignoreDistance;
            _brainInputData[7] = _ignoreDistance;
            _brainInputData[8] = _ignoreDistance;
        }

        if (_collisionDetector.TargetObject != null)
        {
            _brainInputData[9] = _collisionDetector.TargetObject.transform.position.x;
            _brainInputData[10] = _collisionDetector.TargetObject.transform.position.y;
            _brainInputData[11] = _collisionDetector.TargetObject.transform.position.z;
        }

        else
        {
            _brainInputData[9] = _ignoreDistance;
            _brainInputData[10] = _ignoreDistance;
            _brainInputData[11] = _ignoreDistance;
        }


        _brainInputData[12] = _droneEnergyLevel;
        _brainInputData[13] = _droneHealth;

        neuralBrain.ForwardPropagation(_brainInputData);
        _brainOutputData = neuralBrain.OutputSignal();

        _hRot = _brainOutputData[0];
        _vRot = _brainOutputData[1];

        if (_brainOutputData[2] >= 0)
        {
            _thrustOn = true;
        }
        else
        {
            _thrustOn = false;
        }

        if (_vRot != 0 || _hRot != 0 || _thrustOn)
        {
            _isMoving = true;
        }

        else
        {
            _isMoving = false;
        }



    }

    void NeuralDroneMovement2()
    {
        Vector3 velocity = _rigidbody.velocity.normalized;

        _brainInputData[0] = velocity.x;
        _brainInputData[1] = velocity.y;
        _brainInputData[2] = velocity.z;

        if (_energyDetector.TargetObject != null)
        {
            Vector3 energyDirection = (_energyDetector.TargetObject.transform.position - _rigidbody.position).normalized;
            float energySpeedFactor = _rigidbody.velocity.magnitude / _maxVelocity;
            float energyDirectionFactor = Vector3.Dot(velocity, energyDirection);
            _brainInputData[3] = energyDirection.x;
            _brainInputData[4] = energyDirection.y;
            _brainInputData[5] = energyDirection.z;
            _brainInputData[6] = energySpeedFactor;
            _brainInputData[7] = energyDirectionFactor;
        }

        else
        {
            Vector3 randomDirection = Random.onUnitSphere;
            _brainInputData[3] = randomDirection.x;
            _brainInputData[4] = randomDirection.y;
            _brainInputData[5] = randomDirection.z;
            _brainInputData[6] = 0;
            _brainInputData[7] = 0;
        }

        if (_collisionDetector.TargetObject != null)
        {
            Vector3 collisionDirection = (_collisionDetector.TargetObject.transform.position - _rigidbody.position).normalized;
            float collisionSpeedFactor = _rigidbody.velocity.magnitude / _maxVelocity;
            float collisionDirectionFactor = Vector3.Dot(velocity, collisionDirection);
            _brainInputData[8] = collisionDirection.x;
            _brainInputData[9] = collisionDirection.y;
            _brainInputData[10] = collisionDirection.z;
            _brainInputData[11] = collisionSpeedFactor;
            _brainInputData[12] = collisionDirectionFactor;
        }

        else
        {
            _brainInputData[8] = -velocity.x;
            _brainInputData[9] = -velocity.y;
            _brainInputData[10] = -velocity.z;
            _brainInputData[11] = 0;
            _brainInputData[12] = 0;
        }


        neuralBrain.ForwardPropagation(_brainInputData);
        _brainOutputData = neuralBrain.OutputSignal();

        _hRot = _brainOutputData[0];
        _vRot = _brainOutputData[1];

        if (_brainOutputData[2] >= 0)
        {
            _thrustOn = true;
        }
        else
        {
            _thrustOn = false;
        }

        if (_vRot != 0 || _hRot != 0 || _thrustOn)
        {
            _isMoving = true;
        }

        else
        {
            _isMoving = false;
        }

    }

    void UpdateEnergyLevel()
    {
        if (_isMoving)
        {   
            _droneEnergyLevel -= 10 * Time.fixedDeltaTime; 
            _droneEnergyLevel -= 0.001f * _thrustForce * Time.fixedDeltaTime;
        }
        else
        {
            _droneEnergyLevel -= 20000 * Time.fixedDeltaTime;
        }

    }


    void CheckDroneStatus()
    {
        if (_droneIsAlive)
        {
            UpdateEnergyLevel();
            

            if (_droneEnergyLevel < 10)
            {
                _rigidbody.useGravity = true;
            }
            else
            {
                _rigidbody.useGravity = false;
            }

            if (_droneEnergyLevel < 0 || _droneHealth < 0)
            {
                _droneIsAlive = false;
                _AgeOfDrone = Time.time - _initTime;
                neuralBrain.SetFitness(_AgeOfDrone);
            }
            else if (Mathf.Abs(_rigidbody.position.x) > 200 || Mathf.Abs(_rigidbody.position.z) > 200 || Mathf.Abs(_rigidbody.position.y) > 50)
            {
                //_droneIsAlive = false;
                //_AgeOfDrone = Time.time - _initTime;
                //neuralBrain.SetFitness(_AgeOfDrone);
            }
        }

        if (_allowToDestroy)
        {
            Destroy(gameObject, 2f);
        }

    }

    void Update()
    {
        //manualDroneMovement();
        //NeuralDroneMovement();
    }

    private void FixedUpdate()
    {
        NeuralDroneMovement2();

        if (_isMoving)
        {
            if (_rigidbody.velocity.magnitude > _maxVelocity)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * _maxVelocity; 
            }
            _velocity = _rigidbody.velocity.normalized;

            if (_thrustOn)
            {
                _thrustForce += _engineAcceleration * Time.fixedDeltaTime;
                _rigidbody.AddRelativeForce(Vector3.forward * _thrustForce);
            }

            else
            {
                _thrustForce = 0;
            }

            _rigidbody.AddRelativeTorque(Vector3.up * _angularSpeed * _vRot);
            _rigidbody.AddRelativeTorque(-Vector3.right * _angularSpeed * _hRot);
            
        }

        CheckDroneStatus();
    }


}
