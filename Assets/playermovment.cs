using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    private float horizontalMovement;
    private bool canMove = true;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float fallSpeedMultiplier = 2f;
    public float maxFallSpeed = 18f;

    [Header("Respawn")]
    private Transform respawnPoint;

    [Header("UI")]
    public Text piwoText;       // UI-текст для подсчета бутылок
    public GameObject winText;  // Объект с текстом "You Win"

    private int bottleCount = 0;

    void Start()
    {
        jumpsRemaining = maxJumps;

        // Ищем объект Respawn по тегу
        GameObject respawnObj = GameObject.FindGameObjectWithTag("Respawn");
        if (respawnObj != null)
        {
            respawnPoint = respawnObj.transform;
        }
        else
        {
            Debug.LogError("Объект с тегом 'Respawn' не найден!");
        }

        // Скрываем текст победы при старте
        if (winText != null)
            winText.SetActive(false);

        UpdatePiwoUI();
    }

    void Update()
    {
        GroundCheck();

        if (canMove)
        {
            MovePlayer();
            ApplyGravity();
        }

        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }
    }

    private void MovePlayer()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
    }

    private void ApplyGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer);
        Debug.DrawLine(groundCheckPos.position - (Vector3)groundCheckSize / 2, groundCheckPos.position + (Vector3)groundCheckSize / 2, Color.green);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpsRemaining > 0)
        {
            rb.gravityScale = baseGravity;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpsRemaining--;
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FallDetector") && respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            rb.linearVelocity = Vector2.zero;
        }

        if (collision.CompareTag("Coin"))
        {
            bottleCount++;
            UpdatePiwoUI();
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Finish"))
        {
            if (winText != null)
                winText.SetActive(true);
        }
    }

    private void UpdatePiwoUI()
    {
        if (piwoText != null)
        {
            piwoText.text = "Piwo: " + bottleCount.ToString();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        }
    }
}

