using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    private Transform handTransform;
    private Image hand;
    public Vector2 offset;
    public Sprite idle;
    public Sprite click;
    [SerializeField] private float speed = 10;

    void Start()
    {
        handTransform = transform.GetChild(0);
        hand = handTransform.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0
        || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
            return;

        handTransform.position = Vector3.Lerp(handTransform.position, Input.mousePosition + new Vector3(offset.x, offset.y), speed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0)) hand.sprite = click;
        else if (Input.GetMouseButtonUp(0)) hand.sprite = idle;
    }
}
