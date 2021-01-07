using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneDetector : MonoBehaviour
{

    private List<Collider> _triggerList = new List<Collider>();
    private Collider _targetObject;
    public string DetectLayer;


    public Collider TargetObject
    {
        get { return _targetObject; }      
    }


    void Start()
    {

    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(DetectLayer))
        {
            if (!_triggerList.Contains(other))
            {
                _triggerList.Add(other);
                //Debug.Log("object in " + DetectLayer);
            }
        }

       
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(DetectLayer))
        {
            if (_triggerList.Contains(other))
            {
                _triggerList.Remove(other);
                //Debug.Log("object left " + DetectLayer);
            }
        }


    }

    private void DroneVision()
    {
        if (_triggerList.Count != 0)
        {
            _targetObject = _triggerList[0];
        }

    }

    void FixedUpdate()
    {
        DroneVision();
    }
}
