using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class BoidUnit : MonoBehaviour
{
    Transform tip;
    Collider[] colls;
    List<Transform> neighbors;
    LayerMask unitLayerMask;
    BoidsManager boidsManager;

    // obstacle avoidance
    readonly int numAvoidanceDir = 200;
    Vector3[] avoidanceDirs;
    public LayerMask obstacleLayerMask;
    public float obstacleDistance = 7.0f;
    public int maxNeighbors = 20;

    void Start()
    {
        unitLayerMask = LayerMask.GetMask("Unit");
        boidsManager = GameObject.Find("BoidsManager").GetComponent<BoidsManager>();
        neighbors = new List<Transform>();

        avoidanceDirs = new Vector3[numAvoidanceDir];
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        tip = this.transform.GetChild(1);

        //float thetaRange = Mathf.Cos(Mathf.Deg2Rad * boidsManager.FOV);
        float thetaRange = Mathf.Cos(Mathf.Deg2Rad * 125f);
        for (int i = 0; i < numAvoidanceDir; i++)
        {
            float t = i / (numAvoidanceDir - 1.0f);
            float inclination = Mathf.Acos(1 - (1 - thetaRange) * t);
            float azimuth = 2 * Mathf.PI * i * 1.618f;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            avoidanceDirs[i] = new Vector3(x, y, z);
        }

        colls = new Collider[maxNeighbors];
        StartCoroutine(FindNeighborsCoroutine());
    }

    void Update()
    {
        //FindNeighbors(); // coroutine에서 해주고 있음

       Vector3 dir = Vector3.zero;
        
        dir += GetAvoidance();
        if (dir == transform.forward) // == transform.TransformDirection(avoidanceDirs[i])
        {
            if (neighbors.Count > 0)
            {
                dir += GetSeparation();
                dir += GetCohesion();
                dir += GetAlignment();
            }
        }

        
        MoveInDirection(dir);
    }

    Vector3 GetSeparation()
    {
        if (neighbors == null || neighbors.Count == 0)
            return Vector3.zero;

        Vector3 vec = Vector3.zero;
        for (int i = 0; i < neighbors.Count; ++i)
        {
            if (this.transform == neighbors[i])
                continue;

            Vector3 temp = this.transform.position - neighbors[i].position;
            vec += temp.normalized / temp.magnitude * (1f);
            //vec += this.transform.position - neighbors[i].position;
        }

        vec.Normalize();
        Debug.DrawLine(transform.position, transform.position + vec.normalized, Color.cyan);
        return vec * boidsManager.separationWeight;
    }

    Vector3 GetCohesion()
    {
        if (neighbors == null || neighbors.Count == 0)
            return Vector3.zero;

        Vector3 vec = Vector3.zero;
        for (int i = 0; i < neighbors.Count; ++i)
        {
            vec += neighbors[i].position;
        }
        vec /= neighbors.Count;
        vec -= this.transform.position;

        Debug.DrawLine(transform.position, transform.position + vec.normalized, Color.magenta);
        return vec.normalized * boidsManager.cohesionWeight;
    }

    Vector3 GetAlignment()
    {
        if (neighbors == null || neighbors.Count == 0)
            return this.transform.forward;

        Vector3 vec = Vector3.zero;
        for (int i = 0; i < neighbors.Count; ++i)
        {
            vec += neighbors[i].forward;;
        }


        //Debug.DrawLine(transform.position, transform.position + vec.normalized, Color.blue);
        return vec.normalized * boidsManager.alignmentWeight;
    }

    Vector3 GetAvoidance()
    {
        Vector3 detectPos = tip.position;
        Vector3 moveDir = Vector3.zero;
        RaycastHit hit;
        float maxDist = 0f;
        float rayRadius = 0.5f;
        for (int i = 0; i < numAvoidanceDir; i++)
        {
            Vector3 dir = tip.TransformDirection(avoidanceDirs[i]);

            if (Physics.SphereCast(detectPos, rayRadius, dir, out hit, this.obstacleDistance, obstacleLayerMask))
            {
                if (hit.distance > maxDist)
                {
                    maxDist = hit.distance;
                    moveDir = dir;
                }
            }
            else
            {
                moveDir = dir;
                break;
            }

            Debug.DrawLine(detectPos, detectPos + dir * obstacleDistance, Color.red);

        }

        Debug.DrawLine(detectPos, detectPos + moveDir * obstacleDistance, UnityEngine.Color.green);

        moveDir.Normalize();
        return moveDir;
    }

    void FindNeighbors()
    {
        neighbors.Clear();

        int collsCnt = Physics.OverlapSphereNonAlloc(this.transform.position, boidsManager.neighborRange, colls, unitLayerMask);

        for (int i=0; i<collsCnt; i++)
        {
            if (Vector3.Angle(transform.forward, colls[i].transform.position - transform.position) <= boidsManager.FOV)
            {
                neighbors.Add(colls[i].transform);
            }
        }
    }

    IEnumerator FindNeighborsCoroutine()
    {
        FindNeighbors();

        yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));

        StartCoroutine(FindNeighborsCoroutine());
    }

    void MoveInDirection(Vector3 dir)
    {
        Debug.DrawLine(tip.position, tip.position + dir, Color.black);

        dir = Vector3.Lerp(this.transform.forward, dir, Time.deltaTime * boidsManager.turnSpeed);
        dir.Normalize();


        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.position += transform.forward * Time.deltaTime * boidsManager.speed;
    }
}
