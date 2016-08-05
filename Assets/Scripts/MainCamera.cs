using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField]
    float ZoomSpeed;
    [SerializeField]
    Player Player;
    [SerializeField]
    World World;

    Vector3 LocalMapTopRight;

    void OnEnable()
    {
        EventManager.PlayerObjectMoved += FollowPlayer;
    }

    void OnDisable()
    {
        EventManager.PlayerObjectMoved -= FollowPlayer;
    }

    void Start()
    {
        LocalMapTopRight = new Vector3(World.LocalMapSize.X * WorldVisualizer.LocalHexSpriteSize.x - WorldVisualizer.LocalHexSpriteSize.x * 0.5f, World.LocalMapSize.Y * WorldVisualizer.LocalHexSpriteSize.y * 0.75f - WorldVisualizer.LocalHexSpriteSize.y * 0.5f, 0);
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (World.IsCurrentMapLocal())
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
                Camera.main.orthographicSize -= scroll;
                Vector2 zeroPoint = Camera.main.WorldToScreenPoint(Vector3.zero);
                Vector2 topRightPoint = Camera.main.WorldToScreenPoint(LocalMapTopRight);
                if (zeroPoint.x > 0 && topRightPoint.x < Screen.width && zeroPoint.y > 0 && topRightPoint.y < Screen.height)
                    Camera.main.orthographicSize += scroll;
            }
            else
                Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
        }
    }

    void FollowPlayer()
    {
        Vector3 buf = transform.position;
        transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, transform.position.z);
        Vector2 zeroPoint = Camera.main.WorldToScreenPoint(Vector3.zero);
        Vector2 topRightPoint = Camera.main.WorldToScreenPoint(LocalMapTopRight);
        if (World.IsCurrentMapLocal())
        {
            if (zeroPoint.y > 0 || topRightPoint.y < Screen.height)
                transform.position = new Vector3(transform.position.x, buf.y, buf.z);
            if (zeroPoint.x > 0 || topRightPoint.x < Screen.width)
                transform.position = new Vector3(buf.x, transform.position.y, buf.z);
        }
    }
}
