using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : SuperClass 
{
	Grid GridObject;
	public int x;
	public int y;
	public Color color;
	public Vector2 tempPos;
	public bool temp;
	public Vector2 speed;
	private bool bomb;
	private int bombTimer;
	private TextMesh bombText;

	// Diğer altıgenlerin koordinatları için oluşturulan yapı 
	public struct OtherHexs 
	{
		public Vector2 up;
		public Vector2 upLeft;
		public Vector2 upRight;
		public Vector2 down;
		public Vector2 downLeft;
		public Vector2 downRight;
	}

	void Start() 
	{
		GridObject = Grid.instance;
		temp = false;
	}

	void Update() 
	{
		if (temp) 
		{
			float newX = Mathf.Lerp(transform.position.x, tempPos.x, Time.deltaTime*HEXAGON_ROTATE_CONSTANT);
			float newY = Mathf.Lerp(transform.position.y, tempPos.y, Time.deltaTime*HEXAGON_ROTATE_CONSTANT);
			transform.position = new Vector2(newX, newY);
			if (Vector3.Distance(transform.position, tempPos) < HEXAGON_ROTATE_THRESHOLD) 
			{
				transform.position = tempPos;
				temp = false;
			}
		}
	}


	// Döndürme fonksiyonu
	public void Rotate(int newX, int newY, Vector2 newPos) 
	{
		tempPos = newPos;
		SetX(newX);
		SetY(newY);
		temp = true;
	}

	// Altıgenler dönüyor mu? Temp durumunu belirledik
	public bool IsRotating() 
	{
		return temp;
	}

	public bool IsMoving() 
	{
		return !(GetComponent<Rigidbody2D>().velocity == Vector2.zero);
	}
	
	public void Exploded() 
	{
		GetComponent<Collider2D>().isTrigger = true;
	}

	// Diğer altıgenlerin grid konumundan bir yapı oluşturur ve onu döndürür 
	public OtherHexs GetOthers()
	{
		OtherHexs others;
		bool onStepper = GridObject.OnStepper(GetX()); //Grid yapısındaki OnStepper(int x) metoduna eşleştik

		others.down = new Vector2(x, y-1);
		others.up = new Vector2(x, y+1);
		others.upLeft = new Vector2(x-1, onStepper ? y+1 : y);
		others.upRight = new Vector2(x+1, onStepper ? y+1 : y);
		others.downLeft = new Vector2(x-1, onStepper ? y : y-1);
		others.downRight = new Vector2(x+1, onStepper ? y : y-1);

		return others;
	}

	// Altıgen için yeni dünya pozisyonu belirler 
	public void ChangeWorldPosition(Vector2 newPos) 
	{
		tempPos = newPos;
		temp = true;
	}

	// Altıgen için yeni grid konumu
	public void ChangeGridPosition(Vector2 newPos)
	{
		x = (int)newPos.x;
		y = (int)newPos.y;
	}

	// Bomba geri sayımının özellikleri
	public void SetBomb() 
	{
		//TextMesh olduğunu belirtildi
		bombText = new GameObject().AddComponent<TextMesh>();
		//Text'in durucağı pozisyon
		bombText.alignment = TextAlignment.Center;
		bombText.anchor = TextAnchor.MiddleCenter;
		bombText.transform.position = new Vector3(transform.position.x, transform.position.y, -4);
		bombText.transform.localScale = transform.localScale; //Büyüklüğünü ayarlama
		bombText.color = Color.black; //Renk
		bombText.transform.parent = transform; // Bağlı olduğu nesne
		bombTimer = BOMB_TIMER_START; //6
		bombText.text = bombTimer.ToString(); //text olarak bombTimer'ı göstericek.
	}

	// Setters & Getters 
	public int GetX() 
	{ 
		return x; 
	}
	public void SetX(int value) 
	{ 
		x = value; 
	}
	public int GetY() 
	{ 
		return y; 
	}
	public void SetY(int value) 
	{ 
		y = value; 
	}
	public Color GetColor()
	{ 
		return GetComponent<SpriteRenderer>().color; 
	}

	public void SetColor(Color newColor) //yeni rengin gelmesi için oluşturulan SetColor yapısı
	{ 
		GetComponent<SpriteRenderer>().color = newColor; color=newColor; 
	}
	public void Tick() // BombHexagon kodundaki Tick() metoduna bağlantı
	{ 
		bombTimer -= 1; 
		bombText.text = bombTimer.ToString(); 
	}
	public int GetTimer() 
	{ 
		return bombTimer; 
	}
}
