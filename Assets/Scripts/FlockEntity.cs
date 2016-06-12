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

public interface IEntity
{
    Vector3 Position
    {
        get;
    }
    int SPIndex
    {
        get; set;
    }
}

public class FlockEntity : MonoBehaviour, IEntity
{
    public Vector3 m_Velocity = Vector3.zero;
    public float m_Mass = 1;
    public float m_MaxSpeed = 10.0f;
    public float m_MaxForce = 10.0f;
    public float m_MaxTurnRate = 5.0f;
    public float m_Sight = 10.0f;
    [EnumFlagAttribute] public SteeringBehaviour.Behaviour m_Behaviours;

    public float WeightSeparation = 1;
    public float WeightAlignment = 1;
    public float WeightCohesion = 1;
    public float WeightWander = 1;
    public float WeightSeek = 1;
    public float WeightFlee = 1;

    [HideInInspector] public bool Tagged = true;
    private int m_SPIndex;
    public int SPIndex
    {
        get { return m_SPIndex;  }
        set { m_SPIndex = value; }
    }

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
        LookAt(transform.position + newHeading);
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.25f);
        Vector3 wander = transform.position + (Vector3)(transform.localToWorldMatrix * m_Steering.WanderTarget);
        Gizmos.DrawSphere(wander, 0.25f);
        Gizmos.DrawLine(transform.position, wander);
    }

    void LookAt(Vector3 target)
    {
    //    transform.LookAt(target);
   //     transform.rotation = Quaternion.LookRotation(target - transform.position);
        transform.rotation = LookAt(transform.position, target);
    }

    public static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
    {
        Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);

        float dot = Vector3.Dot(Vector3.forward, forwardVector);

        if (Mathf.Abs(dot - (-1.0f)) < 0.000001f)
        {
            return new Quaternion(Vector3.up.x, Vector3.up.y, Vector3.up.z, Mathf.PI);
        }
        if (Mathf.Abs(dot - (1.0f)) < 0.000001f)
        {
            return Quaternion.identity;
        }

        float rotAngle = (float)Mathf.Acos(dot);
        Vector3 rotAxis = Vector3.Cross(Vector3.forward, forwardVector);
        rotAxis = Vector3.Normalize(rotAxis);
        return CreateFromAxisAngle(rotAxis, rotAngle);
    }

    // just in case you need that function also
    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float halfAngle = angle * 0.5f;
        float s = (float)System.Math.Sin(halfAngle);
        Quaternion q;
        q.x = axis.x * s;
        q.y = axis.y * s;
        q.z = axis.z * s;
        q.w = (float)System.Math.Cos(halfAngle);
        return q;
    }
}
