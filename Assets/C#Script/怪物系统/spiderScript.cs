using UnityEngine;

public class spiderScript : Enemybase
{
    public GameObject boom;

    private bool hasDied;

    public override void hit(float harm)
    {
        base.hit(harm);
    }

    public override void OnSpawn()
    {
        hasDied = false;
        base.OnSpawn();
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        hasDied = false;
    }

    public override void Die()
    {
        if (hasDied)
            return;

        hasDied = true;

        Debug.Log("蜘蛛爆炸效果");

        if (boom != null)
        {
            GameObject effectObject = Instantiate(
                boom,
                transform.position,
                transform.rotation
            );

            Destroy(effectObject, 2f);
        }

        base.Die();
    }
}
