using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SuperClass 
{
	private bool validTouch;
	private Grid GridObject;
	private Vector2 touchStartPosition;
	private Hexagon selectedHexagon;

	private void Start () 
	{
		GridObject = Grid.instance;
	}
	
	private void Update () 
	{
		if (GridObject.InputAvailabile() && Input.touchCount > ZERO) 
		{
			/* Dokunulan nesnenin çarpıştırıcısını bir değişkene alma */
			Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(ZERO).position);
			Vector2 touchPos = new Vector2(wp.x, wp.y);
			Collider2D collider = Physics2D.OverlapPoint(touchPos);
			selectedHexagon = GridObject.GetSelectedHexagon();
			
			TouchDetec();
			CheckSelection(collider);
			CheckRotation();
		}
	}

	// İlk dokunuşun gelip gelmediğini kontrol eder 
	private void TouchDetec() 
	{
		/* touch[0] başlangıç noktası ayarlama*/
		if (Input.GetTouch(ZERO).phase == TouchPhase.Began) 
		{
			validTouch = true;
			touchStartPosition = Input.GetTouch(ZERO).position;
		}
	}

	/* Seçim koşulunun sağlanıp sağlanmadığını kontrol eder 
	ve seçimi idare etmesi için grid'i çağırır */
	private void CheckSelection(Collider2D collider) 
	{
		/* Bir çarpıştırıcı varsa ve tag herhangi bir Hexagon ile eşleşiyorsa çalışmaya devam edin*/
		if (collider != null && collider.transform.tag == TAG_HEXAGON) 
		{
			/* Dokunma bittiyse altıgen seçin */
			if (Input.GetTouch(ZERO).phase == TouchPhase.Ended && validTouch) 
			{
				validTouch = false;
				GridObject.Select(collider);
			}
		}
	}

	/* Rotasyon koşulunun sağlanıp sağlanmadığını kontrol eder 
	ve rotasyonu yönetmesi için grid'i çağırır */
	private void CheckRotation() 
	{
		if (Input.GetTouch(ZERO).phase == TouchPhase.Moved && validTouch) 
		{
			Vector2 touchCurrentPosition = Input.GetTouch(ZERO).position;
			float distanceX = touchCurrentPosition.x - touchStartPosition.x;
			float distanceY = touchCurrentPosition.y - touchStartPosition.y;
			
			// İlk temas konumu ile geçerli dokunma konumu arasındaki mesafeyi karşılaştırarak dönüşün tetiklenip tetiklenmediğini kontrol edin
			if ((Mathf.Abs(distanceX) > HEX_ROTATE_SLIDE_DISTANCE || Mathf.Abs(distanceY) > HEX_ROTATE_SLIDE_DISTANCE) && selectedHexagon != null) 
			{
				Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectedHexagon.transform.position);

				bool triggerOnX = Mathf.Abs(distanceX) > Mathf.Abs(distanceY);
				bool swipeRightUp = triggerOnX ? distanceX > ZERO : distanceY > ZERO;
				bool touchThanHex = triggerOnX ? touchCurrentPosition.y > screenPosition.y : touchCurrentPosition.x > screenPosition.x;
				bool clockWise = triggerOnX ? swipeRightUp == touchThanHex : swipeRightUp != touchThanHex;

				validTouch = false; 
				GridObject.Rotate(clockWise); //Grid içerisinde oluşan hexagonların saat yönünde çevrilmesini sağlar
				SoundManager.PlaySound("hexTurn"); //Her 3'lü grup olan altıgen çevrildiğinde ses gelmesini çağırır
			}
		}
	}
}
