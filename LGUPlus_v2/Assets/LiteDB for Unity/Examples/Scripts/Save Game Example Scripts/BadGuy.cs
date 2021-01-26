using UnityEngine;

public class BadGuy : MonoBehaviour
{
    public float MoveSpeed = 2;
    public float TurnSpeed = 270;

    private Hero _hero;

    
    void Start()
    {
        _hero = FindObjectOfType<Hero>();
    }

    void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody>();

        Vector3 heroDirectionVec = new Vector3(_hero.transform.position.x, 0, _hero.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        Quaternion heroDirection = Quaternion.LookRotation(heroDirectionVec);
        float maxDegrees = TurnSpeed * Time.fixedDeltaTime;

        rb.rotation = Quaternion.RotateTowards(transform.rotation, heroDirection, maxDegrees);
        rb.position += transform.forward * MoveSpeed * Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        Hero collidedHero = collision.gameObject.GetComponent<Hero>();
        if (collidedHero != null)
        {
            collidedHero.PerformDeath();
            Destroy(gameObject);
        }
    }
}
