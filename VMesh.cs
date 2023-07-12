namespace vmodel;

using System;
//stores the deserialized data of a vmesh or vbmesh.
public struct VMesh{
    //members:
    public float[] vertices;
    public uint[] indices;
    public Attributes attributes;
    public byte[]? triangleToFaces;

    public VMesh(float[] vertices, uint[] indices, Attributes attributes, byte[]? triangleToFaces){
        this.vertices = vertices;
        this.indices = indices;
        this.attributes = attributes;
        this.triangleToFaces = triangleToFaces;
    }

    public Span<float> GetVertex(uint index)
    {
        uint totalAttrib = attributes.TotalAttributes();
        return new Span<float>(vertices, (int)(index*totalAttrib), (int)totalAttrib);
    }
}