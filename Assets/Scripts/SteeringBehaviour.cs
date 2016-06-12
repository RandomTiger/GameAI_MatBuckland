using UnityEngine;
using System.Collections.Generic;

public class SteeringBehaviour
{
    [System.Flags]
    public enum Behaviour
    {
        flee = 1 << 0,
        separation = 1 << 1,
        allignment = 1 << 2,
        cohesion = 1 << 3,
        wander = 1 << 4,
        seek = 1 << 5,
    }

    Vector3 m_WanderTarget = Vector3.forward;
    public Vector3 WanderTarget
    {
        get { return m_WanderTarget; }
    }

    float m_WanderJitter = 1.0f;
    float m_WanderRadius = 1.0f;
    float m_WanderDistance = 1.0f;

    Vector3 m_SteeringForce;
    Behaviour m_Behaviour;
    public Behaviour Behaviours
    {
        set { m_Behaviour = value; }
    }

    public Vector3 Calc(FlockEntity entity, List<FlockEntity> nearbyEntities, Vector3 target)
    {
        //reset the steering force
        m_SteeringForce = Vector3.zero;

        // summing method
        return CalculateWeightedSum(entity, nearbyEntities, target);
    }

    public bool On(Behaviour flag)
    {
        return (m_Behaviour & flag) == flag;
    }



    //---------------------- CalculatePrioritized ----------------------------
    //
    //  this method calls each active steering behavior in order of priority
    //  and acumulates their forces until the max steering force magnitude
    //  is reached, at which time the function returns the steering force 
    //  accumulated to that  point
    //------------------------------------------------------------------------
    Vector3 CalculatePrioritized(FlockEntity entity, List<FlockEntity> nearby, Vector3 target)
    {
        Vector3 force;
        /*
        if (On(Behaviour.wall_avoidance))
        {
            force = WallAvoidance(m_pVehicle->World()->Walls()) *
                    m_dWeightWallAvoidance;

            if (!AccumulateForce(entity, ref m_vSteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.obstacle_avoidance))
        {
            force = ObstacleAvoidance(m_pVehicle->World()->Obstacles()) *
                    m_dWeightObstacleAvoidance;

            if (!AccumulateForce(entity, ref m_vSteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.evade))
        {
            assert(m_pTargetAgent1 && "Evade target not assigned");

            force = Evade(m_pTargetAgent1) * m_dWeightEvade;

            if (!AccumulateForce(entity, ref m_vSteeringForce, force)) return m_SteeringForce;
        }
        */

        if (On(Behaviour.flee))
        {
            force = Flee(entity, target) * entity.WeightFlee;

            if (!AccumulateForce(entity, ref m_SteeringForce, force)) return m_SteeringForce;
        }



        //these next three can be combined for flocking behavior (wander is
        //also a good behavior to add into this mix)

        if (On(Behaviour.separation))
        {
            force = Separation(entity, nearby) * entity.WeightSeparation;

            if (!AccumulateForce(entity, ref m_SteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.allignment))
        {
            force = Alignment(entity, nearby) * entity.WeightAlignment;

            if (!AccumulateForce(entity, ref m_SteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.cohesion))
        {
            force = Cohesion(entity, nearby) * entity.WeightCohesion;

            if (!AccumulateForce(entity, ref m_SteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.seek))
        {
            force = Seek(entity, target) * entity.WeightSeek;

            if (!AccumulateForce(entity, ref m_SteeringForce, force)) return m_SteeringForce;
        }

        /*
        if (On(Behaviour.arrive))
        {
            force = Arrive(m_pVehicle->World()->Crosshair(), m_Deceleration) * m_dWeightArrive;

            if (!AccumulateForce(m_SteeringForce, force)) return m_SteeringForce;
        }
        */
        if (On(Behaviour.wander))
        {
            force = Wander(entity) * entity.WeightWander;

            if (!AccumulateForce(entity, ref m_SteeringForce, force)) return m_SteeringForce;
        }
        /*
        if (On(Behaviour.pursuit))
        {
            assert(m_pTargetAgent1 && "pursuit target not assigned");

            force = Pursuit(m_pTargetAgent1) * m_dWeightPursuit;

            if (!AccumulateForce(m_SteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.offset_pursuit))
        {
            assert(m_pTargetAgent1 && "pursuit target not assigned");
            assert(!m_vOffset.isZero() && "No offset assigned");

            force = OffsetPursuit(m_pTargetAgent1, m_vOffset);

            if (!AccumulateForce(m_vSteeringForce, force)) return m_SteeringForce;
        }

        if (On(Behaviour.interpose))
        {
            Debug.Assert(m_pTargetAgent1 && m_pTargetAgent2, "Interpose agents not assigned");

            force = Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_dWeightInterpose;

            if (!AccumulateForce(m_vSteeringForce, force)) return m_vSteeringForce;
        }

        if (On(Behaviour.hide))
        {
            assert(m_pTargetAgent1 && "Hide target not assigned");

            force = Hide(m_pTargetAgent1, m_pVehicle->World()->Obstacles()) * m_dWeightHide;

            if (!AccumulateForce(m_vSteeringForce, force)) return m_SteeringForce;
        }


        if (On(Behaviour.follow_path))
        {
            force = FollowPath() * m_dWeightFollowPath;

            if (!AccumulateForce(m_vSteeringForce, force)) return m_SteeringForce;
        }
        */
        return m_SteeringForce;
    }



    //--------------------- AccumulateForce ----------------------------------
    //
    //  This function calculates how much of its max steering force the 
    //  vehicle has left to apply and then applies that amount of the
    //  force to add.
    //------------------------------------------------------------------------
    bool AccumulateForce(FlockEntity entity, ref Vector3 RunningTot, Vector3 ForceToAdd)
    {
        //calculate how much steering force the vehicle has used so far
        float MagnitudeSoFar = RunningTot.magnitude;

        //calculate how much steering force remains to be used by this vehicle
        float MagnitudeRemaining = entity.m_MaxForce - MagnitudeSoFar;

        //return false if there is no more force left to use
        if (MagnitudeRemaining <= 0.0) return false;

        //calculate the magnitude of the force we want to add
        float MagnitudeToAdd = ForceToAdd.magnitude;

        //if the magnitude of the sum of ForceToAdd and the running total
        //does not exceed the maximum force available to this vehicle, just
        //add together. Otherwise add as much of the ForceToAdd vector is
        //possible without going over the max.
        if (MagnitudeToAdd < MagnitudeRemaining)
        {
            RunningTot += ForceToAdd;
        }

        else
        {
            //add it to the steering force
            RunningTot += (Vector3.Normalize(ForceToAdd) * MagnitudeRemaining);
        }

        return true;
    }

    //---------------------- CalculateWeightedSum ----------------------------
    //
    //  this simply sums up all the active behaviors X their weights and 
    //  truncates the result to the max available steering force before 
    //  returning
    //------------------------------------------------------------------------
    Vector3 CalculateWeightedSum(FlockEntity entity, List<FlockEntity> nearby, Vector3 target)
    {
        if (On(Behaviour.separation))
        {
            m_SteeringForce += Separation(entity, nearby) * entity.WeightSeparation;
        }

        if (On(Behaviour.allignment))
        {
            m_SteeringForce += Alignment(entity, nearby) * entity.WeightAlignment;
        }

        if (On(Behaviour.cohesion))
        {
            m_SteeringForce += Cohesion(entity, nearby) * entity.WeightCohesion;
        }

        if (On(Behaviour.wander))
        {
            m_SteeringForce += Wander(entity) * entity.WeightWander;
        }
        
        if (On(Behaviour.seek))
        {
            m_SteeringForce += Seek(entity, target) * entity.WeightSeek;
        }
        
        if (On(Behaviour.flee))
        {
            m_SteeringForce += Flee(entity, target) * entity.WeightFlee;
        }
        /*
        if (On(Behaviour.arrive))
        {
            m_vSteeringForce += Arrive(m_pVehicle->World()->Crosshair(), m_Deceleration) * m_dWeightArrive;
        }

        if (On(Behaviour.pursuit))
        {
            Debug.Assert(m_pTargetAgent1, "pursuit target not assigned");

            m_vSteeringForce += Pursuit(m_pTargetAgent1) * m_dWeightPursuit;
        }

        if (On(offset_pursuit))
        {
            Debug.Assert(m_pTargetAgent1, "pursuit target not assigned");
            Debug.Assert(!m_vOffset.isZero(), "No offset assigned");

            m_vSteeringForce += OffsetPursuit(m_pTargetAgent1, m_vOffset) * m_dWeightOffsetPursuit;
        }
        */
        Vector3.ClampMagnitude(m_SteeringForce, entity.m_MaxForce);

        return m_SteeringForce;
    }

    Vector3 Flee(FlockEntity entity, Vector3 TargetPos)
    {
        //only flee if the target is within 'panic distance'. Work in distance
        //squared space.
        /* const double PanicDistanceSq = 100.0f * 100.0;
         if (Vec2DDistanceSq(m_pVehicle->Pos(), target) > PanicDistanceSq)
         {
           return Vector2D(0,0);
         }
         */

        Vector3 DesiredVelocity = Vector3.Normalize(entity.Position - TargetPos) * entity.m_MaxSpeed;
        return (DesiredVelocity - entity.m_Velocity);
    }

    //---------------------------- Separation --------------------------------
    //
    // this calculates a force repelling from the other neighbors
    //------------------------------------------------------------------------
    Vector3 Separation(FlockEntity entity, List<FlockEntity> nearby)
    {
        Vector3 SteeringForce = Vector3.zero;

        for (int a = 0; a < nearby.Count; ++a)
        {
            //make sure this agent isn't included in the calculations and that
            //the agent being examined is close enough. ***also make sure it doesn't
            //include the evade target ***
            if (nearby[a].Tagged)
            {
                Vector3 ToAgent = entity.transform.position - nearby[a].transform.position;

                if (ToAgent.sqrMagnitude < 0.001f)
                {
                    // if next to randomly move away from
                    SteeringForce += Random.insideUnitSphere;
                }
                else
                {
                    //scale the force inversely proportional to the agents distance  
                    //from its neighbor.
                    SteeringForce += Vector3.Normalize(ToAgent) / ToAgent.magnitude;
                }
            }
        }

        return SteeringForce;
    }


    //---------------------------- Alignment ---------------------------------
    //
    //  returns a force that attempts to align this agents heading with that
    //  of its neighbors
    //------------------------------------------------------------------------
    Vector3 Alignment(FlockEntity entity, List<FlockEntity> nearby)
    {
        //used to record the average heading of the neighbors
        Vector3 AverageHeading = Vector3.zero;

        //used to count the number of vehicles in the neighborhood
        int NeighborCount = 0;

        //iterate through all the tagged vehicles and sum their heading vectors  
        for (int a = 0; a < nearby.Count; ++a)
        {
            //make sure *this* agent isn't included in the calculations and that
            //the agent being examined  is close enough ***also make sure it doesn't
            //include any evade target ***
            if (nearby[a].Tagged)
            {
                AverageHeading += nearby[a].Heading;
                ++NeighborCount;
            }
        }

        //if the neighborhood contained one or more vehicles, average their
        //heading vectors.
        if (NeighborCount > 0)
        {
            AverageHeading /= (float)NeighborCount;
            AverageHeading -= entity.Heading;
        }

        return AverageHeading;
    }

    //-------------------------------- Cohesion ------------------------------
    //
    //  returns a steering force that attempts to move the agent towards the
    //  center of mass of the agents in its immediate area
    //------------------------------------------------------------------------
    Vector3 Cohesion(FlockEntity entity, List<FlockEntity> nearby)
    {
        //first find the center of mass of all the agents
        Vector3 CenterOfMass = Vector3.zero;
        Vector3 SteeringForce = Vector3.zero;

        int NeighborCount = 0;

        //iterate through the neighbors and sum up all the position vectors
        for (int a = 0; a < nearby.Count; ++a)
        {
            if (nearby[a].Tagged)
            {
                CenterOfMass += nearby[a].transform.position;
                ++NeighborCount;
            }
        }

        if (NeighborCount > 0)
        {
            //the center of mass is the average of the sum of positions
            CenterOfMass /= (float)NeighborCount;

            //now seek towards that position
            SteeringForce = Seek(entity, CenterOfMass);
        }

        //the magnitude of cohesion is usually much larger than separation or
        //allignment so it usually helps to normalize it.
        return SteeringForce.normalized;
    }

    Vector3 Seek(FlockEntity entity, Vector3 TargetPos)
    {
        Vector3 DesiredVelocity = Vector3.Normalize(TargetPos - entity.Position) * entity.m_MaxSpeed;
        return (DesiredVelocity - entity.m_Velocity);
    }
   
    Vector3 Wander(FlockEntity entity)
    {
        //this behavior is dependent on the update rate, so this line must
        //be included when using time independent framerate.
        float JitterThisTimeSlice = m_WanderJitter * Time.deltaTime;

        //first, add a small random vector to the target's position
        m_WanderTarget += Random.insideUnitSphere * JitterThisTimeSlice;

        //reproject this new vector back on to a unit circle
        m_WanderTarget.Normalize();

        //increase the length of the vector to the same as the radius
        //of the wander circle
        m_WanderTarget *= m_WanderRadius;

        //move the target into a position WanderDist in front of the agent
        Vector3 target = m_WanderTarget + new Vector3(0,0,m_WanderDistance);

        target = entity.transform.rotation * target;

        //and steer towards it
        return target;
    }
}