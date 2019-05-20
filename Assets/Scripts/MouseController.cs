using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FormationState
{
    None,
    Line,
    Square,
    Length
}

public class MouseController : MonoBehaviour
{
    static MouseController instance;
    public static MouseController Instance { get { return instance; } }

    public float mouseScreenScrollRadius = 10f;
    [Range(0f, 100f)]
    public float mouseScreenScrollSpeed = 5f;
    public FormationState formation = FormationState.Line;

    List<SelectableUnit> selectedObjs;

    Vector3 mouseDownPos = Vector3.zero;
    Vector3 curMousePos = Vector3.zero;
    Vector3 startPos = Vector3.zero;
    Vector3 endPos = Vector3.zero;

    bool isDragging = false;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
            selectedObjs = new List<SelectableUnit>();
        }
        else
            Destroy(this);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            mouseDownPos = Input.mousePosition;
            mouseDownPos.z = Camera.main.transform.position.y;
            startPos = Camera.main.ScreenToWorldPoint(mouseDownPos);
        }

        curMousePos = Input.mousePosition;
        curMousePos.z = Camera.main.transform.position.y;
        if (isDragging)
        {
            endPos = Camera.main.ScreenToWorldPoint(curMousePos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            GetSelectedObjs();
        }

        if (Input.GetMouseButtonDown(1))
        {
            mouseDownPos = Input.mousePosition;
            mouseDownPos.z = Camera.main.transform.position.y;
            MoveSelectedObjs();
        }
    }

    void GetSelectedObjs()
    {
        foreach (SelectableUnit t in selectedObjs)
            t.SetSelected(false);
        selectedObjs.Clear();

        float leftX = Mathf.Min(startPos.x, endPos.x);
        float rightX = Mathf.Max(startPos.x, endPos.x);
        float botZ = Mathf.Min(startPos.z, endPos.z);
        float topZ = Mathf.Max(startPos.z, endPos.z);

        foreach (SelectableUnit t in SelectableUnitManager.Instance.selectableObjsInScene)
        {
            if (t.transform.position.x >= leftX && t.transform.position.x <= rightX && t.transform.position.z >= botZ && t.transform.position.z <= topZ)
            {
                selectedObjs.Add(t);
                t.SetSelected(true);
            }
        }

        for (int i = 1; i < selectedObjs.Count; i++)
        {
            selectedObjs[i].leader = selectedObjs[0];
        }

        SetFormation();
    }

    void MoveSelectedObjs()
    {
        if (selectedObjs.Count <= 0) return;
        Vector3 dest = Camera.main.ScreenToWorldPoint(mouseDownPos);
        dest.y = 0;

        selectedObjs[0].SetDest(dest);
    }

    void SetFormation()
    {
        int rt = (int)Mathf.Clamp(Mathf.Sqrt(selectedObjs.Count), 2f, float.MaxValue);
        switch (formation)
        {
            case FormationState.Line:
                for (int i = 1; i < selectedObjs.Count; i++)
                    selectedObjs[i].SetIndex(i);
                return;
            case FormationState.Square:
                for (int i = 1; i < selectedObjs.Count; i++)
                    selectedObjs[i].SetIndex(i / rt, i % rt);//SetDest(selectedObjs[0].dest + -selectedObjs[0].transform.forward * (i / rt) * selectedObjs[i].sepRadius + selectedObjs[0].transform.right.normalized * (i % rt) * selectedObjs[i].sepRadius);
                return;
        }
    }

    private void LateUpdate()
    {
        if (curMousePos.x - mouseScreenScrollRadius < 0)
            Camera.main.transform.position -= new Vector3(mouseScreenScrollSpeed, 0f) * Time.deltaTime;
        else if (curMousePos.x + mouseScreenScrollRadius > Screen.width)
            Camera.main.transform.position += new Vector3(mouseScreenScrollSpeed, 0f) * Time.deltaTime;

        if (curMousePos.y - mouseScreenScrollRadius < 0)
            Camera.main.transform.position -= new Vector3(0f, 0f, mouseScreenScrollSpeed) * Time.deltaTime;
        else if (curMousePos.y + mouseScreenScrollRadius > Screen.height)
            Camera.main.transform.position += new Vector3(0f, 0f, mouseScreenScrollSpeed) * Time.deltaTime;
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            GUI.Box(new Rect(mouseDownPos.x, Screen.height - mouseDownPos.y, curMousePos.x - mouseDownPos.x, mouseDownPos.y - curMousePos.y), "");
        }
    }

    private void OnValidate()
    {
        SetFormation();
    }
}