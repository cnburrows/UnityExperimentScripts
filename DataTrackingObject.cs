using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTrackingObject : MonoBehaviour
{
    public string objectName;
    private Camera cam;
    private bool wasLookingAt = false;
    private bool lookingAtCenter = false;
    private bool lookingAtPeriphery = false;

    public float timeCenter = 0;
    public float timePeriphery = 0;
    public float sizeCenter = 0;
    public float sizePeriphery = 0;
    public int glances = 0;

    private float tempTimeCenter = 0;
    private float tempTimePeriphery = 0;
    private float tempSizeCenter = 0;
    private float tempSizePeriphery = 0;
    private DataController dataController;

    Renderer objRenderer;
    Collider objCollider;
    // Use this for initialization
    void Start() {
        if (objectName == "") {
            objectName = transform.name;
        }

        objRenderer = GetComponent<Renderer>();
        objCollider = GetComponent<Collider>();
        dataController = GameObject.FindObjectOfType<DataController>();
        cam = Camera.main;
    }
    // Update is called once per frame
    void Update() {
        if (lookingAtCenter) {
            timeCenter += Time.deltaTime;
            tempTimeCenter += Time.deltaTime;
            float size = (CamRectWithObject(gameObject) * Time.deltaTime);
            sizeCenter += size;
            tempSizeCenter += size;
            lookingAtCenter = false;
        }
        else if (lookingAtPeriphery) {
            timePeriphery += Time.deltaTime;
            tempTimePeriphery += Time.deltaTime;
            float size = (CamRectWithObject(gameObject) * Time.deltaTime);
            sizePeriphery += size;
            tempSizePeriphery += size;
            lookingAtPeriphery = false;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (GeometryUtility.TestPlanesAABB(planes, objCollider.bounds)) {
            // Then, see if there's anything in the way
            RaycastHit hit;
            Physics.Linecast(Camera.main.transform.position, objRenderer.bounds.center, out hit);
            if (hit.collider == null || hit.collider.gameObject == gameObject) {
                Vector3 posterCenter = cam.WorldToScreenPoint(objRenderer.bounds.center);
                if ((posterCenter.x > Screen.width / 4 && posterCenter.x < (Screen.width / 4) * 3)
                    && (posterCenter.y > Screen.height / 4 && posterCenter.y < (Screen.height / 4) * 3))
                {
                    lookingAtCenter = true;
                    lookingAtPeriphery = false;
                }
                else {
                    lookingAtCenter = false;
                    lookingAtPeriphery = true;
                }
                if (!wasLookingAt) {
                    glances++;
                    wasLookingAt = true;
                }
            }
            else
            {
                if (wasLookingAt)
                {
                    //data controller add line
                    dataController.AddLooksLine(objectName, tempTimePeriphery, tempTimeCenter, tempSizePeriphery, tempSizeCenter);
                    tempTimeCenter = 0;
                    tempTimePeriphery = 0;
                    tempSizeCenter = 0;
                    tempSizePeriphery = 0;
                    wasLookingAt = false;
                }
            }
        }
        else {
            if (wasLookingAt)
            {
                //data controller add line
                dataController.AddLooksLine(objectName, tempTimePeriphery, tempTimeCenter, tempSizePeriphery, tempSizeCenter);
                tempTimeCenter = 0;
                tempTimePeriphery = 0;
                tempSizeCenter = 0;
                tempSizePeriphery = 0;
                wasLookingAt = false;
            }
        }
    }
    public float CamRectWithObject(GameObject go) {
        Vector3 cen = go.GetComponent<Renderer>().bounds.center;
        Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[4]
        {
            cam.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z)),
            cam.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z)),
            cam.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z)),
            cam.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z))
        };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints) {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        Vector2 rec = new Rect(min.x, min.y, max.x - min.x, max.y - min.y).size;
        return rec.x * rec.y;
    }
}
