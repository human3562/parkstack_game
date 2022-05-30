using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class carController : MonoBehaviour {
    public float maxSpeed = 20;
    public float reverseMultiplier = 0.5f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float driftFactor = 0.95f;
    public float turnSpeed = 4;

    float accelerationInput = 0;
    float steeringInput = 0;
    float velocityVsUp = 0;

    Rigidbody2D rb;

    //events
    private bool stopped = true;
    public UnityEvent OnCarStopped;
    public UnityEvent OnCarStarted;
    public UnityEvent OnCarCrashed;

    [ExecuteAlways]
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    //Player Input Bindings
    public void getInput_acceleration(InputAction.CallbackContext context){
        accelerationInput = context.ReadValue<float>(); stopped = false; OnCarStarted?.Invoke();
    }
    public void getInput_steering(InputAction.CallbackContext context) => steeringInput = context.ReadValue<float>();

    [ExecuteAlways]
    private void FixedUpdate() {
        ApplyEngineForce();
        KillOrthogonalVelocity();
        ApplySteering();
    }

    private void ApplyEngineForce() {
        velocityVsUp = Vector2.Dot(transform.right, rb.velocity);

        if (velocityVsUp > maxSpeed && accelerationInput > 0)
            return;
        if (velocityVsUp < -maxSpeed * reverseMultiplier && accelerationInput < 0)
            return;
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0)
            return;

        if (accelerationInput == 0) {
            rb.drag = Mathf.Lerp(rb.drag, 3.0f, Time.fixedDeltaTime * 3);
            if (Mathf.Abs(rb.velocity.magnitude) <= 0.1f) {
                if (!stopped) OnCarStopped?.Invoke();
                stopped = true;
            }
        }
        else rb.drag = 0;

        Vector2 engineForceVector = transform.right * accelerationInput * accelerationFactor;
        rb.AddForce(engineForceVector, ForceMode2D.Force);
    }

    public GameObject leftWheel;
    public GameObject rightWheel;
    private void ApplySteering() {
        float speedtoturn = Mathf.Clamp01(rb.velocity.magnitude / turnSpeed);
        //rotationAngle -= steeringInput * turnFactor * speedtoturn;
        if(leftWheel != null && rightWheel != null)
            leftWheel.transform.rotation = rightWheel.transform.rotation = Quaternion.Euler(0,0, -(steeringInput * turnFactor * turnFactor) + transform.rotation.eulerAngles.z);
        rb.AddTorque(-steeringInput * turnFactor * speedtoturn * Mathf.Sign(Vector2.Dot(rb.velocity, transform.right)), ForceMode2D.Force);
        //rb.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity() {
        Vector2 forwardVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        Vector2 sideVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);

        rb.velocity = forwardVelocity + sideVelocity * driftFactor;
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.isTrigger) return;
        OnCarCrashed?.Invoke();
    }
}
