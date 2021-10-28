using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class EdgeManager : MonoBehaviour
{


    public Dictionary<double, float> dRightElevation = new Dictionary<double, float>();
    public Dictionary<double, float> dUpElevation = new Dictionary<double, float>();


    public void EdgeGenerator(TriangleNet.Mesh mesh)
    {
        foreach(Vertex vert in mesh.Vertices)
        {

        }
    }


    //List<Vertex> lRight = new List<Vertex>();


    public Dictionary<double, float> RightEdgeGenerator(TriangleNet.Mesh mesh, List<float> elevations)
    {
        foreach (Vertex vert in mesh.Vertices)
        {
            if(vert.y == 0)
            {
                dRightElevation.Add(vert.x, elevations[vert.hash]);
            }
        }
        return dRightElevation;
    }

    
    public void UpEdgeGenerator(Dictionary<double, float> myEdge, TriangleNet.Mesh mesh, List<float> elevations,int xsize)
    {
        foreach (Vertex vert in mesh.Vertices)
        {
            if (vert.x == xsize)
            {
                dUpElevation.Add(vert.y, elevations[vert.hash]);
            }
        }
        myEdge = dUpElevation;
    }
}
