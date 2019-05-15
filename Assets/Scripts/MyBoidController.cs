using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBoidController : MonoBehaviour
{
    public float width = 25f;
    public float height = 25f;
    public Transform leader;
    [Range(0f, 1f)]
    public float sWeight = 0.33f;
    public float sepRadius = 2f;
    [Range(0f, 1f)]
    public float aWeight = 0.33f;
    [Range(0f, 1f)]
    public float cWeight = 0.34f;
    public float recogRadius = 10f;
    public float speed = 5f;
    Vector3 moveDir = Vector3.forward;

    List<MyBoidController> neighbors;

    Vector3 movePos = Vector3.zero;

    private void Awake()
    {
        neighbors = new List<MyBoidController>();
    }

    private void Update()
    {
        Flocking();
        transform.position += (moveDir * speed * Time.deltaTime);
    }

    void Flocking()
    {
        GetNeighbors();
        if (neighbors.Count <= 0)
        {
            BeLeader();
        }
        else
        {
            if (!leader )
            {
                ChooseLeader();
            }
            else if (Vector3.Distance(transform.position, leader.position) > recogRadius)
            {
                ChooseLeader();
            }

            if (!leader)
            {
                BeLeader();
                AvoidObstacle(ref moveDir);
                return;
            }

            Vector3 sepDir = Seperation();
            Vector3 aliDir = Alignment();
            Vector3 cohDir = Cohesion();

            moveDir = sepDir.normalized * sWeight + aliDir.normalized * aWeight + cohDir.normalized * cWeight;
            if (leader)
                moveDir += leader.GetComponent<MyBoidController>().moveDir;
            moveDir = moveDir.normalized;

            AvoidObstacle(ref moveDir);
        }
    }

    void GetNeighbors()
    {
        neighbors.Clear();

        Collider[] colls = Physics.OverlapSphere(transform.position, recogRadius, LayerMask.GetMask("Boid"));
        foreach (Collider c in colls)
        {
            if (c.transform != transform)
                neighbors.Add(c.GetComponent<MyBoidController>());
        }
    }

    void BeLeader()
    {
        leader = null;

        if (Vector3.Distance(transform.position, movePos) <= 0.1f)
        {
            ChangeMovePos();
        }

        moveDir = movePos - transform.position;
        moveDir = moveDir.normalized;
    }

    void ChooseLeader()
    {
        leader = null;
        foreach (MyBoidController t in neighbors)
        {
            if (!t.leader)
            {
                leader = t.transform;
                return;
            }
        }
    }

    bool AvoidObstacle(ref Vector3 dir)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, moveDir, out hit, sepRadius, LayerMask.GetMask("Obstacle")))
        {
            if (leader)
                dir = transform.position - hit.point;
            else
                ChangeMovePos();
            return true;
        }
        return false;
    }

    void ChangeMovePos()
    {
        movePos = new Vector3(Random.Range(-width, width), 0f, Random.Range(-height, height));
    }

    Vector3 Seperation()
    {
        Vector3 dir = Vector3.zero;

        Collider[] colls = Physics.OverlapSphere(transform.position, sepRadius, LayerMask.GetMask("Boid"));
        foreach     (Collider c in colls)
        {
            dir += transform.position - c.transform.position;
        }

        return dir;
    }

    Vector3 Alignment()
    {
        Vector3 dir = Vector3.zero;

        foreach (MyBoidController t in neighbors)
        {
            dir += t.moveDir;
        }

        return dir;
    }

    Vector3 Cohesion()
    {
        Vector3 pos = Vector3.zero;

        foreach (MyBoidController t in neighbors)
        {
            pos += t.transform.position;
        }

        if (neighbors.Count > 0)
            pos /= neighbors.Count;

        return (pos - transform.position);
    }
}