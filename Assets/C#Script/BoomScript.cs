using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomScript : MonoBehaviour
{
    public float boomRadius = 5f;
    public float boomdamge = 50f;
    //boom声
    private AudioSource boomAudio;
    public AudioClip boomClip;
    // Start is called before the first frame update
    void Start()
    {
        boomAudio = GetComponent<AudioSource>();
        boomAudio.PlayOneShot(boomClip);
        boom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void boom()
    {
        Collider[] hitcolliders = Physics.OverlapSphere(transform.position, boomRadius);
        foreach (var col in hitcolliders)
        {
            EnemyScript ES =col.GetComponent<EnemyScript>();
            PlayerControlScript PS= col.GetComponent<PlayerControlScript>();
            if (ES != null)
            {
                ES.hit(boomdamge);
                Debug.Log("guaiwushoudao sahnghia ");
            }
            if(PS != null)
            {
                PS.Hit(boomdamge);
                Debug.Log("Player shoudao shanghai");
            }
        }

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, boomRadius);
    }
}
