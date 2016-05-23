using UnityEngine;
using System.Collections.Generic;

public class FlockManager : MonoBehaviour {

    public GameObject EntityPrefab;
    public int Quantity = 30;
    public GameObject Target;

    List<FlockEntity> m_Entities = new List<FlockEntity>();

	void Start ()
    {
	    for(int i = 0; i < Quantity; i++)
        {
            FlockEntity entity = Instantiate(EntityPrefab).GetComponent<FlockEntity>();
            m_Entities.Add(entity);

            // Group them under manager so they dont swamp the scene hierarchy 
            entity.transform.parent = transform;
            entity.m_Velocity = Vector3.forward;
            entity.transform.position = Random.insideUnitSphere * 10;
        }
	}
	
	void Update ()
    {
        for (int i = 0; i < m_Entities.Count; i++)
        {
            m_Entities[i].Tagged = false;
            // Dont use Unity update per entity, its not efficial on mobile
            m_Entities[i].UpdateInternal(m_Entities, Target.transform.position); // todo, send through sublist based on spacial partition
            m_Entities[i].Tagged = true;
        }
    }
}
