using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletJones : MonoBehaviour {
    public Rigidbody2D currentPelletBody;

    Rect closeRect = new Rect(-0.5f, -0.5f, 1.0f, 1.0f);
    GameObject[] pellets;

    void Start() {
        pellets = GameObject.FindGameObjectsWithTag("pellet");
    }

    void Update() {
        for (int i = 0; i < pellets.Length; i++) {
            var pelletBody = pellets[i].GetComponent<Rigidbody2D>();
            if (Object.ReferenceEquals(pelletBody, currentPelletBody)) continue;
            var posDelta = pelletBody.position - currentPelletBody.position;
            if (!closeRect.Contains(posDelta)) continue;
            var dist = posDelta.magnitude;
            if (dist == 0.0f) continue;
            currentPelletBody.AddForce(Vector2.ClampMagnitude(posDelta / dist / dist / dist * -0.0004f, 0.01f), ForceMode2D.Impulse);
        }
    }
}
