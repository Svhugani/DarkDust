using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnergiParticleMovement : MonoBehaviour
{
    private Rigidbody _rigidbody;   
    private Vector3 _velocity;
    private Vector3 _initPosition;
    private bool _isMoving = false;
    private bool _isFadeIn = true;
    private Material _objectMaterial;
    [SerializeField] private float _speedOfParticle = 30f;
    private float _fadeSpeed = .4f;
    [SerializeField] private Vector3 _centralPosition = new Vector3(0, 25f, 0);


    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _objectMaterial = GetComponent<Renderer>().material;
        _objectMaterial.SetColor("_Color", new Color(_objectMaterial.color.r, _objectMaterial.color.g, _objectMaterial.color.b, 0));

        Invoke(nameof(Launch), 2f);

        if (_rigidbody.position != null)
        {
            _initPosition = _rigidbody.position;
        }
        else
        {
            _initPosition = _centralPosition;
        }
    }

    void Launch()
    {
        _rigidbody.velocity = new Vector3(0, 1f, 0) * _speedOfParticle;
        _isMoving = true;
       
    }

    void FadeIn()
    {
        if (_isFadeIn)
        {
            float _fadeLevel = _objectMaterial.color.a +_fadeSpeed * Time.deltaTime;

            _objectMaterial.SetColor("_Color",  new Color(_objectMaterial.color.r, _objectMaterial.color.g, _objectMaterial.color.b, _fadeLevel));

            if (_objectMaterial.color.a >= 1)
            {
                _objectMaterial.EnableKeyword("_EMISSION");
                _isFadeIn = false;
            }
        }

    }

    private float GaussianNormal()
    {
        //Box-Muller transform 
        float r1 = Random.Range(0.001f, 1f);
        float r2 = Random.Range(0.001f, 1f);

        return Mathf.Sqrt(Mathf.Max(0, -2 * Mathf.Log(r1))) * Mathf.Cos(2 * Mathf.PI * r2);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _rigidbody.velocity = Vector3.Reflect(_velocity, collision.contacts[0].normal).normalized * 0.5f * _speedOfParticle;
    }


    void Update()
    {
        FadeIn();
    }


    void FixedUpdate()
    {
        if (_isMoving )
        {
            float r = Random.Range(0f, 1f);
            if (r < .05)
            {
                _rigidbody.velocity = 0.8f * _rigidbody.velocity + 0.2f * new Vector3(GaussianNormal(), GaussianNormal(), GaussianNormal()).normalized * _speedOfParticle;
                _rigidbody.velocity = _rigidbody.velocity.normalized * _speedOfParticle;
            }

            if ((_rigidbody.position.y > 30) && (_rigidbody.position.y < 0))
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -_rigidbody.velocity.y, _rigidbody.velocity.z);
            }

            if (Mathf.Abs(_rigidbody.position.x) > 170)
            {
                _rigidbody.velocity = new Vector3(-_rigidbody.velocity.x, _rigidbody.velocity.y, _rigidbody.velocity.z);
            }

            if (Mathf.Abs(_rigidbody.position.z) > 170)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, -_rigidbody.velocity.z);
            }

            _velocity = _rigidbody.velocity;
        }
        


    }
}
