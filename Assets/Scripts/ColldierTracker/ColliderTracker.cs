using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTracker : MonoBehaviour {
    public List<Collider2D> colliders = new List<Collider2D>();
    
    void OnTriggerEnter2D(Collider2D other) {
        if (!colliders.Contains(other)) colliders.Add(other);
        for (int i = colliders.Count - 1; i >= 0; i--) {
            if (colliders[i] == null) colliders.RemoveAt(i);
        }
    }
    
    void OnTriggerExit2D(Collider2D other) {
        colliders.Remove(other);
    }
}
