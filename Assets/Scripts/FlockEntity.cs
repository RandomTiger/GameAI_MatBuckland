using UnityEngine;
using System.Collections.Generic;

public class EnumFlagAttribute : PropertyAttribute
{
    public string enumName;

    public EnumFlagAttribute() { }

    public EnumFlagAttribute(string name)
    {
        enumName = name;
    }
}

public class FlockEntity : MonoBehaviour
{
    public Vector3 m_Velocity = Vector3.zero;
    public float m_Mass = 1;
    public float m_MaxSpeed = 10.0f;
    public float m_MaxForce = 10.0f;
    public float m_MaxTurnRate = 5.0f;
    [EnumFlagAttribute] public SteeringBehaviour.Behaviour m_Behaviours;

    public float WeightSeparation = 1;
    public float WeightAlignment = 1;
    public float WeightCohesion = 1;
    public float WeightWander = 1;
    public float WeightSeek = 1;

    public bool Tagged = true;

    public Vector3 Position
    {
        get { return transform.position; }
    }

    public Vector3 Heading
    {
        get { return transform.forward; }
    }

    SteeringBehaviour m_Steering = new SteeringBehaviour();

    void Start ()
    {
        m_Steering.Behaviours = m_Behaviours;
    }
	
	public void UpdateInternal(List<FlockEntity> nearby, Vector3 target)
    {
        Vector3 steer = m_Steering.Calc(this, nearby, target);

        Vector3 accel = steer / m_Mass;

        m_Velocity += accel * Time.deltaTime;

        Vector3.ClampMagnitude(m_Velocity, m_MaxSpeed);

        RotateHeadingToFacePosition(transform.position + m_Velocity);
        transform.position += m_Velocity.magnitude * Heading * Time.deltaTime;

        //transform.LookAt(transform.position + m_Velocity);
    }

    bool RotateHeadingToFacePosition(Vector3 target)
    {
        Vector3 toTarget = Vector3.Normalize(target - Position);

        //first determine the angle between the heading vector and the target
        float angle = Mathf.Acos(Vector3.Dot(Heading, toTarget));

        //return true if the player is facing the target
        if (angle < 0.00001)
        {
            return true;
        }

        //clamp the amount to turn to the max turn rate
        if (angle > m_MaxTurnRate)
        {
            angle = m_MaxTurnRate;
        }

        Vector3 newHeading = Vector3.RotateTowards(Heading, toTarget, angle * Mathf.Deg2Rad * Time.fixedTime, 1.0f);
        transform.LookAt(transform.position + newHeading);
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.25f);
        Vector3 wander = transform.position + (Vector3)(transform.localToWorldMatrix * m_Steering.WanderTarget);
        Gizmos.DrawSphere(wander, 0.25f);
        Gizmos.DrawLine(transform.position, wander);
    }
}
