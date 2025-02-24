﻿using UnityEngine;
using System.Collections;

public class DriveController : MonoBehaviour
{
    private WheelCollider[] wheels;
    private Rigidbody rigBody;
    public GameObject wheelShapePrefab;

    public Car car;

    public float currentSpeed;

    // Curr motor torque
    float motorTorque;

    // Curr brake torque
    float brakeTorque;

    // Curr steer angle
    float steerAngle;

    // Drag variables. These are used to slow down the car
    // when there is no torque on it.
    float upperSpeedLimitForDrag = 10f;
    float lowerDragLimit = 0.1f;
    float upperDragLimit = 0.9f;

    // here we find all the WheelColliders down in the hierarchy
    public void Start()
    {
        rigBody = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();

        // create wheel shapes only when needed
        if (wheelShapePrefab != null)
        {
            foreach (var wheel in wheels)
            {
                var ws = GameObject.Instantiate(wheelShapePrefab, wheel.transform);
            }
        }
    }

    // this is a really simple approach to updating wheels
    // here we simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero
    // this helps us to figure our which wheels are front ones and which are rear
    public void Update()
    {
        if (car == null)
        {
            Debug.LogError("WheelDriveController -- Update() -- Probably you forget a car in the scene.");
            return;
        }

		if (car.controller == Controller.Player)
        {
			steerAngle  = car.maxSteerAngle  * InputController.Instance.horAxis;
			motorTorque = car.maxMotorTorque * InputController.Instance.verAxis;
			brakeTorque = car.maxBrakeTorque * InputController.Instance.spaceKeyAxis;
        }

        foreach (WheelCollider wheel in wheels)
        {
            // update visual wheels if any
            if (wheelShapePrefab)
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                // assume that the only child of the wheelcollider is the wheel shape
                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
            }
        }
    }

    void FixedUpdate()
    {
		rigBody.drag = Mathf.Clamp(((upperSpeedLimitForDrag - currentSpeed) / upperSpeedLimitForDrag) *
            (upperDragLimit - lowerDragLimit) + lowerDragLimit, lowerDragLimit, upperDragLimit);

        foreach (WheelCollider wheel in wheels)
        {
            // a simple car where front wheels steer while rear ones drive
            if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = steerAngle;

            if (currentSpeed < car.maxSpeed)
            {
                if (wheel.transform.localPosition.z < 0)
                    wheel.motorTorque = motorTorque;
            }
            else
			{
                wheel.motorTorque = 0;
			}

            wheel.brakeTorque = brakeTorque;
        }

		currentSpeed = GetSpeedOfCar();
    }

    public bool IsCarGrounded()
    {
        foreach (var wheel in wheels)
            if (wheel.isGrounded == false)
                return false;

        return true;
    }

    public string IsThereObstacle(float stoppingInterval)
    {
        Transform raycastPoint = gameObject.transform.Find("RaycastPoint");

        RaycastHit raycastInfo;

        //float carSpeed = car.getSpeedOfCar != null ? car.getSpeedOfCar() : 1;
		float dist = Mathf.Clamp(stoppingInterval * currentSpeed / 10, 2, stoppingInterval);
        if (Physics.Raycast(raycastPoint.position, raycastPoint.forward, out raycastInfo, dist ) == false)
            return null;

        return raycastInfo.collider.tag;
    }

    public void SetMotorTorque(float torque)
    {
        if (currentSpeed < car.maxSpeed)
            motorTorque = Mathf.Clamp(torque, -car.maxMotorTorque, car.maxMotorTorque);
        else
            motorTorque = 0;
    }

    public void SetBrakeTorque(float torque)
    {
        brakeTorque = Mathf.Clamp(torque, -car.maxBrakeTorque, car.maxBrakeTorque);
    }

    public void SetSteerAngle(float angle)
    {
        steerAngle = Mathf.Clamp(angle, -car.maxSteerAngle, car.maxSteerAngle);
    }

	public float GetSpeedOfCar()
	{
		return Mathf.Abs(rigBody.velocity.magnitude * 3.6f);
	}
}
