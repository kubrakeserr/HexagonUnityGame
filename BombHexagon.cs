using UnityEngine;

public class BombHexagon : Hexagon 
{
	// Hexagon sınıfında Tick() metodu çağırıldı 
	/* Bomba üzerine kurduğum zamanlama(Timer) her patlamadığında bir bir azalması 
	ve bunu üzerine TextMesh ile yazdırması */
	public TextMesh output;
	private int timeValue;

	public void Tick() 
	{ 
		timeValue -= 1; 
		output.text = timeValue.ToString();
	}
	public int GetTimeValue() 
	{ 
		return timeValue; 
	}
	public void SetTimeValue(int value) 
	{ 
		timeValue = value; 
		output.text = timeValue.ToString();
	}
}
