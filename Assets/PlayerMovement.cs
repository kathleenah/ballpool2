using UnityEngine;

public class PlayerMovement : MonoBehaviour{
    private Rigidbody2D rb;

    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    public int maxJumps = 2;
    public LayerMask groundLayer;
    public float rayLength = 0.6f;
    private bool isGrounded;
    private int jumpsRemaining;
    public float swimForce = 4f;
    private bool inWater;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }

    void Update(){
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        isGrounded = hit.collider != null;

        if(isGrounded && rb.linearVelocity.y <= 0.1f){
            jumpsRemaining = maxJumps;
        }

        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput*moveSpeed, rb.linearVelocity.y);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.yellow);

        if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.W)){
            if(inWater){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, swimForce);
            }else if(jumpsRemaining>0){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Water")){
            inWater=true;
            jumpsRemaining=maxJumps;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if(other.CompareTag("Water")){
            inWater=false;
        }
    }
}