﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float rotSpeed = 100f;
    [SerializeField] float thrustSpeed = 100f;
    [SerializeField] AudioClip audioMove;
    [SerializeField] AudioClip audioDie;

    [SerializeField] ParticleSystem partMove;
    [SerializeField] ParticleSystem partDie1;
    [SerializeField] ParticleSystem partDie2;

    Rigidbody rigidBody;
    AudioSource audioSource;
    bool btnThrust;
    bool btnLeft;
    bool btnRight;

    public enum State
    {
        Alive, Dead, Transition
    }
    public State state = State.Alive;
    float transitionTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        var intro = GameObject.Find("IntroString");
        if (intro == null)
        {
            audioSource.PlayOneShot(GameController.i.gameStart);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            GetInput();
            RespondToThrustInput();
            RespondToRotateInput();
        }
    }

    private void GetInput()
    {
            btnThrust = Input.GetKey(KeyCode.W);
            btnLeft = Input.GetKey(KeyCode.A);
            btnRight = Input.GetKey(KeyCode.D);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) return;

        switch (collision.gameObject.tag)
        {
            case "Finish":
                StartSuccessSequence();
                break;
            case "Friendly":
                break;
            default:
                StartFailSequence();
                partDie1.Play();
                partDie2.Play();
                break;
        }
    }

    private void StartFailSequence()
    {
        state = State.Dead;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(audioDie);
        audioSource.PlayOneShot(GameController.i.gameRestart);
        partDie1.Play();
        Invoke("RestartGame", transitionTime);
    }

    private void StartSuccessSequence()
    {
        state = State.Transition;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(GameController.i.gameNextLevel);
        Invoke("LoadNextLevel", transitionTime);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        var sceneIndex = SceneManager.GetActiveScene().buildIndex;
        var sceneCount = SceneManager.sceneCountInBuildSettings;
        if (sceneIndex < sceneCount - 1)
        {
            SceneManager.LoadScene(sceneIndex + 1);
        }
        else SceneManager.LoadScene(0);
    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true;    // Take manual control of rotation

        if (btnLeft) ApplyRotation(-1); else
        if (btnRight) ApplyRotation(1);

        rigidBody.freezeRotation = false;    // Resume physics control
    }

    private void ApplyRotation(float direction)
    {
        float rotAdjustedSpeed = rotSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward * rotAdjustedSpeed * direction);
    }

    private void RespondToThrustInput()
    {
        if (btnThrust) ApplyThrust();
    }

    private void ApplyThrust()
    {
        // Apply speed (offset by deltaTime).
        float thrustAdjustedSpeed = thrustSpeed * Time.deltaTime;
        rigidBody.AddRelativeForce(Vector3.up * thrustAdjustedSpeed);

        // Play movement audio.
        if (!audioSource.isPlaying)
        {
            audioSource.pitch = Random.Range(0.6f, 1f);
            audioSource.PlayOneShot(audioMove);
        }
        partMove.Play();
    }
}
