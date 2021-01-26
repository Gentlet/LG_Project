using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Hero : MonoBehaviour
{
    public float MoveSpeed = 4;
    public float RotationRate = 360;
    public Bullet BulletPrefab;
    public float TimeBetweenShots = 0.333f;

    private CharacterController _controller;

    private float _lastFire = Single.MinValue;
    private Vector3 _startingPos;
    private Quaternion _startingRot;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _startingPos = this.transform.position;
        _startingRot = this.transform.rotation;
    }

    void Update()
    {
        float vert = Input.GetAxis("Vertical");
        float hor = Input.GetAxis("Horizontal");

        bool isShootPressed = Input.GetButton("Jump"); // Space button

        _controller.transform.Rotate(Vector3.up, hor * RotationRate * Time.deltaTime, Space.Self);
        _controller.Move(vert * MoveSpeed * Time.deltaTime * transform.forward);

        if (isShootPressed && Time.time - _lastFire > TimeBetweenShots)
        {
            _lastFire = Time.time;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (BulletPrefab == null)
        {
            Debug.LogWarning("A BulletPrefab is needed!");
            return;
        }

        Instantiate(BulletPrefab, transform.position + transform.forward, transform.rotation);
    }

    public void PerformDeath()
    {
        Debug.Log("The hero is dead!");

        transform.position = _startingPos;
        transform.rotation = _startingRot;

        FindObjectOfType<GameManager>().RegisterDeath();
    }
}
