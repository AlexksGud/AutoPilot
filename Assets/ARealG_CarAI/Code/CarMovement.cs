using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _turnSpeed;
  

    [Header("Drift")]
    [SerializeField, Range(0.85f, 0.995f)] private float driftFactor;
    [SerializeField] private ParticleSystem _smoke1, _smoke2;
    [SerializeField] private TrailRenderer _leftSkid, _rightSkid;
    [SerializeField] private Transform _frontLeftWheel, _frontRightWheel;

    private float _rotationAngle = 0;
    private const float _brakeForceFactor = -0.6f;
    private const float _acceleraqtionFactor = 100000f;
    private float _brakeForce;

    private float _steeringInput;
    private float _accelerationInput;
    private bool _isStopped;

    private Rigidbody _rigidBody;
    private Vector3 _lastVelocity;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    public void SetDrive(float steerValue, float accelValue, float brakeForceValue)
    {
        _steeringInput = steerValue;
        _accelerationInput = accelValue;
        _brakeForce = brakeForceValue;
    }

    public void StopEngine()
    {
        _lastVelocity = _rigidBody.velocity;
        _rigidBody.velocity = Vector3.zero;
        _isStopped = true;
    }

    public void StartEngine()
    {
        _isStopped = false;
        _rigidBody.velocity = _lastVelocity;
    }

    private void FixedUpdate()
    {
        if (_isStopped)
            return;

        ApplyEngineForce();
        DecreaseSideVelocity();
        ApplySteering();
        Drift();
    }

    private void ApplyEngineForce()
    {
        float forwardVelocity = Vector3.Dot(_rigidBody.velocity, transform.forward);

        if (forwardVelocity > _maxSpeed)
            return;

        Vector3 force = transform.forward * (_acceleration * Time.fixedDeltaTime * _accelerationInput * _acceleraqtionFactor);
        Vector3 brakeForceVector = transform.forward * _brakeForce * _brakeForceFactor;

        _rigidBody.AddForce(force, ForceMode.Force);
        _rigidBody.AddForce(brakeForceVector, ForceMode.Force);
    }

    private void DecreaseSideVelocity()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rigidBody.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(_rigidBody.velocity, transform.right);
        _rigidBody.velocity = forwardVelocity + (rightVelocity * driftFactor);
    }

    private void ApplySteering()
    {
        if (_rigidBody.velocity.sqrMagnitude < 0.1f)
        {
            _rotationAngle += 0;
        }
        else
        {
            _rotationAngle += _steeringInput * _turnSpeed * Time.fixedDeltaTime;
        }

        _rigidBody.MoveRotation(Quaternion.Euler(0, _rotationAngle, 0));
        RotateWheels();

        void RotateWheels()
        {
            Quaternion degree = Quaternion.Euler(0, 58f * _steeringInput, 0);

            _frontRightWheel.localRotation = degree;
            _frontLeftWheel.localRotation = degree;
        }
    }

    private void Drift()
    {
        if (_steeringInput == 0)
            return;

        float lateralVelocity = GetLateralVelocity();

        if (Mathf.Abs(lateralVelocity) > 1)
        {
            Skid(true);
            Vector3 driftForce = -transform.right * lateralVelocity * driftFactor * Time.fixedDeltaTime;
            _rigidBody.AddForce(driftForce, ForceMode.Force);
        }
        else
        {
            Skid(false);
        }

        void Skid(bool value)
        {
            _leftSkid.emitting = value;
            _rightSkid.emitting = value;

            if (value)
            {
                _smoke1.Emit(1);
                _smoke2.Emit(1);
            }
        }
    }

    private float GetLateralVelocity()
    {
        return Vector3.Dot(transform.right, _rigidBody.velocity);
    }
}


