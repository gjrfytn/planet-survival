/// <summary>
/// Move player.
/// РЈРїСЂР°РІР»РµРЅРёРµ РјС‹С€СЊСЋ РїРµСЂСЃРѕРЅР°Р¶РµРј
/// Р’РµС‰Р°РµС‚СЃСЏ РЅР° РїРµСЂСЃРѕРЅР°Р¶Р°
/// </summary>
using UnityEngine;
using System.Collections;

    public class MovePlayer : MonoBehaviour
     {
         public float stopStart = 1.5f, speed = 5f, rotationSpeed = 100f, heightPlayer = 1f;

         private float mag, angleToTarget;
         private Ray ray;
         private RaycastHit hit;
         private Vector3 dir;
         private Vector3 target = new Vector3();
         private Vector3 lastTarget = new Vector3();
	
		 public AnimationClip a_Idle;
		 public float a_IdleSpeed = 1;
		 public AnimationClip a_Walk;
		 public float a_WalkSpeed = 2;

         private bool walk;
	
		 private void Start(){
		 	GetComponent<Animation>()[a_Idle.name].speed = a_IdleSpeed;
			GetComponent<Animation>()[a_Walk.name].speed = a_WalkSpeed;
		 	GetComponent<Animation>().CrossFade(a_Idle.name);
		 }

         private void Update()
         {
             if (Input.GetMouseButton(1))
             {
                 ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                 if (Physics.Raycast(ray, out hit, 10000.0f))
                 {
                     target = hit.point;
                 }
             }
             LookAtThis();
             MoveTo();
         }

         private void CalculateAngle(Vector3 temp)
         {
             dir = new Vector3(temp.x, transform.position.y, temp.z) - transform.position;
             angleToTarget = Vector3.Angle(dir, transform.forward);
         }

         private void LookAtThis()
         {
                 if (target != lastTarget)
                 {
                     CalculateAngle(target);
                     if(angleToTarget > 3)
                         transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * UnityEngine.Time.deltaTime);
                 }
         }

         private void MoveTo()
         {
             if (target != lastTarget)
             {
                 if ((transform.position - target).sqrMagnitude > heightPlayer + 0.1f)
                 {
                     if (!walk)
                     {
						 GetComponent<Animation>().CrossFade(a_Walk.name);
                         walk = true;
                     }
                     mag = (transform.position - target).magnitude;
                     transform.position = Vector3.MoveTowards(transform.position, target, mag > stopStart ? speed * UnityEngine.Time.deltaTime : Mathf.Lerp(speed * 0.5f, speed, mag / stopStart) * UnityEngine.Time.deltaTime);
                     ray = new Ray(transform.position, -Vector3.up);
                     if (Physics.Raycast(ray, out hit, 1000.0f))
                     {
                         transform.position = new Vector3(transform.position.x, hit.point.y + heightPlayer, transform.position.z);
                     }
                 }
                 else
                 {
                     lastTarget = target;
                     if (walk)
                     {
						 GetComponent<Animation>().CrossFade(a_Idle.name);
                         walk = false;
                     }
                 }
             }
         }
     } 