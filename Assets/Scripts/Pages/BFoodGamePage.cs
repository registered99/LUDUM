using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BFoodGamePage : BPage, FMultiTouchableInterface
{
	
	private FSprite _background;
	
	private FButton _closeButton;
	
	private FLabel _scoreLabel;
	private FLabel _timeLabel;
	
	private int _frameCount = 0;
	private float _secondsLeft = 60.0f;
	
	private int _totalBananasCreated = 0;
	private FContainer _bananaContainer;
	private List<BBanana> _bananas = new List<BBanana>();
	private FContainer _foodContainer;
	private List<BFood> _foods = new List<BFood>();
	
	private int _maxFramesTillNextBanana = 22;
	private int _framesTillNextBanana = 0;	
	
	private FContainer _effectHolder;
	
	private GameObject _particlePrefab;

	public BFoodGamePage()
	{
		EnableMultiTouch();
		ListenForUpdate(HandleUpdate);
		ListenForResize(HandleResize);
	}

	override public void Start()
	{
		BMain.instance.score = 0;
		
		_background = new FSprite("JungleBlurryBG");
		AddChild(_background);

		//the banana container will make it easy to keep the bananas at the right depth
		_bananaContainer = new FContainer(); 
		AddChild(_bananaContainer); 
		_foodContainer = new FContainer(); 
		AddChild(_foodContainer); 
		
		LayOutFood(BMain.instance.selected_foods);
		
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
	
	public void CreateBanana ()
	{
		BBanana banana = new BBanana();
		_bananaContainer.AddChild(banana);
		banana.x = RXRandom.Range(-Futile.screen.width/2 + 50, Futile.screen.width/2 - 50); //padded inside the screen width
		banana.y = Futile.screen.height/2 + 60; //above the screen
		_bananas.Add(banana);
		_totalBananasCreated++;
	}
	
	
	protected void HandleUpdate ()
	{
		_secondsLeft -= Time.deltaTime;
		
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
		
		_framesTillNextBanana--;
		
		if(_framesTillNextBanana <= 0)
		{
			if(_totalBananasCreated % 4 == 0) //every 4 bananas, make the bananas come a little bit sooner
			{
				_maxFramesTillNextBanana--;
			}
			
			_framesTillNextBanana = _maxFramesTillNextBanana;
			
			CreateBanana();
		}
		
		
		//loop backwards so that if we remove a banana from _bananas it won't cause problems
		for (int b = _bananas.Count-1; b >= 0; b--) 
		{
			BBanana banana = _bananas[b];
			
			//remove a banana if it falls off screen
			if(banana.y < -Futile.screen.halfHeight - 50)
			{
				_bananas.Remove(banana);
				_bananaContainer.RemoveChild(banana);
			}
		}
		
		_frameCount++;
	}
	
	public void CreateFood(BFood food, float x, float y)
	{
		_foodContainer.AddChild(food);
		food.x = x;
		food.y = y;
		_foods.Add(food);
	}
	
	public void LayOutFood(List<BFood> foodlist)
	{
		float xStart = -Futile.screen.halfWidth+70;
		int maxCols = 4;
		float y = 0f;
		float xDiff = (Futile.screen.width-70)/maxCols; // change this Screen width to shelf width instead
		
		int colCount = 0;
		for(int i = 0; i < foodlist.Count; ++i)
		{
			if(colCount > maxCols){ colCount = 0; y -= 55.0f;}
			CreateFood (foodlist[i], xStart+xDiff*colCount, y);
			++colCount;
			
		}
		
	}	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if(touch.phase == TouchPhase.Began)
			{
				
				//we go reverse order so that if we remove a banana it doesn't matter
				//and also so that that we check from front to back
				
				for (int b = _bananas.Count-1; b >= 0; b--) 
				{
					BBanana banana = _bananas[b];
					
					Vector2 touchPos = banana.GlobalToLocal(touch.position);
					
					if(banana.textureRect.Contains(touchPos))
					{
						//HandleGotBanana(banana);	
						break; //break so that a touch can only hit one banana at a time
					}
				}
			}
		}
	}
	
	private void CreateBananaExplodeEffect(BBanana banana)
	{
		//we can't just get its x and y, because they might be transformed somehow
		Vector2 bananaPos = _effectHolder.OtherToLocal(banana,Vector2.zero);
		
		FSprite explodeSprite = new FSprite("Banana");
		_effectHolder.AddChild(explodeSprite);
		explodeSprite.shader = FShader.Additive;
		explodeSprite.x = bananaPos.x;
		explodeSprite.y = bananaPos.y;
		explodeSprite.rotation = banana.rotation;
		
//		Go.to (explodeSprite, 0.3f, new TweenConfig().floatProp("scale",1.3f).floatProp("alpha",0.0f).onComplete(HandleExplodeSpriteComplete));
	}

}

