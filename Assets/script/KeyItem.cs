using UnityEngine;
using System.Collections;

public class KeyItem : MonoBehaviour
{
    // static at to muzu kontrolovat z jinych skriptu bez hledani objektu
    public static bool isHoldingKey = false; 

    [Header("Zvuky")]
    public AudioClip pickupSound;

    private bool isPickedUp = false;
    private Collider2D keyCollider;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    void Start()
    {
        keyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    }

    void OnMouseDown()
    {
        // sebrat klic ze zeme
        if (!isPickedUp)
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
            }
            StartCoroutine(FollowMouseLogic());
        }
    }

    IEnumerator FollowMouseLogic()
    {
        isPickedUp = true;
        KeyItem.isHoldingKey = true; 

        keyCollider.enabled = false; // vypnout collider at neklikam na sebe
        spriteRenderer.sortingOrder = 100; // hodit klic navrh nad vsechno

        yield return new WaitForSeconds(0.1f);

        while (isPickedUp)
        {
            // klic lita za mysi
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePos;

            // leve kliknuti -> zkusit odemknout dvere
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);
                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Door"))
                    {
                        DoorItem doorScript = hit.GetComponent<DoorItem>();
                        if (doorScript != null)
                        {
                            doorScript.UnlockDoor(); 
                        }
                        
                        KeyItem.isHoldingKey = false; 
                        Destroy(gameObject); // smazat klic z mapy
                        yield break; 
                    }
                }
            }

            // prave kliknuti -> zahodit klic zpatky na zem
            if (Input.GetMouseButtonDown(1))
            {
                isPickedUp = false;
                KeyItem.isHoldingKey = false; 

                keyCollider.enabled = true;
                spriteRenderer.sortingOrder = originalSortingOrder;
            }

            yield return null;
        }
    }
}