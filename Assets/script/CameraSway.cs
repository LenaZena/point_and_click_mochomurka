using UnityEngine;

public class CameraSway : MonoBehaviour
{
    [Header("pohyb kamery")]
    public float swayAmount = 0.3f;
    public float tiltAmount = 1.5f;
    public float speed = 2.5f;

    // puvodni pozice kamery na zacatku
    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        // pozice myši 0-1
        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        // hodit do -1 az 1 at to chodi do obou stran
        float offsetX = Mathf.Clamp((mouseX - 0.5f) * 2f, -1f, 1f);
        float offsetY = Mathf.Clamp((mouseY - 0.5f) * 2f, -1f, 1f);

        Vector3 targetPos = startPos + new Vector3(offsetX * swayAmount, offsetY * swayAmount, 0);
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, -offsetX * tiltAmount);

        // plynuly pohyb kamery at to neskace
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * speed);
    }
}