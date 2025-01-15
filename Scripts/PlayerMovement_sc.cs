using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement_sc : MonoBehaviour
{
    public float speed = 5f; // Hareket hızı
    public float rotationSpeed = 5f; // Dönüş hızı
    public float maxRotationAngle = 47f; // Maksimum dönüş açısı
    private Vector3 startPosition = new Vector3(0, -4, 0); // Başlangıç pozisyonu
    private Quaternion startRotation = Quaternion.Euler(0, 0, 90); // Başlangıç rotasyonu
    public ScoreManager_sc scoreValue;
    public GameObject gameOverPanel;
    public GameObject gamePauseButton;

    void Start()
    {
        // Başlangıç pozisyonu ve rotasyonunu ayarla
        transform.position = startPosition;
        transform.rotation = startRotation;
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        Movement();
        Clamp();
    }

    void Movement()
    {
        // Hareket
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right * speed * Time.deltaTime;
            RotateToAngle(60); // Sağ dönüş
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left * speed * Time.deltaTime;
            RotateToAngle(120); // Sol dönüş
        }
        else
        {
            RotateToAngle(90); // Dönüş sıfırlama
        }

        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.down * speed * Time.deltaTime;
        }

        // Yeni pozisyonu uygula
        transform.position += movement;
    }

    void RotateToAngle(float targetAngle)
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Clamp()
    {
        // X ekseninde pozisyon sınırlama
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -2.1f, 2.1f);
        pos.y = Mathf.Clamp(pos.y, -4.3f, 4.3f);
        transform.position = pos;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Cars")
        {
            Time.timeScale = 0;
            gameOverPanel.SetActive(true);
            gamePauseButton.SetActive(false);
        }
        if(collision.gameObject.tag == "Coin")
        {
            scoreValue.score += 10;
            Destroy(collision.gameObject);
        }
    }
}
