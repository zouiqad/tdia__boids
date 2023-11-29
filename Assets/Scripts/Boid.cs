using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 2f;
    public float neighborRadius = 5f;
    public float separationDistance = 4f;

    // Define bounding box constraints
    public float minX = -20f;
    public float maxX = 20f;
    public float minY = -20f;
    public float maxY = 20f;
    public float minZ = -20f;
    public float maxZ = 20f;

    public List<Vector3> near;

    private void UpdateBounds()
    {
        GameObject boundingBox = GameObject.Find("Cube");
        Mesh boundingBoxMesh = boundingBox.GetComponent<MeshFilter>().mesh;

        float scale = 
        minX = boundingBoxMesh.bounds.min.x * boundingBox.transform.localScale.x;
        maxX = boundingBoxMesh.bounds.max.x * boundingBox.transform.localScale.x;
        minY = boundingBoxMesh.bounds.min.y * boundingBox.transform.localScale.y;
        maxY = boundingBoxMesh.bounds.max.y * boundingBox.transform.localScale.y;
        minZ = boundingBoxMesh.bounds.min.z * boundingBox.transform.localScale.z;
        maxZ = boundingBoxMesh.bounds.max.z * boundingBox.transform.localScale.z;
    }

    void Update()
    {
        UpdateBounds();

        // Apply boids rules
        Vector3 separation = Separation();
        Vector3 alignment = Alignment();
        Vector3 cohesion = Cohesion();

        // Combine rules and move the boid
        Vector3 moveDirection = separation + alignment + cohesion;

        // Check and handle bounding box constraints
        CheckBounds();

        transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.Self);

        transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.Self);

        // Rotate towards the movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 Separation()
    {
        Vector3 separationVector = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, neighborRadius);
        
        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject != gameObject)
            {
                Vector3 toNeighbor = transform.position - neighbor.transform.position;
                float distance = toNeighbor.magnitude;
                near.Add(neighbor.transform.position);
                Debug.Log($"{neighbor.name} distance {distance}");
                if (distance < separationDistance)
                {
                    separationVector += toNeighbor.normalized / distance;
                }

            }
        }


        return separationVector;
    }

    void CheckBounds()
    {
        Vector3 pos = transform.position;

        // Check X axis
        if (pos.x < minX || pos.x > maxX)
        {
            transform.position = new Vector3(Mathf.Clamp(pos.x, minX, maxX), pos.y, pos.z);
            BounceOnAxis(true, false, false);
        }

        // Check Y axis
        if (pos.y < minY || pos.y > maxY)
        {
            transform.position = new Vector3(pos.x, Mathf.Clamp(pos.y, minY, maxY), pos.z);
            BounceOnAxis(false, true, false);
        }

        // Check Z axis
        if (pos.z < minZ || pos.z > maxZ)
        {
            transform.position = new Vector3(pos.x, pos.y, Mathf.Clamp(pos.z, minZ, maxZ));
            BounceOnAxis(false, false, true);
        }
    }

    void BounceOnAxis(bool bounceX, bool bounceY, bool bounceZ)
    {
        Vector3 moveDirection = Vector3.zero;

        if (bounceX)
        {
            moveDirection.x = -Mathf.Sign(transform.forward.x);
        }

        if (bounceY)
        {
            moveDirection.y = -Mathf.Sign(transform.forward.y);
        }

        if (bounceZ)
        {
            moveDirection.z = -Mathf.Sign(transform.forward.z);
        }

        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
    }

    void OnDrawGizmos()
    {
        // Draw the main sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);

        foreach(var n in near)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, n);
        }

    }
            
    Vector3 Alignment()
    {
        Vector3 alignmentVector = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, neighborRadius);

        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject != gameObject)
            {
                alignmentVector += neighbor.transform.forward;
            }
        }

        return alignmentVector.normalized;
    }

    Vector3 Cohesion()
    {
        Vector3 cohesionVector = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, neighborRadius);
        int count = 0;

        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject != gameObject)
            {
                cohesionVector += neighbor.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            cohesionVector /= count;
            cohesionVector = (cohesionVector - transform.position).normalized;
        }

        return cohesionVector;
    }
}
