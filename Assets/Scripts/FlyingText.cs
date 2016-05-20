﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlyingText : MonoBehaviour
{
	public GameObject Text;
	public float Offset;
	public float FlyHeight;
	public float FlyTime;
	public float FadeTime;
	
	GameObject CameraCanvas;
	
	void OnEnable()
	{
		EventManager.CreatureHit += ShowDamageText;
		EventManager.AttackMissed+=ShowMissText;
	}
	
	void OnDisable()
	{
		EventManager.CreatureHit -= ShowDamageText;
		EventManager.AttackMissed-=ShowMissText;
	}
	
	void Start()
	{
		CameraCanvas = GameObject.Find("CameraCanvas");
	}
	
	void ShowDamageText(GameObject obj, byte damage)
	{
		GameObject textObj = Instantiate(Text);
		textObj.GetComponent<Text>().text = damage.ToString();
		textObj.GetComponent<Text>().color = new Color(1, 1 - damage / obj.GetComponent<Creature>().MaxHealth, 0);
		textObj.transform.SetParent(CameraCanvas.transform);
		textObj.transform.localScale = Vector3.one;
		StartCoroutine(MoveHelper.Fly(textObj, new Vector2(obj.transform.position.x, obj.transform.position.y + Offset), new Vector2(obj.transform.position.x, obj.transform.position.y + FlyHeight), FlyTime));
		textObj.AddComponent<Fader>().FadeAndDestroyObject(FadeTime);
	}
	
	void ShowMissText(Vector2 pos)
	{
		GameObject textObj = Instantiate(Text);
		textObj.GetComponent<Text>().text = "Miss";
		textObj.GetComponent<Text>().color = Color.red;
		textObj.transform.SetParent(CameraCanvas.transform);
		textObj.transform.localScale = Vector3.one;
		StartCoroutine(MoveHelper.Fly(textObj, new Vector2(pos.x, pos.y + Offset), new Vector2(pos.x, pos.y + FlyHeight), FlyTime));
		textObj.AddComponent<Fader>().FadeAndDestroyObject(FadeTime);
	}
}