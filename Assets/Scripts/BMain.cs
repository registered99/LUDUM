using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BPageType
{
	None,
	TitlePage,
	SelectPage,
	InGamePage,
	ScorePage
}

public class BMain : MonoBehaviour
{	
	public static BMain instance;
	
	public int score = 0;
	public int bestScore = 0;
	
	private BPageType _currentPageType = BPageType.None;
	private BPage _currentPage = null;
	
	private FStage _stage;
	
	private void Start()
	{
		instance = this; 
		
		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;
		
		//Time.timeScale = 0.4f;
		
		FutileParams fparams = new FutileParams(true,true,false,false);
		
		fparams.AddResolutionLevel(480.0f,	1.0f,	1.0f,	"_Scale1"); //iPhone
		
		fparams.origin = new Vector2(0.5f,0.5f);
		
		Futile.instance.Init (fparams);
		
		Futile.atlasManager.LoadAtlas("Atlases/BananaLargeAtlas");
		Futile.atlasManager.LoadAtlas("Atlases/BananaGameAtlas");
		
		Futile.atlasManager.LoadFont("Franchise","FranchiseFont"+Futile.resourceSuffix, "Atlases/FranchiseFont"+Futile.resourceSuffix, 0.0f,-4.0f);
		
		_stage = Futile.stage;
		
		FSoundManager.PlayMusic ("NormalMusic",0.5f);
		
        GoToPage(BPageType.TitlePage);
        GoToPage(BPageType.SelectPage);
	}

	public void GoToPage (BPageType pageType)
	{
		if(_currentPageType == pageType) return; //we're already on the same page, so don't bother doing anything
		
		BPage pageToCreate = null;
		
		if(pageType == BPageType.TitlePage)
		{
			pageToCreate = new BTitlePage();
		}
		if(pageType == BPageType.SelectPage)
		{
			pageToCreate = new BSelectPage();
		}
		if(pageType == BPageType.InGamePage)
		{
			pageToCreate = new BInGamePage();
		}
		else if (pageType == BPageType.ScorePage)
		{
			pageToCreate = new BScorePage();
		}
		
		if(pageToCreate != null) //destroy the old page and create a new one
		{
			_currentPageType = pageType;	
			
			if(_currentPage != null)
			{
				_stage.RemoveChild(_currentPage);
			}
			 
			_currentPage = pageToCreate;
			_stage.AddChild(_currentPage);
			_currentPage.Start();
		}
		
	}
	
}









