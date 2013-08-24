using System;
using UnityEngine;

public class BFood : FSprite
{
	private float _rotationSpeed;
	private float _speedY;
	public string name;
	
	public BFood (string food_type) : base(food_type)
	{
		_rotationSpeed = RXRandom.Range(-3.0f,3.0f);	
		_speedY = RXRandom.Range(-0.1f,-0.5f);	
		name = food_type;

		ListenForUpdate(HandleUpdate);
	}
	
	public void HandleUpdate()
	{
		this.rotation += _rotationSpeed;
	}
	
	

}


