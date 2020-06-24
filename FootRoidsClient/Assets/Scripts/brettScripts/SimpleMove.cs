using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour {

    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _rotationSpeed = 3f;
    [SerializeField] private Rigidbody2D _rigidbody;
    
    // Start is called before the first frame update
    void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        var vInput = Input.GetAxis("Vertical") * _speed;
        var hInput = Input.GetAxis("Horizontal") * _rotationSpeed;

        _rigidbody.AddForce(new Vector2(0, vInput));
        _rigidbody.AddTorque(-hInput);
    }
}