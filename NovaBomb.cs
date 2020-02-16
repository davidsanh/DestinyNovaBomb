using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaBomb : MonoBehaviour
{
    //Stops movement of our nova
    public bool isDead;
    //Shattered nova prefab
    public GameObject shatteredNovaOBJ;
    //The lifetime of our nova bomb before it explodes
    public float lifetime;
    //The spherical aoe radius of the vertex
    public float damageRadius;
    //Force applied to nova bomb upon spawn
    public float spawnForce;
    //The delay before destroying this gameobject
    public float destroyDelay;
    //Gravity down applied to nova bomb overtime
    public float gravityForce;
    //The amount of prefabs that will respawn
    public float shatterAmount;
    //Y offset upon shattering nova bomb for prefabs
    public float shatterOffset;
    //The force applied to the nova bomb when it has a target
    public float seekForce;
    //How often will units inside its radius be damaged
    public float damageInterval;
    //The duration of the vortex
    public float vortexDuration;

    //Private variables
    [HideInInspector]
    public Rigidbody rb;
    private ParticleSystem loop, impact, vortex;
    [HideInInspector]
    public Transform target;
    private GameObject targetAcquisition;
    private float damageIntervalTimer;

    void Start()
    {
        //Reference our rigidbody
        rb = GetComponent<Rigidbody>();
        //Add a force forward upon spawning
        rb.AddForce(transform.forward * spawnForce, ForceMode.VelocityChange);
        //Get our particle systems
        loop = transform.Find("Loop").GetComponent<ParticleSystem>();
        impact = transform.Find("Impact").GetComponent<ParticleSystem>();
        //Reference our target acquisition
        targetAcquisition = transform.Find("TargetAcquisition").gameObject;

        //If we have enabled vortex then we find the vortex particle system
        if (vortexDuration > 0) vortex = transform.Find("Vortex").GetComponent<ParticleSystem>();
        //If we have enabled seeking then we unparent targetAcquisition to avoid colliding with it when first spawned
        if (seekForce > 0) { targetAcquisition.transform.SetParent(null); } else { targetAcquisition.SetActive(false); }
    }

    private void Update()
    {
        //Set the target acquisition to be the position of our nova bomb
        if (targetAcquisition) targetAcquisition.transform.position = transform.position;

        //Check if we have impacted
        if (isDead)
        {
            //If vortex still going
            if (vortexDuration > 0)
            {
                //Lower our vortex duration
                vortexDuration -= Time.deltaTime;
                //Lower our damage interval
                damageIntervalTimer -= Time.deltaTime;

                //If our damage interval is smaller than 0 then we damage
                if (damageIntervalTimer < 0)
                {
                    AOEDamage();
                }
            }
            else
            {
                //If vortex has ended
                if (vortex)
                {
                    //Unparent vortex before we delete it to allow particles to fade
                    vortex.transform.SetParent(null);
                    vortex.Stop();
                    Destroy(vortex.gameObject, destroyDelay);
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            //Deal with our lifetime timer
            if (lifetime > 0)
            {
                //Lower our lifetime by delta time
                lifetime -= Time.deltaTime;
            }
            else
            {
                //We ran out of lifetime so explode ourselves
                Impact();
                AOEDamage();
            }
        }
    }

    void FixedUpdate()
    {
        //Fixed update is used in coordination with the physics engine
        //Check if we have impacted
        if (!isDead)
        {
            //If we have found a target
            if (target)
            {
                //Add a force towards the target using seekForce
                Vector3 dir = target.position - transform.position;
                rb.AddForce(dir.normalized * seekForce, ForceMode.VelocityChange);
            }
            else if(gravityForce > 0)
            {
                //Add gravity force to nova bomb if enabled
                rb.AddForce(Vector3.down * gravityForce, ForceMode.VelocityChange);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Here add as many checks as you want for your nova bomb's collision
        if (!isDead && other.tag != "Player" && other.tag != "NovaBomb")
        {
            Impact();
            AOEDamage();
        }
    }

    void Impact()
    {
        //Check if we have enabled shattering
        if (shatterAmount > 0 && shatteredNovaOBJ)
        {
            for (int i = 0; i < shatterAmount; i++)
            {
                //We randomize the rotation upwards
                Quaternion randomRot = Quaternion.Euler(Random.Range(-20,-90), Random.Range(0, 360), 0);
                //For each shatterAmount we create a new prefab gameobject
                Instantiate(shatteredNovaOBJ, transform.position + Vector3.up * shatterOffset, randomRot);
            }
        }

        //Check if the vertex duration is bigger than 0 and if it is, we enable our vortex, if not then we destroy this
        if (vortexDuration > 0) { vortex.gameObject.SetActive(true); }
        else { Destroy(gameObject, destroyDelay); }

        //Play our impact particle
        impact.Play();
        //Stop playing our loop
        loop.Stop();
        //Stop movement
        isDead = true;
        //Stop our rigidbody velocity
        rb.velocity = Vector3.zero;
        //Destroy our target acquisition gameobject since we dont need it no longer
        Destroy(targetAcquisition);
    }

    void AOEDamage()
    {
        //Set the damage interval timer
        damageIntervalTimer = damageInterval;

        //Create a sphere around location using our radius
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, damageRadius);

        //We get all colliders that overlap our sphere cast
        foreach (Collider col in objectsInRange)
        {
            //We get the enemies within range that contain a enemy script
            //EnemyScript enemy = col.GetComponent<EnemyScript>();

            //We check if enemy has been found
            //if (enemy != null)
            //{
            //You can call your damaging script here
            //enemy.health -= damage;
            //}
        }
    }
}
