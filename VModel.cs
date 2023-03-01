namespace vmodel;

using StbImageSharp;
public struct VModel{
    public VMesh mesh;
    public ImageResult texture;

    public byte? opaqueFaces;
    public VModel(VMesh mesh, ImageResult texture, byte? opaqueFaces){
        this.mesh = mesh;
        this.texture = texture;
        this.opaqueFaces = opaqueFaces;
    }
}
