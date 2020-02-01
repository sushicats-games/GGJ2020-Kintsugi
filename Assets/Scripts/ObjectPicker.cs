using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPicker : MonoBehaviour
{
    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(transform.position, hit.point, Color.yellow);
            Debug.Log("Did Hit");
        }
    }
}
