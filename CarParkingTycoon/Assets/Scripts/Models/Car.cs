﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Car
{
	// Name is the brand
	public string name;

	public NPC npc;

	// Max motor torque
	public float maxMotorTorque;

	// Max brake torque
	public float maxBrakeTorque;

	// Max steer angle
	public float maxSteerAngle;

    //Max speed that this car can get
    public float maxSpeed;

	// %60 damaged
	float currentCarStatePercent;

	// sensivity of car
    float damageTakePerHit;

    // if we want to buy or cell cars this will be usefull
    int price;

    //if we already instatiated smoke particules we dont have to instantiate every time we hit an object
    bool effectAnimIsPlaying = false;

	const float stopError = 0.1f;

    public bool isParked;

    private Controller _controller;
	public Controller controller
    {
        get { return _controller; }
        set
        {
			_controller = value;
            if (value == Controller.Player)
            {
                if (onPlayerControl != null)
                    onPlayerControl(this);
            }
        }
    }

	public Func<bool> isCarGrounded;
	public Func<float, string> isThereObstacle;
	public Func<float> getSpeedOfCar;
	public Action<float> onSetMotorTorque;
	public Action<float> onSetBrakeTorque;
	public Action<float> onSetSteerAngle;
    public Action<Car> onPlayerControl;
	public Func<Vector3[]> getParkingInfo;
    public Action<Car> onOverDamageEffect;

	// if you want to see specific car info you can add as much as you want
    // we can think about adding smoke particule over decent amount of damage
    // maybe fuel
    // TODO : think about adding events or callbacks to appropriate variables that we might need(explain with a good reason)

	public Car(string name, float maxSpeed ,float maxMotorTorque, float maxBrakeTorque, float maxSteerAngle,
		float damagePercent, float damageTakePerHit, int price, bool isParked, Controller controller = Controller.NPC)
	{
		this.name             = name;
		this.maxMotorTorque   = maxMotorTorque;
		this.maxBrakeTorque   = maxBrakeTorque;
		this.maxSteerAngle    = maxSteerAngle;
		this.currentCarStatePercent    = damagePercent;
		this.damageTakePerHit = damageTakePerHit;
		this.price            = price;
		this.isParked         = isParked;
		this.controller       = controller;
        this.maxSpeed         = maxSpeed;
	}

	public Car Clone()
	{
		Car car = new Car(
			this.name,
            this.maxSpeed,
			this.maxMotorTorque,
			this.maxBrakeTorque,
			this.maxSteerAngle,
			this.currentCarStatePercent,
			this.damageTakePerHit,
			this.price,
			this.isParked,
			this.controller
		);

		return car;
	}

    public void TakeDamage()
    {
        currentCarStatePercent -= damageTakePerHit;

        if (effectAnimIsPlaying == false && currentCarStatePercent <= 50 && onOverDamageEffect != null)
        {
            effectAnimIsPlaying = true;
            onOverDamageEffect(this);
        }
    }
	public void Update()
	{
        // If the car is not controlled by npc, just return.
		if (controller != Controller.NPC)
		{
            return;
        }

		npc.Update();
	}

	public void ParkCar(ParkingSpace ps)
	{
		controller = Controller.None;
		StopTheCar();
		isParked = true;
		ps.occupied = true;

		CarPark carPark = WorldController.Instance.world.carPark;
		carPark.ParkCar(this, ps);
	}

	public void StopTheCar()
	{
		if(onSetMotorTorque != null)
			onSetMotorTorque(0f);

		if(onSetBrakeTorque != null)
			onSetBrakeTorque(maxBrakeTorque);
	}

	public bool IsCarMoving()
	{
		if(getSpeedOfCar == null)
			return false;

		return (getSpeedOfCar() > stopError);
	}


	#region Register or unregister actions and funcs

	public void RegisterIsCarGroundedFunc(Func<bool> func)
	{
		this.isCarGrounded += func;
	}

	public void UnRegisterIsCarGroundedFunc(Func<bool> func)
	{
		this.isCarGrounded -= func;
	}

	public void RegisterIsThereObstacleFunc(Func<float, string> func)
	{
		this.isThereObstacle += func;
	}

	public void UnRegisterIsThereObstacleFunc(Func<float, string> func)
	{
		this.isThereObstacle -= func;
	}

	public void RegisterGetSpeedOfCarFunc(Func<float> func)
	{
		this.getSpeedOfCar += func;
	}

	public void UnRegisterGetSpeedOfCarFunc(Func<float> func)
	{
		this.getSpeedOfCar -= func;
	}

	public void RegisterOnSetMotorTorque(Action<float> cb)
	{
		this.onSetMotorTorque += cb;
	}

	public void UnRegisterOnSetMotorTorque(Action<float> cb)
	{
		this.onSetMotorTorque -= cb;
	}

	public void RegisterOnSetBrakeTorque(Action<float> cb)
	{
		this.onSetBrakeTorque += cb;
	}

	public void UnRegisterOnSetBrakeTorque(Action<float> cb)
	{
		this.onSetBrakeTorque -= cb;
	}

	public void RegisterOnSetSteerAngle(Action<float> cb)
	{
		this.onSetSteerAngle += cb;
	}

	public void UnRegisterOnSetSteerAngle(Action<float> cb)
	{
		this.onSetSteerAngle -= cb;
	}

    public void RegisterOnPlayerControl(Action<Car> cb)
    {
        this.onPlayerControl += cb;
    }

    public void UnRegisterOnPlayerControl(Action<Car> cb)
    {
        this.onPlayerControl -= cb;
    }

    public void RegisterOnOverDamageEffect(Action<Car> cb)
    {
        this.onOverDamageEffect += cb;
    }

    public void UnRegisterOnOverDamageEffect(Action<Car> cb)
    {
        this.onOverDamageEffect -= cb;
    }
    public void RegisterGetParkingInfo(Func<Vector3[]> cb)
	{
		this.getParkingInfo += cb;
	}

	public void UnRegisterGetParkingInfo(Func<Vector3[]> cb)
	{
		this.getParkingInfo -= cb;
	}

    #endregion
}
