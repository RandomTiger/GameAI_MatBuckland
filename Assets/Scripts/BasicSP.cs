using UnityEngine;
using System.Collections.Generic;

class BasicSP<FlockEntities> //where T : IEntity
{
    Vector3i m_NumCells;
    Vector3 m_CellSize;

    Bounds m_Bounds;

    List<FlockEntity>[] m_Cells;

    public BasicSP(Vector3i cells, Vector3 min, Vector3 max)
    {
        m_NumCells = cells;

        m_Bounds.SetMinMax(min, max);
        m_CellSize = max - min;

        m_Cells = new List<FlockEntity>[m_NumCells.x * m_NumCells.y * m_NumCells.z];
        for (int i = 0; i < m_Cells.Length; i++)
        {
            m_Cells[i] = new List<FlockEntity>();
        }
    }

    public void Update(List<FlockEntity> entities)
    {
        m_CellSize.x = m_Bounds.size.x / m_NumCells.x;
        m_CellSize.y = m_Bounds.size.y / m_NumCells.y;
        m_CellSize.z = m_Bounds.size.z / m_NumCells.z;

        // Reconfigure bounds
        m_Bounds.SetMinMax(entities[0].Position, entities[0].Position);
        for (int e = 1; e < entities.Count; e++)
        {
            m_Bounds.Encapsulate(entities[e].Position);
        }

        // todo: Stablise the bounds a little?

        for (int i = 0; i < m_Cells.Length; i++)
        {
            m_Cells[i].Clear();
        }

        for (int e = 0; e < entities.Count; e++)
        {
            int index = GetIndex(entities[e].Position);
            entities[e].SPIndex = index;
            m_Cells[index].Add(entities[e]);
        }
    }

    public static bool SphereAABBIntersect(Vector3 aabbMin, Vector3 aabbMax, Vector3 sphereCenter, float sphereRadius)
    {
        Vector3 closestPointInAabb = Vector3.Min(Vector3.Max(sphereCenter, aabbMin), aabbMax);
        float sqrMag = (closestPointInAabb - sphereCenter).sqrMagnitude;

        // The AABB and the sphere overlap if the closest point within the rectangle is within the sphere's radius
        return sqrMag < (sphereRadius * sphereRadius);
    }

    public void GetEntitiesInRange(ref List<FlockEntity> found, Vector3 pos, float range, bool clear = true)
    {
        if (clear)
        {
            found.Clear();
        }

        for (int c = 0; c < m_Cells.Length; c++)
        {
            Vector3 cell = GetCellPos(c);
            if (SphereAABBIntersect(cell, cell + m_CellSize, pos, range) == false)
            {
                continue;
            }

            List<FlockEntity> cellEntities = m_Cells[c];
            for (int i = 0; i < cellEntities.Count; i++)
            {
                if ((cellEntities[i].Position - pos).sqrMagnitude <= range * range)
                {
                    found.Add(cellEntities[i]);
                }
            }
        }
    }

    private int GetIndex(Vector3 pos)
    {
        int cellX = Mathf.FloorToInt((pos.x - m_Bounds.min.x) / m_CellSize.x);
        int cellY = Mathf.FloorToInt((pos.y - m_Bounds.min.y) / m_CellSize.y);
        int cellZ = Mathf.FloorToInt((pos.z - m_Bounds.min.z) / m_CellSize.z);

        // Pull into nearest cell
        cellX = Mathf.Clamp(cellX, 0, m_NumCells.x - 1);
        cellY = Mathf.Clamp(cellY, 0, m_NumCells.y - 1);
        cellZ = Mathf.Clamp(cellZ, 0, m_NumCells.z - 1);

        return GetIndex(cellX, cellY, cellZ);
    }

    int GetIndex(int x, int y, int z)
    {
        return x + y * m_NumCells.x + z * m_NumCells.x * m_NumCells.y;
    }

    Vector3 GetCellPos(int x, int y, int z)
    {
        Vector3 result = Vector3.zero;
        result.x = m_Bounds.min.x + x * m_CellSize.x;
        result.y = m_Bounds.min.y + y * m_CellSize.y;
        result.z = m_Bounds.min.z + z * m_CellSize.z;

        return result;
    }

    Vector3 GetCellPos(int index)
    {
        int storedIndex = index;

        int z = index / (m_NumCells.x * m_NumCells.y);
        index -= (z * m_NumCells.x * m_NumCells.y);
        int y = index / m_NumCells.x;
        int x = index % m_NumCells.x;

        int testIndex = GetIndex(x, y, z);
        Debug.Assert(storedIndex == testIndex);

        return GetCellPos(x, y, z);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        for (int x = 0; x < m_NumCells.x; x++)
        {
            for (int y = 0; y < m_NumCells.y; y++)
            {
                for (int z = 0; z < m_NumCells.z; z++)
                {
                    Vector3 cell = GetCellPos(x, y, z);
                    Vector3 pos = cell + (m_CellSize * 0.5f);

                    Gizmos.color = m_Cells[GetIndex(x, y, z)].Count > 0 ? Color.blue : Color.green;
                    Gizmos.DrawWireCube(pos, m_CellSize);
                }
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_Bounds.center, m_Bounds.size);
    }
}