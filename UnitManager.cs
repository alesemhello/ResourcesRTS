using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;
    protected BoxCollider _collider;
    public virtual Unit Unit { get; set; }


    private void OnMouseDown()
    {
        if (IsActive())
            Select(
                true,
                Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift)
            );
    }

    private Transform _canvas;
    private GameObject _healthbar;

    private void Awake()
    {
        _canvas = GameObject.Find("Canvas").transform;
    }

    protected virtual bool IsActive()
    {
        return true;
    }

    public void Select() { Select(false, false); }

    public void Select(bool singleClick, bool holdingShift)
    {
        // basic case: using the selection box
        if (!singleClick)
        {
            /*_SelectUtil();
            return;*/
            if (!Globals.SELECTED_UNITS.Contains(this))
            {
                Debug.Log("Not Contained!");
                _SelectUtil();
            }
            return;
        }

        // single click: check for shift key
        if (!holdingShift)
        {
            Debug.Log("holdingShift");
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (UnitManager um in selectedUnits)
                um.Deselect();
            _SelectUtil();
        }
        else
        {
            if (!Globals.SELECTED_UNITS.Contains(this))
            {
                Debug.Log("Contained!!");
                _SelectUtil();
            }
            else
            {
                Deselect();
                Debug.Log("Deselecting");
            }
        }
    }

    private void _SelectUtil()
    {
        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);
        if (_healthbar == null)
        {
            _healthbar = Instantiate(Resources.Load("Prefabs/UI/Healthbar")) as GameObject;
            _healthbar.transform.SetParent(_canvas);
            Healthbar h = _healthbar.GetComponent<Healthbar>();
            Rect boundingBox = Utils.GetBoundingBoxOnScreen(
                transform.Find("Mesh").GetComponent<Renderer>().bounds,
                Camera.main
            );
            h.Initialize(transform, boundingBox.height);
            h.SetPosition();
        }
        EventManager.TriggerEvent("SelectUnit", Unit);
    }

    public void Deselect()
    {
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);
        Destroy(_healthbar);
        _healthbar = null;
        EventManager.TriggerEvent("DeselectUnit", Unit);
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }
}