using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVisual : MonoBehaviour
{
    [SerializeField] private int rayCount = 15;
    [SerializeField] private LayerMask layerMask;
    public float fov;
    public float viewDistance = 10f;
    private Vector3 origin = Vector3.zero;
    private float startingAngle;
    private float angleIncrease;
    private Mesh mesh;

    void Start()
    {
        mesh = new();
        GetComponent<MeshFilter>().mesh = mesh;
        angleIncrease = fov / rayCount;
    }

    private void LateUpdate()
    {
        float angle = startingAngle;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1]; // define the mesh
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin - transform.position; //set the start position at the player

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++) 
        {
            Vector3 vertex;
            Physics.Raycast(origin, GetVectorFromAngle(angle), out RaycastHit hit, viewDistance, layerMask); // check for a wall
            if (hit.collider == null)
            {
                vertex = origin - transform.position + GetVectorFromAngle(angle) * viewDistance; // if there's no walls, set the vertex on the FOV radius
            }
            else
            {
                vertex = hit.point - transform.position; // if there is a wall, set the vertex on the hit point
            }
            vertices[vertexIndex] = vertex;

            if (i > 0) // define the triangles
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        // apply the calulated values to the mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        float rads = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(rads), 0, Mathf.Sin(rads));
    }

    public void SetOrigin(Vector3 newOrigin)
    {
        origin = newOrigin;
    }

    public void SetDirection(Vector3 aimDirection)
    {
        aimDirection = aimDirection.normalized;
        float n = Mathf.Atan2(aimDirection.z, aimDirection.x) * Mathf.Rad2Deg;
        if (n < 0)
        { n += 360; }
        startingAngle = n + fov / 2f ;
    }
}
