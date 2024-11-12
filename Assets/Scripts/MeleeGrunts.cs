using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeGrunts : Enemy
{
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        rb.gravityScale = 12f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!isRecoiling)
        {
            Vector2 targetPosition = new Vector2(PLayerController.Instance.transform.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Flip the direction based on movement
            if (targetPosition.x < transform.position.x)
            {
                // Moving left, flip to face left
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (targetPosition.x > transform.position.x)
            {
                // Moving right, flip to face right
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    public override void Enemyhit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.Enemyhit(_damageDone, _hitDirection, _hitForce);
    }
}
