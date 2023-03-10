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

// public sealed class MeshBuilder{
//     private readonly Dictionary<int /*hash*/, Vertex> _vertexLookup;
//     private readonly List<Vertex> _vertices;
//     private readonly List<uint> _indices;

//     public MeshBuilder(){
//         _vertexLookup = new Dictionary<int, Vertex>();
//         _vertices = new List<Vertex>();
//         _indices = new List<uint>();
//     }

//     public MeshBuilder(int vertexCapacity, int indexCapacity){
//         _vertexLookup = new Dictionary<int, Vertex>(vertexCapacity);
//         _vertices = new List<Vertex>(vertexCapacity);
//         _indices = new List<uint>(indexCapacity);
//     }

//     public void Clear()
//     {
//         _vertexLookup.Clear();
//         _indices.Clear();
//         _vertices.Clear();
//     }
//     public void AddVertex(params float[] vert){
//         int hash = Vertex.GenerateHash(vert);

//         if(_vertexLookup.TryGetValue(hash, out var oldVert)){
//             _indices.Add(oldVert.ind);
//             return;
//         }
//         Vertex vertex = new Vertex(vert, (uint)_vertices.Count);
//         _vertexLookup.Add(hash, vertex);
//         _vertices.Add(vertex);
//         _indices.Add(vertex.ind);

//     }

//     public void AddVertex(int[] mapping, params float[] vert){
//         vert = ConvertVertex(vert, mapping);
//         AddVertex(vert);
//     }
//     public VMesh ToMesh(Attributes attributes){
//         List<float> vertices = new List<float>(_vertices.Count);
//         for(int i=0; i<_vertices.Count; i++){
//             vertices.AddRange(_vertices[i].vert);
//         }
//         #if DEBUG
//         if(_indices.Count % 3 != 0)throw new Exception("Bro these vertices arent triangular");
//         #endif
//         return new VMesh(vertices.ToArray(), _indices.ToArray(), attributes, null);
//     }

//     public int indicesCount()
//     {
//         return _indices.Count;
//     }

//     [System.Diagnostics.Conditional("DEBUG")]
//     public void checkIndicesOrDie()
//     {
//         if(_indices.Count % 3 != 0) throw new Exception("The indices are not triangular!");
//     }
//     public static float[] ConvertVertex(float[] vertex, params int[] mapping){
//         float[] ret = new float[mapping.Length];
//         for(int i=0; i<mapping.Length; i++){
//             int map = mapping[i];
//             if(map != -1){
//                 ret[i] = vertex[map];
//             }
//         }
//         return ret;
//     }

//     struct Vertex{
//         readonly public float[] vert; //the actual vertex data
//         readonly public uint ind; //the index where this vertex can be found

//         public Vertex(float[] vertex, uint index){
//             vert = vertex;
//             ind = index;
//         }
//         public static int GenerateHash(float[] vertex){
//             HashCode hasher = new HashCode();
//             foreach(float f in vertex){
//                 hasher.Add(BitConverter.SingleToInt32Bits(f));
//             }
//             return hasher.ToHashCode();
//         }
//     }
// }
