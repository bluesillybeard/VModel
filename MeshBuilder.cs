namespace vmodel;

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.InteropServices;

public sealed class MeshBuilder
{
    //takes a hash code as an input, and outputs the index of the vertex with that hash code.
    private readonly List<float> vertices;
    private readonly List<uint> indices;
    private readonly HashSet<int> vertexLookup; //To see if a vertex is already in the mesh somewhere
    public readonly Attributes attributes;
    public readonly uint totalAttributes;

    public MeshBuilder(Attributes attributes)
    {
        vertices = new List<float>();
        indices = new List<uint>();
        this.attributes = attributes;
        totalAttributes = attributes.TotalAttributes();
        vertexLookup = new HashSet<int>();
    }
    public MeshBuilder(Attributes attributes, int vertexCapacity, int indexCapacity)
    {
        vertices = new List<float>(vertexCapacity);
        indices = new List<uint>(indexCapacity);
        this.attributes = attributes;
        totalAttributes = attributes.TotalAttributes();
        vertexLookup = new HashSet<int>(vertexCapacity);
    }

    public void Clear()
    {
        vertices.Clear();
        indices.Clear();
    }

    public void AddVertex(params float[] vertex)
    {
        if(vertex.Length != totalAttributes)
        {
            throw new Exception("Wrong attributes for vertex");
        }
        AddVertexInternal(vertex);
    }

        public void AddVertex(ReadOnlySpan<float> vertex)
    {
        if(vertex.Length != totalAttributes)
        {
            throw new Exception("Wrong attributes for vertex");
        }
        AddVertexInternal(vertex);
    }

    private void AddVertexInternal(ReadOnlySpan<float> vertex)
    {
        //If this vertex already exists, we just add it to our indices
        if(FindVertex(vertex, out var index))
        {
            indices.Add(index);
            return;
        }
        index = (uint)(vertices.Count / totalAttributes);
        indices.Add(index);
        //vertices.AddRange(vertex);
        foreach(var val in vertex)
        {
            vertices.Add(val);
        }
    }

    private bool FindVertex(ReadOnlySpan<float> vertex, out uint index)
    {
        int hash = GenerateHash(vertex);
        //If no vertex has a matching hash code, then we already know there are none
        if(!vertexLookup.Contains(hash))
        {
            index = 0;
            return false;
        }
        vertexLookup.Add(hash);
        //Even if there is a matching hash code, that does not mean there is a matching vertex.
        // So, search for the vertex to find the actual match
        var numberOfVertices = vertices.Count / totalAttributes;
        var verticesSpan = CollectionsMarshal.AsSpan(vertices);
        for(uint i=0; i< numberOfVertices; i++)
        {
            //List.GetRange is slow, since it makes a copy.
            // So, slicing a span is a lot faster.
            var existingVertex = verticesSpan.Slice((int)(i*totalAttributes), (int)totalAttributes);
            if(existingVertex.SequenceEqual(vertex))
            {
                index = i;
                return true;
            }
        }
        index = 0;
        return false;
    }
    public void AddVertex(int[] mapping, params float[] vertex)
    {
        if(mapping.Length != totalAttributes)
        {
            throw new Exception("Mapping would not create correct attributes");
        }
        Span<float> converted = stackalloc float[mapping.Length];
        ConvertVertex(vertex, converted, mapping);
        AddVertexInternal(converted);
    }

    public void AddVertex(int[] mapping, ReadOnlySpan<float> vertex)
    {
        if(mapping.Length != totalAttributes)
        {
            throw new Exception("Mapping would not create correct attributes");
        }
        Span<float> converted = stackalloc float[mapping.Length];
        ConvertVertex(vertex, converted, mapping);
        AddVertexInternal(converted);
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

    private static int GenerateHash(ReadOnlySpan<float> vertex){
        HashCode hasher = new();
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

    public static void ConvertVertex(ReadOnlySpan<float> vertex, Span<float> destination, params int[] mapping){
        for(int i=0; i<mapping.Length; i++){
            int map = mapping[i];
            if(map != -1){
                destination[i] = vertex[map];
            }
        }
    }
}