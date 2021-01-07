using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : SuperClass 
{

	public static Grid instance = null;

	/* Editörden atanacak değişkenler - prefab, gameobject, resim*/
	public GameObject hexPrefab; 
	public GameObject hexParent; 
	public GameObject outParent; 
	public Sprite outlineSprite; 
	public Sprite hexagonSprite;

	// Değişkenler 
	private int gridWidth;
	private int gridHeight;
	private int selectionStatus;
	private bool bombProduction;
	private bool gameEnd;
	private Vector2 selectedPosition;
	private Hexagon selectedHexagon;
	private List<List<Hexagon>> gameGrid;
	private List<Hexagon> selectedGroup;
	private List<Hexagon> bombs;
	private List<Color> colorList;
	// Coroutine 
	private bool hexagonRotationStatus;
	private bool hexagonExplosionStatus;
	private bool hexagonProductionStatus;
	//Skorun görüntüsü
	public Transform floatingText;

	// Singleton 
	void Awake() 
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	void Start() 
	{
		gameEnd = false; // Başlangıçta game over ekranı olmasın 
		bombProduction = false;
		hexagonRotationStatus = false;
		hexagonExplosionStatus = false;
		hexagonProductionStatus = false;
		bombs = new List<Hexagon>();
		selectedGroup = new List<Hexagon>();
		gameGrid = new List<List<Hexagon>>();
	}

	//width - height ayarlanmalıdır 
	public void InitializeGrid() 
	{
		List<int> missingCells = new List<int>();

		// missingCells doldurulması 
		for (int i = 0; i<GetGridWidth(); ++i) 
		{
			for (int j = 0; j<GetGridHeight(); ++j)
				missingCells.Add(i);

			gameGrid.Add(new List<Hexagon>());
		}

		// Grid içerisini altıgenler(hexagon) ile doldur
		StartCoroutine(ProduceHexagons(missingCells, ColoredGridProducer()));
	}



	// Altıgen grubunu seçme işlevi, seçilen altıgeni döndürür 
	public void Select(Collider2D collider) 
	{
		// Seçim mevcut hexden farklıysa, değişkenleri sıfırla 
		if (selectedHexagon == null || !selectedHexagon.GetComponent<Collider2D>().Equals(collider)) 
		{
			selectedHexagon = collider.gameObject.GetComponent<Hexagon>();
			selectedPosition.x = selectedHexagon.GetX();
			selectedPosition.y = selectedHexagon.GetY();
			selectionStatus = 0;
		} else {
			selectionStatus = (++selectionStatus) % SELECTION_STATUS_COUNT; //Toplam sayıyı aşmadan seçim durumu 
		}

		DestructOutline();
		ConstructOutline();
	}

	// Hex'in dokunma konumunda döndürme işlevi 
	public void Rotate(bool clockWise) 
	{
		// Rotasyonun başladığını belirt
		DestructOutline();
		StartCoroutine(RotationCheckCoroutine(clockWise));
	}


	
	// 3'lü altıgen grubunu bul
	private void FindHexagonGroup() 
	{
		List<Hexagon> returnValue = new List<Hexagon>();
		Vector2 XPos, YPos;
		// Grid içinde gerekli diğer 2 altıgen koordinatı bulma 
		selectedHexagon = gameGrid[(int)selectedPosition.x][(int)selectedPosition.y];
		FindOtherHexagons(out XPos, out YPos);
		selectedGroup.Clear();
		selectedGroup.Add(selectedHexagon);
		selectedGroup.Add(gameGrid[(int)XPos.x][(int)XPos.y].GetComponent<Hexagon>());
		selectedGroup.Add(gameGrid[(int)YPos.x][(int)YPos.y].GetComponent<Hexagon>());
	}

	// FindHexagonGroup() metodu içinde seçilen altıgenin komşularını bulmak için yardımcı işlev
	private void FindOtherHexagons(out Vector2 first, out Vector2 second) 
	{
		Hexagon.OtherHexs others = selectedHexagon.GetOthers();
		bool breakTheLoop = false;
		// Seçim konumuna göre doğru komşuyu seçmek
		do {
			switch (selectionStatus) 
			{
				case 0: first = others.up; second = others.upRight; break;
				case 1: first = others.upRight; second = others.downRight; break;
				case 2: first = others.downRight; second = others.down; break;
				case 3: first = others.down; second = others.downLeft; break;
				case 4: first = others.downLeft; second = others.upLeft; break;
				case 5: first = others.upLeft; second = others.up; break;
				default: first = Vector2.zero; second = Vector2.zero; break;
			}
			// Geçerli konumlara sahip iki komşu bulunana kadar döngünün devam etmesi
			if (first.x < ZERO || first.x >= gridWidth || first.y < ZERO || first.y >= gridHeight || second.x < ZERO || second.x >= gridWidth || second.y < ZERO || second.y >= gridHeight)
			{
				selectionStatus = (++selectionStatus) % SELECTION_STATUS_COUNT;
			}else {
				breakTheLoop = true;
			}
		} while (!breakTheLoop);
	}

	
	// Tüm altıgenlerin dönmeyi bitirip bitirmediğini kontrol etme işlevi 
	private IEnumerator RotationCheckCoroutine(bool clockWise) 
	{
		List<Hexagon> explosiveHexagons = null;
		bool temp = true;

		// Seçili grubu bomba gelene kadar veya döndürülemeyecek altıgen grubuna ulaşılana kadar devam etsin
		hexagonRotationStatus = true;
		for (int i=0; i<selectedGroup.Count; ++i) 
		{
			/* Altıgenlerin 0.3 saniye dönmesini sağlamak */
			SwapHexagons(clockWise);
			yield return new WaitForSeconds(0.3f);

			/* Herhangi bir patlama olup olmadığını kontrol edin, 
			varsa döngüyü kır */
			explosiveHexagons = CheckExplosion(gameGrid);
			if (explosiveHexagons.Count > ZERO) 
			{
				break;
			}
		}

		// Dönüşün sona erdiği ve patlamanın başladığı 
		hexagonExplosionStatus = true;
		hexagonRotationStatus = false;

		// Altıgenleri patlayıcı altıgenler kalmayıncaya kadar patlatma 
		while (explosiveHexagons.Count > ZERO) 
		{
			if (temp) 
			{
				hexagonProductionStatus = true;
				StartCoroutine(ProduceHexagons(ExplodeHexagons(explosiveHexagons)));
				temp = false;
			}		
			else if (!hexagonProductionStatus) 
			{
				explosiveHexagons = CheckExplosion(gameGrid);
				temp = true;
			}
			yield return new WaitForSeconds(0.3f);
		}

		hexagonExplosionStatus = false;
		FindHexagonGroup();
		ConstructOutline();
	}



	// Seçili 3 altıgen grubunun konumlarını değiştirmek için yardımcı işlev
	private void SwapHexagons(bool clockWise) 
	{
		int x1, x2, x3, y1, y2, y3;
		Vector2 pos1, pos2, pos3;
		Hexagon first, second, third;
		
		// Her konum yerel değişkenlerde 
		first = selectedGroup[0];
		second = selectedGroup[1];
		third = selectedGroup[2];

		x1 = first.GetX();
		x2 = second.GetX();
		x3 = third.GetX();

		y1 = first.GetY();
		y2 = second.GetY();
		y3 = third.GetY();

		pos1 = first.transform.position;
		pos2 = second.transform.position;
		pos3 = third.transform.position;


		/* Döndürme saat yönündeyse, sonraki dizindeki öğenin konumuna dön, yoksa önceki dizine dön */
		if (clockWise) 
		{
			first.Rotate(x2, y2, pos2);
			gameGrid[x2][y2] = first;

			second.Rotate(x3, y3, pos3);
			gameGrid[x3][y3] = second;

			third.Rotate(x1, y1, pos1);
			gameGrid[x1][y1] = third;
		}else {
			first.Rotate(x3, y3, pos3);
			gameGrid[x3][y3] = first;

			second.Rotate(x1, y1, pos1);
			gameGrid[x1][y1] = second;

			third.Rotate(x2, y2, pos2);
			gameGrid[x2][y2] = third;
		}
	}
	
	// Patlamaya hazır altıgenleri içeren bir liste döndür, yoksa boş bir liste döndür. 
	private List<Hexagon> CheckExplosion(List<List<Hexagon>> listToCheck) 
	{
		List<Hexagon> otherList = new List<Hexagon>();
		List<Hexagon> explosiveList = new List<Hexagon>();
		Hexagon currentHexagon;
		Hexagon.OtherHexs currentOthers;
		Color currentColor;

		for (int i = 0; i<listToCheck.Count; ++i) 
		{
			for (int j = 0; j<listToCheck[i].Count; ++j) 
			{
				// Güncel altıgen bilgilerini al
				currentHexagon = listToCheck[i][j];
				currentColor = currentHexagon.GetColor();
				currentOthers = currentHexagon.GetOthers();

				// Diğer altıgen(others) listesini, geçerli pozisyonlara sahip dikey-düpedüz komşularla doldur
				if (IsValid(currentOthers.up)) otherList.Add(gameGrid[(int)currentOthers.up.x][(int)currentOthers.up.y]);
				else otherList.Add(null);

				if (IsValid(currentOthers.upRight)) otherList.Add(gameGrid[(int)currentOthers.upRight.x][(int)currentOthers.upRight.y]);
				else otherList.Add(null);

				if (IsValid(currentOthers.downRight)) otherList.Add(gameGrid[(int)currentOthers.downRight.x][(int)currentOthers.downRight.y]);
				else otherList.Add(null);

				// Mevcut 3 altıgenin hepsi aynı renkse, bunları patlama listesine ekle
				for (int k = 0; k<otherList.Count-1; ++k) 
				{
					if (otherList[k] != null && otherList[k+1] != null) 
					{
						if (otherList[k].GetColor() == currentColor && otherList[k+1].GetColor() == currentColor) 
						{
							if (!explosiveList.Contains(otherList[k]))
								explosiveList.Add(otherList[k]);
							if (!explosiveList.Contains(otherList[k+1]))
								explosiveList.Add(otherList[k+1]);
							if (!explosiveList.Contains(currentHexagon))
								explosiveList.Add(currentHexagon);
						}
					}
				}
				otherList.Clear();
			}
		}
		return explosiveList;
	}



	/* Patlayıcı altıgenleri(bomba) temizleme ve grid düzenleme işlevi */
	private List<int> ExplodeHexagons(List<Hexagon> list) {
		List<int> missingColumns = new List<int>();
		float positionX, positionY;
		
		// Bombaları kontrol et 
		foreach (Hexagon hex in bombs) 
		{
			if (!list.Contains(hex)) 
			{
				hex.Tick();
				if (hex.GetTimer() == ZERO) 
				{
					gameEnd = true;
					UserInterfaceManager.instance.GameEnd();
					StopAllCoroutines();
					return missingColumns;
				}
			}
		}

		// Altıgenleri oyun tablosundan kaldır 
		foreach (Hexagon hex in list) 
		{
			if (bombs.Contains(hex))
			{
				bombs.Remove(hex);
			}
			UserInterfaceManager.instance.Score(1);
			gameGrid[hex.GetX()].Remove(hex);
			missingColumns.Add(hex.GetX());
			Destroy(hex.gameObject);
			Instantiate(floatingText, transform.position, Quaternion.identity).GetComponent<TextMesh>().text = SCORE_CONSTANT.ToString();
			SoundManager.PlaySound("hexExp"); //her altıgen yok olduğunda ona göre ses dosyası
		}

		// Sol altıgen konumlarını yeniden ata 
		foreach (int i in missingColumns) 
		{
			for (int j=0; j<gameGrid[i].Count; ++j) 
			{
				positionX = GetGridStartCoordinateX() + (HEX_DISTANCE_HORIZONTAL * i);
				positionY = (HEX_DISTANCE_VERTICAL * j * 2) + GRID_VERTICAL_OFFSET + (OnStepper(i) ? HEX_DISTANCE_VERTICAL : ZERO);
				gameGrid[i][j].SetY(j);
				gameGrid[i][j].SetX(i);
				gameGrid[i][j].ChangeWorldPosition(new Vector3(positionX, positionY, ZERO));
			}
		}
		// Eksik sütun listesine dön 
		hexagonExplosionStatus = false;
		return missingColumns;
	}




	
	// outline nesnelerini temizleme işlevi 
	private void DestructOutline() 
	{
		if (outParent.transform.childCount > ZERO) 
		{
			foreach (Transform child in outParent.transform)
				Destroy(child.gameObject);
		}
	}
	
	/* outline oluşturma işlevi */
	private void ConstructOutline() 
	{
		// Seçili altıgen grubu alınması 
		FindHexagonGroup();
		
		foreach (Hexagon outlinedHexagon in selectedGroup)
		 {
			 //Seçilen altıgenin dışında outline nesnesi oluşturma
			GameObject go = outlinedHexagon.gameObject;
			GameObject outline = new GameObject("Outline");
			GameObject outlineInner = new GameObject("Inner Object");

			outline.transform.parent = outParent.transform;

			outline.AddComponent<SpriteRenderer>();
			outline.GetComponent<SpriteRenderer>().sprite = outlineSprite;
			outline.GetComponent<SpriteRenderer>().color = Color.white;
			outline.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
			outline.transform.localScale = HEX_OUTLINE_SCALE;

			outlineInner.AddComponent<SpriteRenderer>();
			outlineInner.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
			outlineInner.GetComponent<SpriteRenderer>().color = go.GetComponent<SpriteRenderer>().color;
			outlineInner.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -2);
			outlineInner.transform.localScale = go.transform.localScale;
			outlineInner.transform.parent = outline.transform;
		}
	}
	

	/* Yeni altıgenlerin üretilmesi */
	private IEnumerator ProduceHexagons(List<int> columns, List<List<Color>> colorSeed = null) 
	{
		Vector3 startPosition;
		float positionX, positionY;
		float startX = GetGridStartCoordinateX();
		bool stepperStatus;

		// Altıgen üretiminin başlangıcı için gösterge 
		hexagonProductionStatus = true;

		// Yeni altıgen üretme, değişkenleri ayarlama 
		foreach (int i in columns) 
		{
			// Yeni altıgeni örnekle ve biraz gecikme verme 
			stepperStatus = OnStepper(i);
			positionX = startX + (HEX_DISTANCE_HORIZONTAL * i);
			positionY = (HEX_DISTANCE_VERTICAL * gameGrid[i].Count * 2)  + GRID_VERTICAL_OFFSET + (stepperStatus ? HEX_DISTANCE_VERTICAL : ZERO);
			startPosition = new Vector3(positionX, positionY, ZERO);

			GameObject newObj = Instantiate(hexPrefab, HEX_START_POSITION, Quaternion.identity, hexParent.transform);
			Hexagon newHex = newObj.GetComponent<Hexagon>();
			yield return new WaitForSeconds(DELAY_TO_PRODUCE_HEXAGON);

			// Üretim sinyali geldiyse bombanın kurulması 
			if (bombProduction) 
			{
				newHex.SetBomb();
				bombs.Add(newHex);
				bombProduction = false;
			}

			// Altıgenin grid konumlarını ayarlama 
			if (colorSeed == null)
				newHex.SetColor(colorList[(int)(Random.value * RANDOM_SEED)%colorList.Count]);
			else 
				newHex.SetColor(colorSeed[i][gameGrid[i].Count]);

			newHex.ChangeGridPosition(new Vector2(i, gameGrid[i].Count));
			newHex.ChangeWorldPosition(startPosition);
			gameGrid[i].Add(newHex);
		}
		// Altıgen üretiminin sonu için gösterge
		hexagonProductionStatus = false;
	}



	/* Geçerli renklere sahip bir grid oluşturma işlevi */
	private List<List<Color>> ColoredGridProducer() 
	{
		List<List<Color>> returnValue = new List<List<Color>>();
		List<Color> checkList = new List<Color>();
		bool exit = true;

		// Diğer hexagonlar patlatmaya hazır olmadan bir renk listesi oluşturma 
		for (int i = 0; i<GetGridWidth(); ++i) 
		{
			returnValue.Add(new List<Color>());
			for (int j = 0; j<GetGridHeight(); ++j) 
			{
				returnValue[i].Add(colorList[(int)(Random.value * RANDOM_SEED)%colorList.Count]);
				do {
					exit = true;
					returnValue[i][j] = colorList[(int)(Random.value * RANDOM_SEED)%colorList.Count];
					if (i-1 >= ZERO && j-1 >= ZERO) 
					{
						if (returnValue[i][j-1] == returnValue[i][j] || returnValue[i-1][j] == returnValue[i][j])
							exit = false;
					}
				} while (!exit);
			}
		}
		return returnValue;
	}
	//hexagon step üzerinde mi?
	public bool OnStepper(int x) 
	{
		int midIndex = GetGridWidth()/HALF;
		return (midIndex%2 == x%2);
	}

	// Oyunun girdi almaya hazır olup olmadığını görmek için coroutine durum değişkenlerini kontrol eder
	public bool InputAvailabile() 
	{
		return !hexagonProductionStatus && !gameEnd && !hexagonRotationStatus && !hexagonExplosionStatus;
	}

	/* İlk sütun konumunun x koordinatını bulmak için yardımcı işlev */
	private float GetGridStartCoordinateX() 
	{
		return gridWidth/HALF * -HEX_DISTANCE_HORIZONTAL;
	}

	private bool IsValid(Vector2 pos) 
	{
		return pos.x >= ZERO && pos.x < GetGridWidth() && pos.y >= ZERO && pos.y <GetGridHeight();
	}

	private void PrintGameGrid() 
	{
		string map = "";

		for (int i = GetGridHeight()-1; i>=0; --i) 
		{
			for (int j = 0; j<GetGridWidth(); ++j) 
			{
				if (gameGrid[j][i] == null)
					map +=  "0 - ";
				else
					map += "1 - ";
			}

			map += "\n";
		}

		print(map);
	}
	


	/* Setters & Getters */
	public void SetGridWidth(int width) { gridWidth = width; }
	public void SetGridHeight(int height) { gridHeight = height; }
	public void SetColorList(List<Color> list) { colorList = list; }
	public void SetBombProduction() { bombProduction = true; }

	public int GetGridWidth() { return gridWidth; }
	public int GetGridHeight() { return gridHeight; }
	public Hexagon GetSelectedHexagon() { return selectedHexagon; }
}
