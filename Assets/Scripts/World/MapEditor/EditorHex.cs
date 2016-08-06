using UnityEngine;

using LocalPos = U16Vec2;

public class EditorHex : MonoBehaviour
{
    const float HighlightPower = 1.3f;

    bool PosSet = false;
    LocalPos Pos_;
    public LocalPos Pos
    {
        get
        {
            return Pos_;
        }
        set
        {
            if (PosSet)
                throw new System.InvalidOperationException("Pos has already been set.");
            else
            {
                Pos_ = value;
                PosSet = true;
            }
        }
    }

    public void OnMouseUpAsButton()
    {

    }

    public void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * HighlightPower;
    }

    public void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color / HighlightPower;
    }
}
