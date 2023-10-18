using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationFactor;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float brakeForce;

    [Header("Drift")]
    [SerializeField] private float driftFactor;
    [SerializeField] private ParticleSystem smoke1, smoke2;
    [SerializeField] private TrailRenderer leftSkid, rightSkid;
    [SerializeField] private Transform frontLeftWheel, frontRightWheel;

    private float rotationAngle = 0;
    private const float brakeForceFactor = -0.6f;

    private float steeringInput;
    private float accelerationInput;
    private bool isStopped;

    private Rigidbody rb;
    private Vector3 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        accelerationFactor *= 100000f;
    }

    public void SetDrive(float steerValue, float accelValue, float brakeForceValue)
    {
        steeringInput = steerValue;
        accelerationInput = accelValue;
        brakeForce = brakeForceValue;
    }

    public void StopEngine()
    {
        lastVelocity = rb.velocity;
        rb.velocity = Vector3.zero;
        isStopped = true;
    }

    public void StartEngine()
    {
        isStopped = false;
        rb.velocity = lastVelocity;
    }

    private void FixedUpdate()
    {
        if (isStopped)
            return;

        ApplyEngineForce();
        DecreaseSideVelocity();
        ApplySteering();
        Drift();
    }

    private void ApplyEngineForce()
    {
        float forwardVelocity = Vector3.Dot(rb.velocity, transform.forward);

        if (forwardVelocity > maxSpeed)
            return;

        Vector3 force = transform.forward * (accelerationFactor * Time.fixedDeltaTime * accelerationInput);
        Vector3 brakeForceVector = transform.forward * brakeForce * brakeForceFactor;

        rb.AddForce(force, ForceMode.Force);
        rb.AddForce(brakeForceVector, ForceMode.Force);
    }

    private void ApplySteering()
    {
        if (rb.velocity.sqrMagnitude < 0.1f)
        {
            rotationAngle += 0;
        }
        else
        {
            rotationAngle += steeringInput * turnSpeed * Time.fixedDeltaTime;
        }

        rb.MoveRotation(Quaternion.Euler(0, rotationAngle, 0));
        RotateWheels();

        void RotateWheels()
        {
            Quaternion degree = Quaternion.Euler(0, 58f * steeringInput, 0);

            frontRightWheel.localRotation = degree;
            frontLeftWheel.localRotation = degree;
        }
    }

    private void Drift()
    {
        if (steeringInput == 0)
            return;

        float lateralVelocity = GetLateralVelocity();

        if (Mathf.Abs(lateralVelocity) > 1)
        {
            Skid(true);
            Vector3 driftForce = -transform.right * lateralVelocity * driftFactor * Time.fixedDeltaTime;
            rb.AddForce(driftForce, ForceMode.Force);
        }
        else
        {
            Skid(false);
        }

        void Skid(bool value)
        {
            leftSkid.emitting = value;
            rightSkid.emitting = value;

            if (value)
            {
                smoke1.Emit(1);
                smoke2.Emit(1);
            }
        }
    }

    private void DecreaseSideVelocity()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right);
        rb.velocity = forwardVelocity + (rightVelocity * driftFactor);
    }

    private float GetLateralVelocity()
    {
        return Vector3.Dot(transform.right, rb.velocity);
    }
}


