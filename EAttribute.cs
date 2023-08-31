namespace vmodel;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
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

public readonly struct Attributes : IEnumerable, IEnumerable<EAttribute>
{
    private readonly EAttribute[] attributes;
    public Attributes(EAttribute[] attributes)
    {
        this.attributes = attributes;
    }
    public ReadOnlySpan<EAttribute> Attr {get => attributes.AsSpan();}

    public EAttribute this[int index]
    {
        get {
            return attributes[index];
        }
    }
    //Totals the number of floats that is required to store one vertex using a list of attributes
    public uint TotalAttributes()
    {
        uint totalAttributes = 0;
        for(int i=0; i<attributes.Length; i++){
            totalAttributes += ((uint)attributes[i] % 5);
        }
        return totalAttributes;
    }

    public int Length{get => attributes.Length;}

    //Yikes C# generics can get messy when they are done in the weird way they are.
    // I get that having non-generic versions of generic classes is useful,
    // But personally I would MUCH rather have a language feature
    // that allows generic classes to be used non-generically
    // than having non-generic base classes of every generic type.
    // People say Java generics are worse (and they are) but C# isn't actually that much better.
    IEnumerator IEnumerable.GetEnumerator()
    {
        return attributes.GetEnumerator();
    }

    IEnumerator<EAttribute> IEnumerable<EAttribute>.GetEnumerator()
    {
        return attributes.AsEnumerable<EAttribute>().GetEnumerator();
    }

    public override int GetHashCode()
    {
        //TODO: acctual hash code function
        return base.GetHashCode();
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is Attributes other)
        {
            return other == this;
        }
        if(obj is Attributes?)
        {
            var other0 = (Attributes?)obj;
            return other0.Value == this;
        }
        return false;
    }

    public static bool operator == (Attributes a, Attributes b)
    {
        return a.attributes.SequenceEqual(b.attributes);
    }

    public static bool operator != (Attributes a, Attributes b)
    {
        return !a.attributes.SequenceEqual(b.attributes);
    }

    public override string ToString()
    {
        return string.Join(", ", attributes);
    }
}