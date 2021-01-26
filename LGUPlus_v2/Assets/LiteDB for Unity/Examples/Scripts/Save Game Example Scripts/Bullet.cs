using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float MoveSpeed = 10;
    public float BulletLifeTime = 12;

    void Start()
    {
        Destroy(this, BulletLifeTime);

        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = transform.forward * MoveSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<BadGuy>() != null)
        {
            Destroy(collision.gameObject);
            FindObjectOfType<GameManager>().RegisterKill();
        }

        Destroy(gameObject);
    }
}
