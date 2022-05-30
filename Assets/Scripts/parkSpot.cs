using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parkSpot : MonoBehaviour{
    public bool occupied = false;

    public float timeToPark = 2.0f;
    private float timer = 0f;
    bool carIn = false;
    bool carStopped = false;

    private parkstackManager manager;
    private Renderer rend;

    private void Awake() {
        manager = FindObjectOfType<parkstackManager>();
        rend = GetComponent<Renderer>();
    }

    private void Update() {
        rend.material.SetFloat("park_timer", timer / timeToPark);

        if (occupied) return;

        if (carIn && carStopped) {
            timer += Time.deltaTime;
        }
        else timer = 0;

        if(timer >= timeToPark) {
            Debug.Log("parked");
            manager.carParked();
            manager.freeSpaces--;
            occupied = true;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.GetComponent<carController>() || occupied) return;
        if (!collision.GetComponent<carController>().enabled) return;
        Debug.Log("in");
        rend.material.SetColor("_Color", Color.white);
        carIn = true;
        carController car = collision.GetComponent<carController>();
        car.OnCarStarted.AddListener(OnCarStarted);
        car.OnCarStopped.AddListener(OnCarStopped);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!collision.GetComponent<carController>() || occupied) return;
        rend.material.SetColor("_Color", Color.yellow);
        carIn = false;
        carController car = collision.GetComponent<carController>();
        car.OnCarStarted.RemoveListener(OnCarStarted);
        car.OnCarStopped.RemoveListener(OnCarStopped);
    }

    void OnCarStopped() {
        carStopped = true;
        Debug.Log("car stopped");
    }
    void OnCarStarted() => carStopped = false;
}
