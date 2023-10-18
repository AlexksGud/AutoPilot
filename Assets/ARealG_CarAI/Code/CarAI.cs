using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    [SerializeField] private float pointRange = 20f;
    [SerializeField] private CarController carController;
    [SerializeField] private List<Transform> currentPointList;

    private bool _isDriving = true;
    private int currentPoint;

    private float _gasPower=1;
    private float _brakeForce;

    private void FixedUpdate()
    {
        if (!_isDriving)
            return;

        carController.SetDrive(CalculateSteeringAngle(), _gasPower, _brakeForce);
        HandlePointControl();
    }

    private float CalculateSteeringAngle()
    {
        Vector3 point = currentPointList[currentPoint].position;
        Vector3 target = point - transform.position;
        target.Normalize();

        float angleToTarget = Vector3.SignedAngle(transform.forward, target, transform.up);
        float steerAmount = angleToTarget / 45f;
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        CalculateBrake(steerAmount);

        return steerAmount;
    }

    private void CalculateBrake(float angle)
    {
        if (Mathf.Abs(angle) > 0.6f)
            _brakeForce = 1;
        else
            _brakeForce = -1;
    }

    private void HandlePointControl()
    {
        if (Vector3.Distance(transform.position, currentPointList[currentPoint].position) < pointRange)
        {
            MoveToNextPoint();
        }
    }

    private void MoveToNextPoint()
    {
        if (currentPoint == currentPointList.Count - 1)
        {
            currentPoint = 0;
        }
        else
        {
            currentPoint++;
        }
    }

    public void SetPointsList(List<Transform> trackpoints)
    {
        currentPointList = trackpoints;
    }

    public void StopDrive()
    {
        _isDriving = false;
        carController.StopEngine();
    }
    public void StartDrive()
    {
        _isDriving = true;
        carController.StartEngine();
    }
}



