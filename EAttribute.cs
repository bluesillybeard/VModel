namespace vmodel;

public enum EAttribute{
    //generic/unreasoned types
    scalar=1,
    vec2 = 2,
    vec3 = 3,
    vec4 = 4,
    //purposeful attributes
    // Notice that ID mod 5 always matches the number of floats in the attribute
    textureCoords = 7,
    position = 8,
    rgbaColor = 9,
    normal= 13,
    rgbColor = 18,
}