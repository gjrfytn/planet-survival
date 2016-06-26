using UnityEngine;

public class DefenceAction: MonoBehaviour
{
	[SerializeField]
	float StaminaCost;
	[SerializeField,Range(0,1)]
	float Chance;
	[SerializeField,Range(0,1)]
	float DamageResist;

	public bool TryPerform(ref byte damage)
	{
		if(Random.value<Chance)
		{
			damage=(byte)Mathf.RoundToInt(damage*(1-DamageResist));
			return true;
		}
		return false;
	}
}
