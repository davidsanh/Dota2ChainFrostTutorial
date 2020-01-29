using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainFrostScript : MonoBehaviour
{
    //Visualization
    public bool Debug;

    //Amount of times the chain frost will jump between targets
    public int bounces;
    //Variables to change in inspector
    public float movSpeed, rotSpeed, distance, radius;
    //The transform our chain frost will chase
    public Transform target;
    //The tag to check so you can differentiate between gameobjects in your scene to get your enemy
    public string tagToCheck;

    //Particle effects to show location of chain frost
    private ParticleSystem impact, loop;
    //This boolean breaks the update loop and trigger functions
    private bool isDead;
    //This list consists of all the posible targets within a radius set in the engine
    private List<Transform> possibleTargets = new List<Transform>();

    private void Start()
    {
        //Reference our particle systems
        impact = transform.Find("Impact").GetComponent<ParticleSystem>();
        loop = transform.Find("Loop").GetComponent<ParticleSystem>();

        //If we dont have a target then force to find one within radius
        if (!target && possibleTargets.Count > 0)
        {
            //Clean our list before assigning the target
            possibleTargets.Remove(target);
            //Randomization using the amount of possble targets
            var random = Random.Range(0, possibleTargets.Count);
            //Assign our target based on the randomization
            target = possibleTargets[random];
        }
    }

    void Update()
    {
        //This loops forever until it dies
        if (!isDead)
        {
            //If we have assign a target
            if (target)
            {
                //Remove the target from our possbiel targets list
                if (possibleTargets.Contains(target)) { possibleTargets.Remove(target); }

                //Store the distance between target and chain frost in a temporary variable
                var dist = Vector3.Distance(transform.position, target.position);

                //Rotation of chain frost
                var lookPos = target.position - transform.position;
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotSpeed * Time.deltaTime);

                //Movement of chain frost
                transform.Translate(Vector3.forward * movSpeed * Time.deltaTime);

                if (dist < distance)
                {
                    //If the distance variable we stored is smaller than distance(float in inspector) we do our chain
                    ChainFrost();
                }
            }
            else if (possibleTargets.Count > 0)
            {
                //Clean our list before assigning the target
                possibleTargets.Remove(target);
                //Randomization using the amount of possble targets
                var random = Random.Range(0, possibleTargets.Count);
                //Assign our target based on the randomization
                target = possibleTargets[random];
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Do our checks before adding it to our possible targets list
        if (other.tag == tagToCheck && !possibleTargets.Contains(other.transform) && target != other.transform && !isDead)
        {
            possibleTargets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Check posibble target before removing it from list
        if (other.tag == tagToCheck && !isDead)
        {
            possibleTargets.Remove(other.transform);
        }
    }

    void ChainFrost()
    {
        //Check if list is bigger than zero
        if (possibleTargets.Count > 0)
        {
            //Clean our list before assigning the target
            possibleTargets.Remove(target);
            //Play the impact particle effect
            impact.Play();
            //Lower our bounces by one
            bounces -= 1;
            //Randomization using the amount of possble targets
            var random = Random.Range(0, possibleTargets.Count);
            //Assign our target based on the randomization
            target = possibleTargets[random];

            //Finish chainfrost if our bounces are smaller than 0
            if (bounces <= 0) { FinishChainFrost(); }
        }

        //Create a sphere around location using our radius
        Collider[] objectsInRange = Physics.OverlapSphere(target.transform.position, radius);

        //We get all colliders that overlap our sphere cast
        foreach (Collider col in objectsInRange)
        {
            //We get the enemies within range that contain a enemy script
            //EnemyScript enemy = col.GetComponent<EnemyScript>();

            //We check if enemy has been found
            //if (enemy != null)
            //{
            //You can also call your damaging script here
            //enemy.health -= damage;
            // }
        }
    }

    void FinishChainFrost()
    {
        //Stop update loop 
        isDead = true;
        //Store looping particle emission 
        var loopEmi = loop.emission;
        //Set our rate overtime to nothing so it stops
        loopEmi.rateOverTime = 0;
        //Destroy this chain frost with a delay of 2 seconds
        Destroy(gameObject, 2);
    }

    //Draw visualization of our damaging area
    void OnDrawGizmos()
    {
        if (Debug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
