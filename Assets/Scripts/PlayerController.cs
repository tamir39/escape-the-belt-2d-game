using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    public float thrustForce = 10f;
    public float maxSpeed = 5f;
    
    private float elapsedTime = 0f;
    private float score = 0f;
    public float scoreMultiplier = 10f;
    public int endingPoint = 500;

    public UIDocument uIDocument;
    private Label scoreText;
    private float highScore = 0f;
    private Label highScoreText;
    private Label newBestLabel;
    private bool recordNotified = false;

    private Button restartButton;
    private Button settingButton;
    private Button closingSettingButton;
    private VisualElement settingPanel;
    private Label settingPanelLabel;

    public GameObject yellowBoosterFlame;
    public GameObject redBoosterFlame;
    public GameObject explosionEffect;
    public GameObject borderParent;
    public AudioSource audioSource;
    public AudioClip thrustSound;
    public AudioClip explosionSound;
    public InputAction moveForward;
    public InputAction lookPosition;


    private VisualElement progressBarFill;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        var root = uIDocument.rootVisualElement;
        scoreText = root.Q<Label>("ScoreLable");
        highScoreText = root.Q<Label>("HighScoreLabel");
        newBestLabel = root.Q<Label>("NewScoreLabel");
        newBestLabel.style.display = DisplayStyle.None;

        restartButton = root.Q<Button>("RestartButton");
        restartButton.style.display = DisplayStyle.None;
        restartButton.clicked += ReloadScene;

        settingButton = root.Q<Button>("SettingButton");
        settingPanel = root.Q<VisualElement>("SettingPanel");
        settingPanelLabel = root.Q<Label>("SettingPanelLabel");
        closingSettingButton = root.Q<Button>("CloseSettingButton");
        settingPanel.style.display = DisplayStyle.None;

        if (settingButton != null)
        {
            settingButton.clicked += OpenSettings;
        }

        if (closingSettingButton != null)
        {
            closingSettingButton.clicked += CloseSettings;
        }

        highScore = PlayerPrefs.GetFloat("HighScore", 0f);

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        

        progressBarFill = root.Q<VisualElement>("ProgressBarFill");

        moveForward.Enable();
        lookPosition.Enable();
    }
    void Update()
    {
        UpdateScore();
        MovePlayer();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteKey("HighScore");
            highScore = 0;
            if (highScoreText != null) highScoreText.text = "High Score: 0";
        }
        //Toggle setting
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettings();
        }
    }

    void ToggleSettings()
    {
        if (settingPanel == null) return;

        // Is that displaying?
        bool isSettingsOpen = settingPanel.style.display == DisplayStyle.Flex;

        if (isSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }
    void OpenSettings()
    {
        if (settingPanel != null)
        {
            settingPanel.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f;
            moveForward.Disable();
            settingButton.style.display = DisplayStyle.None;

            if (closingSettingButton != null)
            {
                Debug.Log("Close: " + closingSettingButton.pickingMode);
            }
            else
            {
                Debug.LogError("No!");
            }
        }
    }
    void CloseSettings()
    {
        if (settingPanel != null)
        {
            settingPanel.style.display = DisplayStyle.None;
            Time.timeScale = 1f;

            moveForward.Enable();

            settingButton.style.display = DisplayStyle.Flex;
        }
    }

    // Update is called once per frame
    void UpdateScore()
    {
        elapsedTime += Time.deltaTime;
        score = Mathf.FloorToInt(elapsedTime * scoreMultiplier);
        scoreText.text = "Score: " + score;

        if (score > highScore && highScore > 0)
        {
            if (!recordNotified)
            {
                newBestLabel.style.display = DisplayStyle.Flex;
                recordNotified = true;
            }
        }

        float progress = (float)score / endingPoint;
        if (progressBarFill != null)
        {
            progressBarFill.style.width = new Length(progress * 20, LengthUnit.Percent);
        }

        if (progress >= 0.5f)
        {
            progressBarFill.style.backgroundColor = Color.red;
        }

        if (score >= endingPoint)
        {
            WinGame();
        }
    }

    void MovePlayer()
    {
        if (moveForward.IsPressed())
        {
            // Calculate mouse direction
            Vector2 pointerScreenPos = lookPosition.ReadValue<Vector2>();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(pointerScreenPos);

            Vector2 direction = (mousePos - transform.position).normalized;

            transform.up = direction;
            rb.AddForce(direction * thrustForce);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        // Active BoosterFlame and Sound
        if (moveForward.WasPressedThisFrame())
        {
            if (audioSource != null && thrustSound != null)
            {
                audioSource.PlayOneShot(thrustSound);
            }

            yellowBoosterFlame.SetActive(true);
            redBoosterFlame.SetActive(true);
        }
        else if (moveForward.WasReleasedThisFrame())
        {
            yellowBoosterFlame.SetActive(false);
            redBoosterFlame.SetActive(false);
        }
    }

    void setNewHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
        }
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        setNewHighScore();

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        Instantiate(explosionEffect, transform.position, transform.rotation);
        gameObject.SetActive(false);
        restartButton.style.display = DisplayStyle.Flex;

        borderParent.SetActive(false);
    }

    void ReloadScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void WinGame()
    {
        setNewHighScore();
        if (restartButton != null)
        {
            restartButton.style.display = DisplayStyle.Flex;
            restartButton.text = "CONGRATS, YOU'VE REACHED THE END!";
            gameObject.SetActive(false);
            borderParent.SetActive(false);
        }
    }

    public float GetCurrentScore() => score;
    //void OnDisable()
    //{
    //    if (restartButton != null) restartButton.clicked -= ReloadScene;
    //    if (settingButton != null) settingButton.clicked -= OnSettingClicked;
    //}
}
