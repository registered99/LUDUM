using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BSelectPage : BPage, FMultiTouchableInterface
{
	
	private FSprite _background;
	
	private FButton _closeButton;
	
	private FLabel _scoreLabel;
	private FLabel _timeLabel;
	
	private int _frameCount = 0;
	private float _secondsLeft = 15.9f;
	
	private int _totalBananasCreated = 0;
	private FContainer _foodContainer;
	private List<BFood> _foods = new List<BFood>();
	private List<BFood> _selected_foods = new List<BFood>();
	
	private int _maxFramesTillNextBanana = 22;
	private int _framesTillNextBanana = 0;	
	
	private FContainer _effectHolder;
	
	private GameObject _particlePrefab;

	public BSelectPage()
	{
		EnableMultiTouch();
		ListenForUpdate(HandleUpdate);
		ListenForResize(HandleResize);
	}

	override public void Start()
	{
		_background = new FSprite("JungleBlurryBG");
		AddChild(_background);

		//the banana container will make it easy to keep the bananas at the right depth
		_foodContainer = new FContainer(); 
		AddChild(_foodContainer); 
		List<string> foodList = new List<string>();
		foodList.Add ("Monkey_0");
		for(int i = 0; i < 8; ++i)
		{
			foodList.Add ("Banana");
		}
		LayOutFood (foodList);
		
		_closeButton = new FButton("CloseButton_normal", "CloseButton_down","CloseButton_over", "ClickSound");
		AddChild(_closeButton);
		
		_closeButton.SignalRelease += HandleCloseButtonRelease;
		
		_scoreLabel = new FLabel("Franchise", "0 Bananas");
		_scoreLabel.anchorX = 0.0f;
		_scoreLabel.anchorY = 1.0f;
		_scoreLabel.scale = 0.75f;
		_scoreLabel.color = new Color(1.0f,0.90f,0.0f);
		
		_timeLabel = new FLabel("Franchise", ((int)_secondsLeft) + " Seconds Left");
		_timeLabel.anchorX = 1.0f;
		_timeLabel.anchorY = 1.0f;
		_timeLabel.scale = 0.75f;
		_timeLabel.color = new Color(1.0f,1.0f,1.0f);
		
		AddChild(_scoreLabel);
		AddChild(_timeLabel);
		
		_effectHolder = new FContainer();
		AddChild (_effectHolder);
		
		_scoreLabel.alpha = 0.0f;
		Go.to(_scoreLabel, 0.5f, new TweenConfig().
			setDelay(0.0f).
			floatProp("alpha",1.0f));
		
		_timeLabel.alpha = 0.0f;
		Go.to(_timeLabel, 0.5f, new TweenConfig().
			setDelay(0.0f).
			floatProp("alpha",1.0f).
			setEaseType(EaseType.BackOut));
		
		_closeButton.scale = 0.0f;
		Go.to(_closeButton, 0.5f, new TweenConfig().
			setDelay(0.0f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
		
		HandleResize(true); //force resize to position everything at the start
		
	}
	
	protected void HandleResize(bool wasOrientationChange)
	{
		//this will scale the background up to fit the screen
		//but it won't let it shrink smaller than 100%
		_background.scale = Math.Max (Math.Max(1.0f,Futile.screen.height/_background.textureRect.height),Futile.screen.width/_background.textureRect.width);
		 
		_closeButton.x = -Futile.screen.halfWidth + 30.0f;
		_closeButton.y = -Futile.screen.halfHeight + 30.0f;
		
		_scoreLabel.x = -Futile.screen.halfWidth + 10.0f;
		_scoreLabel.y = Futile.screen.halfHeight - 10.0f;
		
		_timeLabel.x = Futile.screen.halfWidth - 10.0f;
		_timeLabel.y = Futile.screen.halfHeight - 10.0f;
	}

	private void HandleCloseButtonRelease (FButton button)
	{
		BMain.instance.GoToPage(BPageType.TitlePage);
	}
	
	public void CreateFood(string foodName, float x, float y)
	{
		BFood food = new BFood(foodName);
		_foodContainer.AddChild(food);
		food.x = x;
		food.y = y;
		_foods.Add(food);
	}
	
	public void LayOutFood(List<string> foodlist)
	{
		float xStart = -Futile.screen.halfWidth+70;
		int maxCols = 4;
		float y = 0f;
		float xDiff = (Futile.screen.width-70)/maxCols; // change this Screen width to shelf width instead
		
		int colCount = 0;
		for(int i = 0; i< foodlist.Count; ++i)
		{
			if(colCount > maxCols){ colCount = 0; y -= 55.0f;}
			CreateFood (foodlist[i], xStart+xDiff*colCount, y);
			++colCount;
			
		}
		
	}
	
	protected void HandleUpdate ()
	{
		//_secondsLeft -= Time.deltaTime;
		
		if(_secondsLeft <= 0)
		{
			FSoundManager.PlayMusic("VictoryMusic",0.5f);
			BMain.instance.GoToPage(BPageType.ScorePage);
			return;
		}
		
		_timeLabel.text = ((int)_secondsLeft) + " Seconds Left";
		
		if(_secondsLeft < 10) //make the timer red with 10 seconds left
		{
			_timeLabel.color = new Color(1.0f,0.2f,0.0f);
		}
	}
	
	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if(touch.phase == TouchPhase.Began)
			{
				
				//we go reverse order so that if we remove a banana it doesn't matter
				//and also so that that we check from front to back
				
				for (int f = _foods.Count-1; f >= 0; f--) 
				{
					BFood food = _foods[f];
					
					Vector2 touchPos = food.GlobalToLocal(touch.position);
					
					if(food.textureRect.Contains(touchPos))
					{
					TouchFood(food);
					CreateExplodeEffect(food);	
					break; //break so that a touch can only hit one banana at a time
					}
				}
			}
		}
	}
	
	private void CreateExplodeEffect(BFood food)
	{
		//we can't just get its x and y, because they might be transformed somehow
		Vector2 foodPos = _effectHolder.OtherToLocal(food,Vector2.zero);
		
		FSprite explodeSprite = new FSprite("Banana");
		_effectHolder.AddChild(explodeSprite);
		explodeSprite.shader = FShader.Additive;
		explodeSprite.x = foodPos.x;
		explodeSprite.y = foodPos.y;
		explodeSprite.rotation = food.rotation;
		
		Go.to (explodeSprite, 0.3f, new TweenConfig().floatProp("scale",1.3f).floatProp("alpha",0.0f).onComplete(HandleExplodeSpriteComplete));
	}
	private void TouchFood(BFood food){
		if(	_selected_foods.Contains(food)){
			_selected_foods.Remove(food);
			food.alpha = 1f;
		} else {
			_selected_foods.Add(food);
			food.alpha = 0.5f;
		}
	}
	
	private static void HandleExplodeSpriteComplete (AbstractTween tween)
	{
		FSprite explodeSprite = (tween as Tween).target as FSprite;
		explodeSprite.RemoveFromContainer();
	}
}

