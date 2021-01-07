using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserInterfaceManager : SuperClass 
{
	public Text score;
	public Text highScore;
	public int hexWidth; //9
	public int hexHeight; //8
	public GameObject scoreBoard;
	public GameObject MenuScreen;	
	public GameObject gameOverScreen;
	public bool tick;
	private Grid GridObject;
	private int colorCount;
	private int blownHexagons;
	private int bombCount;
	public static UserInterfaceManager instance;

	//Singleton pattern
	void Awake() 
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	void Start () 
	{
		highScore.text = PlayerPrefs.GetInt("scoreText", 0).ToString(); //Highscore text'ini kayıt etme
		bombCount = ZERO; //başlangıçta bomba yok
		GridObject = Grid.instance;
		blownHexagons = ZERO; 
		colorCount = 5; //renk sayısı 5
	}
	
	void Update () 
	{
		if (tick) 
		{
			StartGameButton();
			tick = false;
		}
	}

	//Oyun içi scor ve Yüksek scor'un hesabı
	public void Score(int x) 
	{
		blownHexagons += x; 
		int number = (SCORE_CONSTANT * blownHexagons);
		score.text = number.ToString();
		if (Int32.Parse(score.text) > BOMB_SCORE_THRESHOLD * bombCount + BOMB_SCORE_THRESHOLD) 
		{
			++bombCount;
			GridObject.SetBombProduction();
			SoundManager.PlaySound("point"); //Bomba oyuna geldiğinde gelen ses
		}
		//Highscore hesabını tutma
		if(number > PlayerPrefs.GetInt("scoreText", 0))
		{
			PlayerPrefs.SetInt("scoreText", number);
			highScore.text = number.ToString();
		}
	}
	//ilk ekran yani menu ekranı
	public void firstScene()
	{
		SceneManager.LoadScene("SampleScene");
	}
	//Oyun sonunda game over ekranı ve game over audio
	public void GameEnd() {
		gameOverScreen.SetActive(true); 
		SoundManager.PlaySound("gameOver");
	}
	//Oyun çıkış - exit
	public void quitGame()
	{
		Application.Quit();
		Debug.Log("Quit Game");
	}
	// Oyunu başlat simgesine tıklandığında çalışan özelliklerin bulunduğu alan
	public void StartGameButton() 
	{ 
		MenuScreen.SetActive(false);
		scoreBoard.SetActive(true);
		GridObject.SetGridHeight(hexHeight);
		GridObject.SetGridWidth(hexWidth);		

		List<Color> colors = new List<Color>();

		colors.Add(Color.blue);
		colors.Add(Color.red);
		colors.Add(Color.yellow);
		colors.Add(Color.green);
		colors.Add(Color.cyan);
		GridObject.SetColorList(colors);
		GridObject.InitializeGrid();
	}
}
