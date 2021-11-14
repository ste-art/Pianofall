using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{
    private Vector3 _screenPoint;
    private Vector3 _offset;

    private bool? _rigidBodyExist = null;
    private Rigidbody _rigidbody;
    private Rigidbody Rigidbody
    {
        get
        {
            if (_rigidBodyExist.HasValue && !_rigidBodyExist.Value)
            {
                return null;
            }
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            _rigidBodyExist = _rigidbody != null;

            return _rigidbody;
        }
    }

    void OnMouseDown()
    {
        _screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        _offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z));
        if(Rigidbody != null)
        {
            Rigidbody.isKinematic = true;
        }
    }

    void OnMouseUp()
    {
        if (Rigidbody != null)
        {
            Rigidbody.isKinematic = false;
        }
    }

    void OnMouseDrag()
    {
        _screenPoint.z += Input.GetAxis("Mouse ScrollWheel") * 5;
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + _offset;
        transform.position = curPosition;
        foreach (Rigidbody obj in FindObjectsOfType<Rigidbody>())
        {
            obj.WakeUp();
        }
    }
}
