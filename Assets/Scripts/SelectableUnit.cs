using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnit : MonoBehaviour
{
    public float width = 25f;
    public float height = 25f;
    bool isSelected;
    public GameObject SelectedTexture;

    public SelectableUnit leader;

    public float moveSpeed = 10f;
    [Range(0f, 1f)]
    public float sWeight = 0.33f;
    public float sepRadius = 2f;
    [Range(0f, 1f)]
    public float aWeight = 0.33f;
    [Range(0f, 1f)]
    public float cWeight = 0.34f;
    public float recogRadius = 10f;
    Vector3 moveDir = Vector3.zero;

    List<SelectableUnit> neighbors;
    HashSet<SelectableUnit> followers;

    public Vector3 dest;
    public int indexI;
    public int indexJ;

    private void Awake()
    {
        UnSelected();
        SelectableUnitManager.Instance.selectableObjsInScene.Add(this);
        neighbors = new List<SelectableUnit>();
        followers = new HashSet<SelectableUnit>();
        ChangeDest();
    }

    private void Update()
    {
        Flocking();
        transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
        transform.forward = moveDir.normalized;
    }

    void Flocking()
    {
        GetNeighbors();
        if (neighbors.Count > 0)
        {
            if (!leader) ChooseLeader();
            else if (Vector3.Distance(transform.position, leader.transform.position) > recogRadius) ChooseLeader();

            if (leader && Vector3.Distance(transform.position, leader.transform.position) > recogRadius) BeLeader();
            if (!leader) SetIndex();
            if (!leader && Vector3.Distance(transform.position, dest) < 0.1f)
                ChangeDest();
            SetDirection();
            Vector3 s = Seperation();
            Vector3 a = Vector3.zero;
            Vector3 c = Vector3.zero;
            if (MouseController.Instance.formation == FormationState.None)
            {
                a = Alignment();
                c = Cohesion();
                if (leader)
                    moveDir += leader.moveDir.normalized;
            }
            moveDir += s.normalized * sWeight + a.normalized * aWeight + c.normalized * cWeight;
        }
        else
        {
            BeLeader();
        }
        AvoidObstacle();
    }

    void ChangeDest()
    {
        dest = new Vector3(Random.Range(-width, width), 0f, Random.Range(-height, height));
    }

    void SetFormation()
    {
        if (leader)
        {
            float sr = sepRadius + 1f;
            switch (MouseController.Instance.formation)
            {
                case FormationState.Line:
                    dest = leader.transform.position + /*leader.transform.right.normalized*/ Vector3.right * indexI * sr;
                    return;
                case FormationState.Square:
                    dest = leader.transform.position + /*leader.transform.right.normalized*/ Vector3.right * indexJ * sr + -/*leader.transform.forward.normalized*/Vector3.forward * indexI * sr;
                    return;
            }
        }
    }

    void BeLeader()
    {
        leader = null;
        followers.Clear();

        if (Vector3.Distance(transform.position, dest) < 0.1f)
            ChangeDest();

                    SetDirection();
    }

    void ChooseLeader()
    {
        SelectableUnit l = neighbors[0];
        if (neighbors[0].leader) l = neighbors[0].leader;
        if (l == this) return;

        leader = l;
        l.AddFollower(this);
        foreach(SelectableUnit t in followers)
        {
            if (t == l) continue;
            t.leader = l;
            l.AddFollower(t);
        }
        l.SetIndex();
    }

    void SetDirection()
    {
        if (leader)
        {
            if (MouseController.Instance.formation != FormationState.None)
            {
                SetFormation();
            }
            else
            {
                moveDir = (leader.dest - transform.position).normalized;
                return;
            }
        }

        moveDir = (dest - transform.position).normalized;
    }

    void GetNeighbors()
    {
        neighbors.Clear();

        Collider[] colls = Physics.OverlapSphere(transform.position, recogRadius, LayerMask.GetMask("Boid"));
        foreach (Collider c in colls)
        {
            if (c.transform != transform)
                neighbors.Add(c.GetComponent<SelectableUnit>());
        }
    }

    Vector3 Seperation()
    {
        Vector3 dir = Vector3.zero;

        Collider[] colls = Physics.OverlapSphere(transform.position, sepRadius, LayerMask.GetMask("Boid"));
        foreach (Collider c in colls)
        {
            dir += transform.position - c.transform.position;
        }

        return dir;
    }

    Vector3 Alignment()
    {
        Vector3 dir = Vector3.zero;

        foreach (SelectableUnit t in neighbors)
        {
            dir += t.moveDir;
        }

        return dir;
    }

    Vector3 Cohesion()
    {
        Vector3 pos = Vector3.zero;

        foreach (SelectableUnit t in neighbors)
        {
            pos += t.transform.position;
        }

        if (neighbors.Count > 0)
            pos /= neighbors.Count;

        return (pos - transform.position);
    }

    void AvoidObstacle()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, moveDir, out hit, sepRadius, LayerMask.GetMask("Obstacle")))
        {
            moveDir = (transform.position - hit.point).normalized;
            if (!leader)
            {
                ChangeDest();
            }
            return;
            if (Physics.Raycast(transform.position, new Vector3(moveDir.normalized.x, 0, 0), out hit, moveSpeed * Time.deltaTime, LayerMask.GetMask("Obstacle")))
            {
                moveDir = new Vector3(0, 0, moveDir.z).normalized;
            }
            if (Physics.Raycast(transform.position, new Vector3(0, 0, moveDir.normalized.z), out hit, moveSpeed * Time.deltaTime, LayerMask.GetMask("Obstacle")))
            {
                moveDir = new Vector3(moveDir.x, 0, 0).normalized;
            }

            if (moveDir == Vector3.zero)
                ChangeDest();
        }
    }

    public void AddFollower(SelectableUnit t)
    {
        followers.Add(t);
    }

    public void SetDest(Vector3 d)
    {
        dest = d;
    }

    public void SetSelected(bool b)
    {
        isSelected = b;
        if (b) Selected();
        else UnSelected();
    }

    void Selected()
    {
        SelectedTexture.SetActive(true);
    }

    void UnSelected()
    {
        SelectedTexture.SetActive(false);
        leader = null;
        isSelected = false;
        dest = transform.position;
    }

    public void SetIndex(int i)
    {
        indexI = i;
    }

    public void SetIndex(int i, int j)
    {
        indexI = i;
        indexJ = j;
    }

    public void SetIndex()
    {
        if (leader) return;
        SelectableUnit[] objs = new SelectableUnit[followers.Count];
        followers.CopyTo(objs);
        int rt = (int)Mathf.Clamp(Mathf.Sqrt(followers.Count), 2f, float.MaxValue);
        switch (MouseController.Instance.formation)
        {
            case FormationState.Line:
                for (int i = 0; i < followers.Count; i++)
                    objs[i].SetIndex(i+1);
                return;
            case FormationState.Square:
                for (int i = 0; i < followers.Count; i++)
                    objs[i].SetIndex((i+1) / rt, (i+1) % rt);//SetDest(selectedObjs[0].dest + -selectedObjs[0].transform.forward * (i / rt) * selectedObjs[i].sepRadius + selectedObjs[0].transform.right.normalized * (i % rt) * selectedObjs[i].sepRadius);
                return;
        }
    }
}
