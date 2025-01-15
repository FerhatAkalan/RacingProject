using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class CarAgent : Agent
{
    public float speed = 5f; // Hareket hızı
    public float rotationSpeed = 8f; // Dönüş hızı
    public GameObject gameOverPanel;
    public ScoreManager_sc scoreManager;
    public GameObject gamePauseButton;
    public GameObject restartButton;
    private Vector3 startPosition = new Vector3(0, -4, 0); // Başlangıç 
    private Quaternion startRotation = Quaternion.Euler(0, 0, 90); // Başlangıç rotasyonu
    public float maxRotationAngle = 47f;
    public float raycastAngle = 5f;  // Raycast açısı (derece cinsinden)
    public float raycastLengthFront = 4f;
    public float raycastLengthSide = 4f; 
    public float maxRayDistance = 4f;  


    void Start()
    {
        Debug.Log("PlayerAgent Started!");
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("PlayerAgent Initialized!");
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("New Episode Started!");
        transform.position = startPosition;
        transform.rotation = startRotation;
        Invoke("HideGameOverPanel", 0.5f);
        gamePauseButton.SetActive(true);
        scoreManager.score = 0;
    }

    private void HideGameOverPanel()
    {
        gameOverPanel.SetActive(false);
    }
   public override void CollectObservations(VectorSensor sensor)
    {
        float[] rayAngles = { 0f, raycastAngle, -raycastAngle, 90f, -90f };  // Açıyı değişkenden al

        foreach (float angle in rayAngles)
        {
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxRayDistance, LayerMask.GetMask("Cars"));
            
            float distance = hit.collider != null ? 1- (hit.distance / maxRayDistance) : 0.0f;
            sensor.AddObservation(distance);

            Color rayColor = hit.collider != null ? Color.red : Color.green;
            Debug.DrawRay(transform.position, direction * maxRayDistance, rayColor);
        }
        
        sensor.AddObservation(transform.position.x / 2.2f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontalInput = actions.ContinuousActions[0];
        
        // Raycast kontrolleri
        RaycastHit2D hitFront = Physics2D.Raycast(transform.position, Vector2.up, maxRayDistance, LayerMask.GetMask("Cars"));
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, maxRayDistance, LayerMask.GetMask("Cars"));
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, maxRayDistance, LayerMask.GetMask("Cars"));
        
        // 10 derece açılı raycast'ler
        Vector2 rightAngled = Quaternion.Euler(0, 0, -raycastAngle) * Vector2.up;  // Açıyı değişkenden al
        Vector2 leftAngled = Quaternion.Euler(0, 0, raycastAngle) * Vector2.up;    // Açıyı değişkenden al
        RaycastHit2D hitFrontRight = Physics2D.Raycast(transform.position, rightAngled, maxRayDistance, LayerMask.GetMask("Cars"));
        RaycastHit2D hitFrontLeft = Physics2D.Raycast(transform.position, leftAngled, maxRayDistance, LayerMask.GetMask("Cars"));

        // Tehlike tespiti
        bool frontDanger = hitFront.collider != null && hitFront.distance < 3f;
        bool frontRightDanger = hitFrontRight.collider != null && hitFrontRight.distance < 3f;
        bool frontLeftDanger = hitFrontLeft.collider != null && hitFrontLeft.distance < 3f;

        if (frontDanger || frontRightDanger || frontLeftDanger)
        {
            AddReward(-0.01f); // Tehlike durumu genel cezası
            Debug.Log("Çarpışma var");
            // Mevcut pozisyona göre kaçış yönü belirleme
            float currentX = transform.position.x;
            
            // Sağ tarafta tehlike veya sınıra yakınsa
            if ((frontRightDanger || currentX > 1.8f) && hitLeft.collider == null && currentX > -2.0f)
            {
                horizontalInput = -1f; // Sola kaç
                AddReward(0.005f); // Doğru kaçınma ödülü
            }
            // Sol tarafta tehlike veya sınıra yakınsa
            else if ((frontLeftDanger || currentX < -1.8f) && hitRight.collider == null && currentX < 2.0f)
            {
                horizontalInput = 1f; // Sağa kaç
                AddReward(0.005f); // Doğru kaçınma ödülü
            }
            // Sadece önde tehlike varsa
            else if (frontDanger)
            {
                // Orta noktaya göre kaçış yönü seç
                if (currentX > 0 && hitLeft.collider == null)
                {
                    horizontalInput = -1f;
                }
                else if (currentX <= 0 && hitRight.collider == null)
                {
                    horizontalInput = 1f;
                }
            }
        }
        else
        {
            // Güvenli sürüş ödülü
            AddReward(0.001f);
            
            // Orta şeride dönme eğilimi
            float distanceFromCenter = Mathf.Abs(transform.position.x);
            if (distanceFromCenter > 1.0f)
            {
                horizontalInput = transform.position.x > 0 ? -0.5f : 0.5f;
            }
        }

        // Hareket ve pozisyon sınırlama
        Vector3 movement = new Vector3(
            horizontalInput * speed * Time.deltaTime,
            0,
            0
        );
        transform.position += movement;

        // Pozisyonu sınırla
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -2.2f, 2.2f),
            transform.position.y,
            transform.position.z
        );

        // Rotasyon güncelleme
        float targetAngle = 90;
        if (horizontalInput > 0)
            targetAngle = 60;
        else if (horizontalInput < 0)
            targetAngle = 120;
        
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        Debug.Log($"Current Reward: {GetCumulativeReward()}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cars"))
        {
            Debug.Log("Collision with Car - Episode Ended");
            Debug.Log($"Final Score: {scoreManager.score}, Final Reward: {GetCumulativeReward()}");
            AddReward(-1.0f);
            ScoreManager_sc.lastScore = scoreManager.score;
            scoreManager.lastScoreText.text = "Last Score: " + ScoreManager_sc.lastScore.ToString();
            scoreManager.scoreText.text = scoreManager.score.ToString();
            gameOverPanel.SetActive(true);
            gamePauseButton.SetActive(false);
            restartButton.SetActive(false);
            Destroy(collision.gameObject);
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("Coin"))
        {
            Debug.Log($"Coin Collected - Score: {scoreManager.score + 10}, Reward Added: 0.1");
            // Coin toplandığında ödül ver
            AddReward(0.1f);
            scoreManager.score += 10;
            Destroy(collision.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        // Raycast kontrolleri
        RaycastHit2D hitFront = Physics2D.Raycast(transform.position, Vector2.up, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastLengthSide, LayerMask.GetMask("Cars"));
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastLengthSide, LayerMask.GetMask("Cars"));
        
        // Açılı raycast'ler
        Vector2 rightAngled = Quaternion.Euler(0, 0, -raycastAngle) * Vector2.up;  // Açıyı değişkenden al
        Vector2 leftAngled = Quaternion.Euler(0, 0, raycastAngle) * Vector2.up;    // Açıyı değişkenden al
        RaycastHit2D hitFrontRight = Physics2D.Raycast(transform.position, rightAngled, raycastLengthFront, LayerMask.GetMask("Cars"));
        RaycastHit2D hitFrontLeft = Physics2D.Raycast(transform.position, leftAngled, raycastLengthFront, LayerMask.GetMask("Cars"));

        // Düz raycast'ler çizimi
        Gizmos.color = hitFront.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * raycastLengthFront);
        
        Gizmos.color = hitRight.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * raycastLengthSide);
        
        Gizmos.color = hitLeft.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * raycastLengthSide);

        // Açılı raycast'ler çizimi
        Gizmos.color = hitFrontRight.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(rightAngled * raycastLengthFront));
        
        Gizmos.color = hitFrontLeft.collider != null ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(leftAngled * raycastLengthFront));
    }
}
