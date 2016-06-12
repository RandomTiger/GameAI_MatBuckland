using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Vector3i
{
    public Vector3i(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public int x, y, z;
}

public class FlockManager : MonoBehaviour
{
    public GameObject EntityPrefab;
    public int Quantity = 30;
    public GameObject Target;
    public bool SpatialPartition = true;
    public int SpatialDivide = 10;

    List<FlockEntity> m_Entities = new List<FlockEntity>();

    BasicSP<FlockEntity> m_BasicSP;

    void Start()
    {
        m_BasicSP = new BasicSP<FlockEntity>(new Vector3i(SpatialDivide, SpatialDivide, SpatialDivide), new Vector3(-50f, -50f, -50f), new Vector3(50f, 50f, 50f));

        for (int i = 0; i < Quantity; i++)
        {
            FlockEntity entity = Instantiate(EntityPrefab).GetComponent<FlockEntity>();
            m_Entities.Add(entity);

            // Group them under manager so they dont swamp the scene hierarchy 
            entity.transform.parent = transform;
            entity.m_Velocity = Vector3.forward;
            entity.transform.position = Random.insideUnitSphere * 10;
        }
    }

    void Update()
    {
        List<FlockEntity> nearbyEntities = new List<FlockEntity>();
        if(SpatialPartition)
        {
            m_BasicSP.Update(m_Entities);
        }

        for (int i = 0; i < m_Entities.Count; i++)
        {
            if(SpatialPartition)
            {
                m_BasicSP.GetEntitiesInRange(ref nearbyEntities, m_Entities[i].Position, m_Entities[i].m_Sight);
            }
            else
            {
                nearbyEntities = m_Entities;
            }

            m_Entities[i].Tagged = false;
            // Dont use Unity update per entity, its not efficent on mobile
            m_Entities[i].UpdateInternal(nearbyEntities, Target.transform.position); 
            m_Entities[i].Tagged = true;
        }
    }

    void OnDrawGizmos()
    {
        if(m_BasicSP != null)
        {
            m_BasicSP.OnDrawGizmos();
        }
    }
}
