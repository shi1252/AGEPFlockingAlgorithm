using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnitManager : MonoBehaviour
{
    static SelectableUnitManager instance;
    public static SelectableUnitManager Instance { get { return instance; } }

    public List<SelectableUnit> selectableObjsInScene;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            selectableObjsInScene = new List<SelectableUnit>();
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }
}
