using System.Collections.Generic;
using UnityEngine;

public class PathCircuit : MonoBehaviour
{
    public List<Transform> nodes;

    //Funciona como un update pero en unity en general, no en el tiempo de ejecucion
    private void OnDrawGizmos()
    {
        Transform[] tempNodes = GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < tempNodes.Length; i++)
        {
            if (tempNodes[i] != transform)
            {
                nodes.Add(tempNodes[i]);
                Gizmos.DrawWireSphere(tempNodes[i].position, 5);
            }
        }
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i+1 != nodes.Count)
            {
                Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
            }
            else
            {
                Gizmos.DrawLine(nodes[i].position, nodes[0].position);
            }
            
        }
    }
}
