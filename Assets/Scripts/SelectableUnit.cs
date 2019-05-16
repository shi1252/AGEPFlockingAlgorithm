using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnit : MonoBehaviour
{
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
    Vector3 moveDir = Vector3.forward;

    List<SelectableUnit> neighbors;

    public Vector3 dest;


    private void Awake()
    {
        UnSelected();
        SelectableUnitManager.Instance.selectableObjsInScene.Add(this);
        neighbors = new List<SelectableUnit>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, dest) > 0.1f)
        {
            transform.position += (dest - transform.position).normalized * moveSpeed * Time.deltaTime;
            transform.forward = (dest - transform.position).normalized;
        }
    }

    public void SetDest(Vector3 d)
    {
        dest = d;
        transform.forward = (dest - transform.position).normalized;
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
}
