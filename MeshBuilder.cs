namespace vmodel;

using System;
using System.Collections.Generic;


public sealed class MeshBuilder
{
    //takes a hash code as an input, and outputs the index of the vertex with that hash code.
    private readonly Dictionary<int, uint> vertexLookup;
    private readonly List<float> vertices;
    private readonly List<uint> indices;
    public readonly Attributes attributes;

    public MeshBuilder(Attributes attributes)
    {
        vertexLookup = new Dictionary<int, uint>();
        vertices = new List<float>();
        indices = new List<uint>();
        this.attributes = attributes;
    }
    public MeshBuilder(Attributes attributes, int vertexCapacity, int indexCapacity)
    {
        vertexLookup = new Dictionary<int, uint>(vertexCapacity);
        vertices = new List<float>(vertexCapacity);
        indices = new List<uint>((int)(indexCapacity * attributes.TotalAttributes()));
        this.attributes = attributes;
    }

    public void Clear()
    {
        vertexLookup.Clear();
        vertices.Clear();
        indices.Clear();
    }

    public void AddVertex(params float[] vertex)
    {
        if(vertex.Length != attributes.TotalAttributes())
        {
            throw new Exception("Wrong attributes for vertex");
        }
        AddVertexInternal(vertex);
    }

    private void AddVertexInternal(float[] vertex)
    {
        //If this vertex already exists, we just add it to our indices
        int hash = GenerateHash(vertex);
        if(vertexLookup.TryGetValue(hash, out var index))
        {
            indices.Add(index);
            return;
        }
        index = (uint)(vertices.Count / attributes.TotalAttributes());
        indices.Add(index);
        vertices.AddRange(vertex);
        vertexLookup.Add(hash, index);
    }

    public void AddVertex(int[] mapping, params float[] vertex)
    {
        if(mapping.Length != attributes.TotalAttributes())
        {
            throw new Exception("Mapping would not create correct attributes");
        }
        AddVertexInternal(ConvertVertex(vertex, mapping));
    }

    public VMesh ToMesh()
    {
        uint[] indicesArr = indices.ToArray();
        float[] vertciesArr = vertices.ToArray();
        return new VMesh(vertciesArr, indicesArr, attributes, null);
    }

    public bool IsTriangular()
    {
        return indices.Count % 3 != 0;
    }

    private static int GenerateHash(float[] vertex){
        HashCode hasher = new HashCode();
        foreach(float f in vertex){
           hasher.Add(BitConverter.SingleToInt32Bits(f));
        }
        return hasher.ToHashCode();
    }

    public static float[] ConvertVertex(float[] vertex, params int[] mapping){
        float[] ret = new float[mapping.Length];
        for(int i=0; i<mapping.Length; i++){
            int map = mapping[i];
            if(map != -1){
                ret[i] = vertex[map];
            }
        }
        return ret;
    }

    public static float[] ConvertVertex(int[] mapping, params float[] vertex)
    {
        return ConvertVertex(vertex, mapping);
    }
}
