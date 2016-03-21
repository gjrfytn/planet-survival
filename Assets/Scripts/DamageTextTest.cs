using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageTextTest : MonoBehaviour 
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
	}
	
	void OnDisable()
	{
		EventManager.CreatureHit -= ShowDamageText;
    }

	void Start()
	{
		CameraCanvas = GameObject.Find("CameraCanvas");
	}

	void ShowDamageText(GameObject obj,float damage)
	{
		GameObject textObj=Instantiate(Text);
		textObj.GetComponent<Text>().text=damage.ToString();
		textObj.GetComponent<Text>().color=new Color(1,1-damage/obj.GetComponent<Creature>().MaxHealth,0);
		textObj.transform.SetParent(CameraCanvas.transform);
		textObj.transform.localScale = Vector3.one;
		StartCoroutine(MoveHelper.Fly(textObj, new Vector2(obj.transform.position.x, obj.transform.position.y + Offset), new Vector2(obj.transform.position.x, obj.transform.position.y + FlyHeight),FlyTime));
		StartCoroutine(RenderHelper.FadeAndDestroyObject(textObj.GetComponent<CanvasRenderer>(), FadeTime));
	}
}
