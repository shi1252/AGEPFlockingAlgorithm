using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnit : MonoBehaviour
{
    protected bool isSelected;
    SpriteRenderer SelectedTexture;

    public SelectableUnit leader;

    public float moveSpeed = 10f;
    [Range(0f, 1f)]
    public float sWeight = 0.33f;
    public float sepRadius = 2f;
    [HideInInspector]
    public Vector3 moveDir = Vector3.zero;

    public HashSet<SelectableUnit> followers;

    public Vector3 dest;
    public int indexI;
    public int indexJ;

    Rigidbody rb;
    bool seperation = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SelectedTexture = GetComponentInChildren<SpriteRenderer>();
        UnSelected();
        SelectableUnitManager.Instance.selectableObjsInScene.Add(this);
        followers = new HashSet<SelectableUnit>();
        SetDest(transform.position);
    }

    void FixedUpdate()
    {
        if (isSelected)
        {
            float dis = Vector3.Distance(transform.position, dest);
            Flocking();
            if (dis < 0.1f && !seperation)
                moveDir = Vector3.zero;
            if (moveDir != Vector3.zero)
            {
                rb.velocity = moveDir.normalized * moveSpeed * (seperation ? 1f : Mathf.Clamp(dis / 0.3f, 0.8f, 1f));
                transform.forward = moveDir.normalized;
            }
        }
    }

    protected virtual void Flocking()
    {
        SetDirection();
        Vector3 s = Seperation();
        moveDir += s.normalized * sWeight;
        if (leader)
        {
            if (Vector3.Distance(leader.transform.position, leader.dest) > 0.1f)
                moveDir += leader.moveDir.normalized;
        }
        AvoidObstacle();
    }

    public void BeLeader()
    {
        if (leader)
        {
            leader.DeleteFollower(this);
            leader = null;
        }
        try
        {
            followers.Clear();
        }
        catch {}

        SetDirection();
    }

    public void DeleteFollower(SelectableUnit f)
    {
        followers.Remove(f);
        SetIndex();
    }

    public void ChooseLeader(SelectableUnit l)
    {
        if (l == this) return;

        leader = l;
        l.AddFollower(this);
        foreach (SelectableUnit t in followers)
        {
            if (t == l) continue;
            t.leader = l;
            l.AddFollower(t);
        }
        l.SetIndex();
    }

    protected void SetDirection()
    {
        if (leader)
        {
            SetFormation();
        }

        moveDir = (dest - transform.position).normalized;
    }

    protected void SetFormation()
    {
        if (leader)
        {
            float sr = sepRadius + 1f;
            switch (MouseController.Instance.formation)
            {
                case FormationState.Line:
                    dest = leader.dest + /*leader.transform.position*/  /*leader.transform.right.normalized*/ Vector3.right * indexI * sr;
                    return;
                case FormationState.Square:
                    dest = leader.dest + /*leader.transform.position*/  /*leader.transform.right.normalized*/ Vector3.right * indexJ * sr + -/*leader.transform.forward.normalized*/Vector3.forward * indexI * sr;
                    return;
                case FormationState.Circle:
                    Vector4 v = Matrix4x4.Rotate(Quaternion.Euler(0, indexI * indexJ, 0)) * new Vector4(0, 0, Mathf.Clamp(((sepRadius + 1) * 360f) / (2 * Mathf.PI * indexJ), sepRadius + 1, float.MaxValue), 0);
                    dest = leader.dest + /*leader.transform.position*/  new Vector3(v.x, 0, v.z);
                    return;
            }
        }
    }

    protected Vector3 Seperation()
    {
        Vector3 dir = Vector3.zero;

        Collider[] colls = Physics.OverlapSphere(transform.position, sepRadius, LayerMask.GetMask("Boid"));
        foreach (Collider c in colls)
        {
            dir += (transform.position - c.transform.position);
        }
        if (dir != Vector3.zero) seperation = true;
        else seperation = false;
        return dir;
    }

    protected void AvoidObstacle()
    {
        RaycastHit hit;
        Vector3 newPos = transform.position + new Vector3(0.5f, 0, 0.5f);
        if (Physics.Raycast(newPos, moveDir, out hit, 1f, LayerMask.GetMask("Obstacle")))
        {
            Debug.DrawRay(transform.position, hit.point - transform.position, Color.blue);
            //moveDir = (transform.position - hit.point).normalized;
            //return;
            if (Physics.Raycast(newPos, new Vector3(moveDir.normalized.x, 0, 0), out hit, moveSpeed * Time.deltaTime, LayerMask.GetMask("Obstacle")))
            {
                moveDir = new Vector3(0, 0, moveDir.z).normalized;
            }
            else if (Physics.Raycast(newPos, new Vector3(0, 0, moveDir.normalized.z), out hit, moveSpeed * Time.deltaTime, LayerMask.GetMask("Obstacle")))
            {
                moveDir = new Vector3(moveDir.x, 0, 0).normalized;
            }
        }
        else
        {
            newPos = transform.position + new Vector3(-0.5f, 0, 0.5f);
            if (Physics.Raycast(newPos, moveDir, out hit, 1f, LayerMask.GetMask("Obstacle")))
            {
                Debug.DrawRay(transform.position, hit.point - transform.position, Color.red);
                if (Physics.Raycast(newPos, new Vector3(moveDir.normalized.x, 0, 0), out hit, moveSpeed * Time.deltaTime, LayerMask.GetMask("Obstacle")))
                {
                    moveDir = new Vector3(0, 0, moveDir.z).normalized;
                }
                else if (Physics.Raycast(newPos, new Vector3(0, 0, moveDir.normalized.z), out hit, moveSpeed * Time.deltaTime, LayerMask.GetMask("Obstacle")))
                {
                    moveDir = new Vector3(moveDir.x, 0, 0).normalized;
                }
            }
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
        SelectedTexture.enabled = true;
    }

    void UnSelected()
    {
        SelectedTexture.enabled = false;
        BeLeader();
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
        float root = Mathf.Sqrt(followers.Count + 1);
        int rt = (int)Mathf.Clamp(root + (root % (int)root > Mathf.Epsilon ? 1 : 0), 1f, float.MaxValue);
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
            case FormationState.Circle:
                for (int i = 0; i < followers.Count; i++)
                    objs[i].SetIndex(i, (int)360f / followers.Count);
                return;
        }
    }
}
