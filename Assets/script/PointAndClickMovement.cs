using UnityEngine;

public class PointAndClickMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 targetPosition;
    private Animator animator;
    private bool isMoving = false;
    
    private Collider2D floorCollider; 

    void Start()
    {
        targetPosition = transform.position;
        animator = GetComponent<Animator>();
        
        // najit podlahu podle tagu
        GameObject floor = GameObject.FindGameObjectWithTag("Floor");
        if (floor != null)
        {
            floorCollider = floor.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        // kliknuti na pohyb
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            if (floorCollider != null)
            {
                // hrac dojde k nejblizsimu bodu na podlaze i kdyz kliknu mimo ni
                targetPosition = floorCollider.ClosestPoint(mousePos);
                isMoving = true;
            }
        }

        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            animator.SetBool("isWalking", true);

            // stopnuti kousek pred cílem at to necuká
            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                isMoving = false;
                animator.SetBool("isWalking", false);
            }
        }
    }
}