using UnityEngine;
using System.Collections.Generic;

public class CarAgentDQN : MonoBehaviour
{
    public float speed = 5f;
    public float raycastLengthFront = 8f;
    public float raycastLengthSide = 5f;
    public float frontAngle = 10f;
    
    public GameObject gameOverPanel;
    public ScoreManager_sc scoreManager;
    public GameObject gamePauseButton;
    public GameObject restartButton;

    private Brain_sc brain;
    private Vector3 startPosition;
    private List<double> lastState;
    private bool isTraining = true;
    private float survivalTime = 0f;

    void Start()
    {
        brain = GetComponent<Brain_sc>();
        startPosition = transform.position;
        Debug.Log("CarAgentDQN Started!");
    }

    void Update()
    {
        if (!isTraining) return;

        // Raycast kontrolleri
        Vector2 frontDir = Vector2.up;
        Vector2 rightAngledDir = Quaternion.Euler(0, 0, -frontAngle) * Vector2.up;
        Vector2 leftAngledDir = Quaternion.Euler(0, 0, frontAngle) * Vector2.up;

        RaycastHit2D hitFront = Physics2D.Raycast(transform.position, frontDir, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitFrontRight = Physics2D.Raycast(transform.position, rightAngledDir, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitFrontLeft = Physics2D.Raycast(transform.position, leftAngledDir, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastLengthSide, LayerMask.GetMask("Cars"));
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastLengthSide, LayerMask.GetMask("Cars"));

        // State bilgilerini al
        lastState = brain.GetState(
            hitFront.collider != null ? hitFront.point - (Vector2)transform.position : frontDir * raycastLengthFront,
            hitFrontRight.collider != null ? hitFrontRight.point - (Vector2)transform.position : rightAngledDir * raycastLengthFront,
            hitFrontLeft.collider != null ? hitFrontLeft.point - (Vector2)transform.position : leftAngledDir * raycastLengthFront,
            hitRight.collider != null ? hitRight.point - (Vector2)transform.position : Vector2.right * raycastLengthSide,
            hitLeft.collider != null ? hitLeft.point - (Vector2)transform.position : Vector2.left * raycastLengthSide
        );

        // Eylem seç ve uygula
        int action = brain.DecideAction(lastState);
        float horizontalInput = action == 0 ? -1f : 1f;

        // Ödül hesapla
        double reward = 0.1f; // Hayatta kalma ödülü

        // Çarpışma yakınlığına göre ceza
        if (hitFront.collider != null)
        {
            reward = -1.0f;
        }

        brain.AddMemory(lastState, reward);
        Move(horizontalInput);
        
        survivalTime += Time.deltaTime;
        scoreManager.score = (int)survivalTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cars"))
        {
            Debug.Log("[COLLISION] Car hit!");
            double reward = -1.0f;
            brain.AddMemory(lastState, reward);
            brain.Train();
            brain.UpdateStats(survivalTime, true);
            
            ScoreManager_sc.lastScore = scoreManager.score;
            scoreManager.lastScoreText.text = "Last Score: " + ScoreManager_sc.lastScore.ToString();
            scoreManager.scoreText.text = scoreManager.score.ToString();
            
            gameOverPanel.SetActive(true);
            gamePauseButton.SetActive(false);
            restartButton.SetActive(false);
            
            ResetAgent();
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            Debug.Log("[COLLISION] Coin collected!");
            double reward = 1.0f;
            brain.AddMemory(lastState, reward);
            
            scoreManager.score += 10;
            Destroy(collision.gameObject);
        }
    }

    private void Move(float horizontalInput)
    {
        Vector3 movement = new Vector3(horizontalInput * speed * Time.deltaTime, 0, 0);
        transform.position += movement;

        // Ekran sınırları kontrolü
        float xPos = Mathf.Clamp(transform.position.x, -2.3f, 2.3f);
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    private void ResetAgent()
    {
        transform.position = startPosition;
        survivalTime = 0f;
        brain.ResetBrain();
        Invoke("HideGameOverPanel", 0.5f);
        gamePauseButton.SetActive(true);
        scoreManager.score = 0;
    }

    private void HideGameOverPanel()
    {
        gameOverPanel.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (!brain) return;

        Vector3 frontDir = Vector3.up;
        Vector3 rightAngledDir = Quaternion.Euler(0, 0, -frontAngle) * Vector3.up;
        Vector3 leftAngledDir = Quaternion.Euler(0, 0, frontAngle) * Vector3.up;

        // Raycast kontrolleri
        RaycastHit2D hitFront = Physics2D.Raycast(transform.position, frontDir, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitFrontRight = Physics2D.Raycast(transform.position, rightAngledDir, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitFrontLeft = Physics2D.Raycast(transform.position, leftAngledDir, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastLengthSide, LayerMask.GetMask("Cars"));
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastLengthSide, LayerMask.GetMask("Cars"));

        // Raycast çizgileri
        Gizmos.color = hitFront.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + frontDir * raycastLengthFront);
        
        Gizmos.color = hitFrontRight.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + rightAngledDir * raycastLengthFront);
        
        Gizmos.color = hitFrontLeft.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + leftAngledDir * raycastLengthFront);
        
        Gizmos.color = hitRight.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * raycastLengthSide);
        
        Gizmos.color = hitLeft.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * raycastLengthSide);

        // Çarpışma noktalarını göster
        if (hitFront.collider != null) Gizmos.DrawWireSphere(hitFront.point, 0.2f);
        if (hitFrontRight.collider != null) Gizmos.DrawWireSphere(hitFrontRight.point, 0.2f);
        if (hitFrontLeft.collider != null) Gizmos.DrawWireSphere(hitFrontLeft.point, 0.2f);
        if (hitRight.collider != null) Gizmos.DrawWireSphere(hitRight.point, 0.2f);
        if (hitLeft.collider != null) Gizmos.DrawWireSphere(hitLeft.point, 0.2f);
    }
}
