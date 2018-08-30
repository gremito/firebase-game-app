using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{

	public const int GET_POINT = 10;
	public const int TIME_MAX = 15;
	
	[SerializeField] private Canvas gameOverScene;
	[SerializeField] private Text userPointText;
	[SerializeField] private Text gameTimeText;
	
	private int userPoint;	
	private bool gameNow;
	private float gameTime;

	public void GoToStart()
	{
		SceneManager.LoadScene("start");
	}

	void Init()
	{
		userPoint = 0;
		gameTime = TIME_MAX;
		gameNow = true;
		gameOverScene.enabled = false;
	}

	void Awake()
	{
		Init();
	}

	void Update()
	{
		if (!gameNow)
		{
			return;
		}
		
		gameTime -= Time.deltaTime;

		UpdateUI();

		IsGameOver();
	}

	public void HitShot()
	{
		if (this.gameNow)
		{
			this.userPoint += GET_POINT;
		}
	}

	void UpdateUI()
	{
		userPointText.text = userPoint + "pt";
		gameTimeText.text = "Time: " + (int) gameTime;
	}

	void IsGameOver()
	{
		if (gameTime < 1)
		{
			gameOverScene.enabled = true;
			gameNow = false;
		}
	}
	
}
