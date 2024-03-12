using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PoSquarePushAgent : Agent
{
    private Rigidbody agentRb;
    private TargetController target;
    public float moveSpeed = 100f;
    public float pushPower = 200f;
    public int maxSteps = 5000;
    private int m_StepCount;

    private void Start()
    {
        agentRb = GetComponent<Rigidbody>();
        target = GameObject.Find("Target").GetComponent<TargetController>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent position and rotation
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Reset agent velocity
        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;

        // Reset target position
        target.ResetTarget();

        // Reset step count
        m_StepCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent observation: target position
        sensor.AddObservation(target.transform.localPosition);

        // Agent observation: agent position
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the movement input from the actions
        Vector3 movement = new Vector3(actions.ContinuousActions[0], 0f, actions.ContinuousActions[1]);
        movement.Normalize();

        // Apply the movement input to the agent
        agentRb.AddForce(movement * moveSpeed);

        // Check if the agent is touching the target
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Target"))
            {
                // Apply push force to the target
                Vector3 pushDirection = (collider.transform.position - transform.position).normalized;
                collider.attachedRigidbody.AddForce(pushDirection * pushPower, ForceMode.Impulse);
            }
        }

        // Reward for moving towards the target quickly
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);
        AddReward(1f / distanceToTarget);

        // Check if the target is off the platform
        if (target.transform.localPosition.y < 0f)
        {
            // Give a large positive reward for pushing the target off the platform
            AddReward(10f);

            // End the episode
            EndEpisode();
        }

        // Check if the agent is off the platform
        if (transform.localPosition.y < 0f)
        {
            // Give a penalty for falling off the platform
            AddReward(-5f);

            // End the episode
            EndEpisode();
        }

        // Increment the step count
        m_StepCount++;

        // Check if the step count exceeds the maximum number of steps
        if (m_StepCount >= maxSteps)
        {
            // End the episode if the maximum number of steps is reached
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
}