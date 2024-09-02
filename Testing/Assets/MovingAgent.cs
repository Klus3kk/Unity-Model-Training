using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class NewBehaviourScript : Agent
{
    public float moveSpeed = 0.1f;
    public float rotationSpeed = 3f;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    [SerializeField] private Transform target;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMesh;
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(0f, 2f), 0, Random.Range(0f, 2f));
        target.localPosition = new Vector3(Random.Range(0f, 3f), 0, Random.Range(3f, 5f));
        transform.localRotation = Quaternion.Euler(0.0f, -100.0f, 0.0f);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 movement = new Vector3(moveX, 0.0f, moveZ) * Time.deltaTime * moveSpeed;
        rb.MovePosition(rb.position + movement);

        // Apply force to the ball to move it
        Vector3 force = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.AddForce(force, ForceMode.VelocityChange);

        // Make the ball spin by applying torque
        Vector3 rotation = new Vector3(moveX, 0.0f, -moveZ);
        rb.AddTorque(rotation * rotationSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(1f);
            floorMesh.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall wall) || transform.position.y < -0.1f)
        {
            SetReward(-1f);
            floorMesh.material = loseMaterial;
            EndEpisode();
        }
    }
}
