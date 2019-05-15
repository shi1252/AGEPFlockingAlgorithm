using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour {

    public Transform leader;
    public float speed;
    public Vector3 direction;
    public float checkRadius = 15f;
    public float closeDistance = 3f;
    public float leaderDistance = 10f;
    public bool isSeperation = false;
    public List<RaycastHit> neighbors = new List<RaycastHit>();

	void Start ()
    {

    }
	
	void Update ()
    {
        Flocking();
        transform.position += (direction * speed * Time.deltaTime);
	}

    private void CheckNeighbor()
    {
        neighbors.Clear();

        // Set Neighbor
        int layerMask = (1 << LayerMask.NameToLayer("Boid"));
        RaycastHit[] allneighbors = Physics.SphereCastAll(
            transform.position, checkRadius, 
            transform.forward, 0f, layerMask);

        foreach (RaycastHit n in allneighbors)
        {
            if (n.transform != transform)
                neighbors.Add(n);
        }
    }

    private Vector3 Seperation()
    {
        Vector3 sepDirResult = Vector3.zero;
        foreach(RaycastHit n in neighbors)
        {
            if(Vector3.Distance(transform.position, n.transform.position) < closeDistance)
            {
                sepDirResult += (transform.position - n.transform.position);
                isSeperation = true;
            }
        }

        return sepDirResult.normalized;
    }

    private Vector3 Alignment()
    {
        Vector3 alignDirResult = Vector3.zero;
        foreach (RaycastHit n in neighbors)
        {
            alignDirResult += n.transform.forward;
        }

        Vector3 leaderDir = FollowLeader();
        alignDirResult += (5.0f * leaderDir.normalized); 
        return alignDirResult.normalized;
    }

    private Vector3 Cohesion()
    {
        Vector3 targetPos = Vector3.zero;
        foreach (RaycastHit n in neighbors)
        {
            targetPos += n.transform.position;
            targetPos /= neighbors.Count;
        }

        return (targetPos - transform.position).normalized;
    }

    private Vector3 FollowLeader()
    {
        return (leader.transform.position - transform.position).normalized;
    }

    private void Flocking()
    {
        speed = 5f;
        CheckNeighbor();
        if(neighbors.Count == 0)
        {
            direction = FollowLeader().normalized;
        }
        else
        {
            isSeperation = false;
            Vector3 sepDir = Seperation();
            if(isSeperation)
            {
                direction = sepDir;
                speed = 10f;
            }
            else
            {
                if (Vector3.Distance(leader.position, transform.position) > leaderDistance)
                {
                    direction = FollowLeader().normalized;
                    speed = 10f;
                }
                else
                {
                    Vector3 cohDir = Cohesion();
                    Vector3 alignDir = Alignment();
                    direction = (1.0f * cohDir + 3.0f * alignDir).normalized;
                }
            }

        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + (direction * 1f));
    }
}
