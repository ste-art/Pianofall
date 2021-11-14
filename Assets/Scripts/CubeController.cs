using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class CubeController : MonoBehaviour
{
    public GameObject NewObject;
    public float DefaultHeight = 7f;

    public float LeftPos = -6.5f;
    public float size = 0.1f;

    public int MaxObjects = 1500;
    public Text sliderValue;
    public GameObject[] Pool;
    public int Iterator;

    //public Vector3 Gravity = new Vector3(0, -9.8f, 0);

    public void SpawnNewCube(int data, float yOffset, Color color)
    {
        SpawnDefault(data, yOffset, color);
    }

    public void SpawnDefault(int data, float yOffset, Color color)
    {
        var pos = new Vector3(data * size + LeftPos, yOffset + DefaultHeight, 0f);
        var obj = Pool[Iterator];
        obj.SetActive(true);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.identity;
        var rigid = obj.GetComponent<Rigidbody>();
        if (rigid != null)
        {
            //rigid.velocity = Gravity/2f;
            rigid.velocity = new Vector3(0f, -5f, 0f);
            rigid.angularVelocity = new Vector3(0f, 0f, 0f);
        }

        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Iterator = (Iterator + 1) % MaxObjects;
    }

    void Start()
    {
        SetMaxNotes(MaxObjects);
    }

    public void SetMaxNotes(float value)
    {
        MaxObjects = (int)value;
        if(sliderValue!=null)
        {
            sliderValue.text = MaxObjects.ToString();
        }
        if (Pool == null)
        {
            Pool = new GameObject[MaxObjects];
            for (int i = 0; i < MaxObjects; i++)
            {
                Pool[i] = (GameObject)Instantiate(NewObject, new Vector3(), Quaternion.identity);
                Pool[i].SetActive(false);
            }
        }
        if (MaxObjects < Pool.Length)
        {
            var temp = new GameObject[MaxObjects];
            var starter = Iterator - MaxObjects + Pool.Length;
            for (int i = 0; i < Pool.Length; i++)
            {
                var index = (i + starter) % Pool.Length;
                if (i < MaxObjects)
                {
                    temp[i] = Pool[index];
                }
                else
                {
                    Destroy(Pool[index]);
                }
            }
            Pool = temp;
            Iterator = 0;
        }
        if (MaxObjects > Pool.Length)
        {
            var temp = new GameObject[MaxObjects];
            var j = 0;
            for (var i = 0; i < MaxObjects - Pool.Length; i++, j++)
            {
                temp[j] = (GameObject)Instantiate(NewObject, new Vector3(), Quaternion.identity);
                temp[j].SetActive(false);
            }
            for (var i = Iterator; i < Pool.Length; i++, j++)
            {
                temp[j] = Pool[i];
            }
            for (var i = 0; i < Iterator; i++, j++)
            {
                temp[j] = Pool[i];
            }
            Iterator = 0;
            Pool = temp;
        }
    }

    public void Clear()
    {
        foreach (var o in Pool)
        {
            o.SetActive(false);
        }
    }
}

public class CubeQueue
{
    private LinkedList<QueuedNote> _track = new LinkedList<QueuedNote>();
    public CubeController Controller;

    public CubeQueue(CubeController controller)
    {
        Controller = controller;
    }

    public void Update(long time, bool noDoubles = false/*, Vector3? gravity = null*/)
    {
        var played = new HashSet<int>();
        lock (_track)
        {
            //Controller.Gravity = gravity ?? new Vector3(0, -9.8f, 0);
            while (_track.First != null && _track.First.Value.Time <= time)
            {
                var note = _track.First.Value;
                _track.RemoveFirst();

                if (noDoubles && played.Contains(note.Data))
                {
                    continue;
                }
                played.Add(note.Data);

                Controller.SpawnNewCube(note.Data, -(time - note.Time) / 300f, note.Color);
            }
        }
    }

    public void InsertNote(int data, long time, Color color)
    {
        var note = new QueuedNote(data, time, color);
        lock (_track)
        {
            if (_track.Count == 0)
            {
                _track.AddLast(note);
                return;
            }
            var current = _track.Last;
            while (current != null && note.Time < current.Value.Time)
            {
                current = current.Previous;
            }
            if (current == null)
            {
                _track.AddFirst(note);
            }
            else
            {
                _track.AddAfter(current, note);
            }
        }
    }
}

internal class QueuedNote
{
    public int Data;
    public long Time;
    public Color Color;

    public QueuedNote(int data, long time, Color color)
    {
        Data = data;
        Time = time;
        Color = color;
    }
}
