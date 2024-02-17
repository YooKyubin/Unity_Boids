using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidsManager : MonoBehaviour
{
    [Range(0f, 2f)]
    public float separationWeight;
    [Range(0f, 2f)]
    public float cohesionWeight;
    [Range(0f, 2f)]
    public float alignmentWeight;

    public float speed;
    public float turnSpeed;
    public float neighborRange;
    public float FOV;

    public GameObject prefab;



    // Start is called before the first frame update
    void Awake()
    {
        separationWeight = 1.0f;
        cohesionWeight = 1.0f;
        alignmentWeight = 1.0f;

        speed = 7.0f;
        turnSpeed = 8.0f;
        neighborRange = 5.0f;
        FOV = 120.0f;

        for (int i=0; i<500; ++i)
        {
            GameObject temp = MakeBoid();
            temp.transform.SetPositionAndRotation(Random.insideUnitSphere * 14f, Random.rotationUniform);
            temp.transform.position += new Vector3(0, 14f, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    GameObject MakeBoid()
    {
        GameObject boid = new GameObject("boid");
        boid.AddComponent<BoidUnit>();
        boid.AddComponent<CapsuleCollider>();
        CapsuleCollider collider = boid.GetComponent<CapsuleCollider>();
        {
            collider.direction = 2;
            collider.radius = 0.3f;
            collider.height = 1.0f;
        }

        GameObject model = Instantiate(prefab);
        model.transform.position = Vector3.zero;
        model.transform.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        model.transform.parent = boid.transform;

        GameObject head = new GameObject("Head");
        head.transform.position = new Vector3(0, 0, 0.5f);
        head.transform.parent = boid.transform;

        boid.layer = LayerMask.NameToLayer("Unit");
        foreach (Transform child in boid.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Unit");
        }

        return boid;
    }

}
