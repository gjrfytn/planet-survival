﻿using UnityEngine;
using System.Collections;

public class EntityMousePoint : MonoBehaviour
{
	Sprite healthBarSprite;
	Sprite healthBarFillerSprite;
	GameObject healthBar;
	GameObject healthBarFiller;

	void Start()
	{
		healthBarSprite=Resources.Load<Sprite>("Sprites/healthBar");
		healthBarFillerSprite=Resources.Load<Sprite>("Sprites/healthBarFiller");
	}

	//UNDONE
    public void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 1.1f;

		healthBar=new GameObject("healthBar");
		healthBar.AddComponent<SpriteRenderer>().sprite=healthBarSprite;
		healthBar.GetComponent<SpriteRenderer>().sortingLayerName="Player";
		healthBar.transform.position=new Vector2(transform.position.x,transform.position.y+0.5f);

		healthBarFiller=new GameObject("healthBarFiller");
		healthBarFiller.AddComponent<SpriteRenderer>().sprite=healthBarFillerSprite;
		healthBarFiller.GetComponent<SpriteRenderer>().sortingLayerName="Player";
		Creature c=GetComponent<Creature>();
		healthBarFiller.transform.position=new Vector2(transform.position.x-((c.MaxHealth-c.Health)/c.MaxHealth)*0.34f,transform.position.y+0.5f);
		healthBarFiller.transform.localScale=new Vector3(c.Health/c.MaxHealth,healthBarFiller.transform.lossyScale.y,healthBarFiller.transform.lossyScale.z);
    }

	//UNDONE
    public void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 0.909f;

		Destroy(healthBar);
		Destroy(healthBarFiller);
    }
}