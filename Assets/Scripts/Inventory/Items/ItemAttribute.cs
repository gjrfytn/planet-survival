using UnityEngine;
using System.Collections;

[System.Serializable]
public class ItemAttribute {

	public string AttributeName;
	public int AttributeValue;
	public ItemAttribute(string attributeName, int attributeValue)
	{
		this.AttributeName = attributeName;
		this.AttributeValue = attributeValue;
	}

	public ItemAttribute() {}
}
