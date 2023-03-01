namespace vmodel;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using StbImageSharp;
using StbImageWriteSharp;
public class VModelUtils{
    public static Dictionary<string, string> ParseListMap(string listContents, out List<VError>? errors){
        //TODO: write a better version that handles escape characters properly.
        //TODO: use something 'proper' like a YAML loader or something.
        errors = null;

        //first, we split each non-empty line into it's own string.
        string[] lines = listContents.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (string line in lines){
            //for each line, split it into the key and value
            string[] keyValue = line.Split(":", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if(keyValue.Length != 2){
                if(errors == null)errors = new List<VError>(4);
                errors.Add(new VError($"Can't have empty value: \"{line}\". Are you missing a colon or forgot to define it?"));
            } else {
                string value = keyValue[1];
                string key = keyValue[0];
                //then, add them to the dictionary.
                dict.TryAdd(key, value);
            }
        }
        return dict;
    }
    public static string CreateListMap(Dictionary<string, string> contents)
    {
        StringBuilder b = new StringBuilder();
        foreach(KeyValuePair<string, string> item in contents)
        {
            b.Append(item.Key + ": " + item.Value + "\n");
        }
        return b.ToString();
    }
    public static Tuple<string, string> SplitFolderAndFile(string path){
        int lastSlash = path.LastIndexOf('/');
        if(lastSlash == -1)return new Tuple<string, string>("", path);
        //we want to include the last slash in the folder, and exclude it in the path
        return new Tuple<string, string>(path.Substring(0, lastSlash+1), path.Substring(lastSlash+1));
    }

    public static VMesh? LoadMesh(string path, out Exception? error){
        byte[] file;
        try{
            file = File.ReadAllBytes(path);
            return LoadMesh(file, out error);
        }catch(Exception e){
            error = e;
            return null;
        }
    }

    public static VMesh? LoadMesh(byte[] file, out Exception? error){
        try{
            int index = 0; //index keeps track of what byte we are on
            //first we get the header info
            uint numVerts = BitConverter.ToUInt32(file, index);
            index+=4;
            if(numVerts == 0){
                error = new InvalidDataException("Invalid vmesh file: cannot have 0 vertices");
                return null;
            }
            uint numTris = BitConverter.ToUInt32(file, index);
            index+=4;
            uint numInds = numTris*3;
            if(numTris == 0){
                error = new InvalidDataException("Invalid vmesh file: cannot have 0 triangles");
                return null;
            }
            uint numAttributes = BitConverter.ToUInt32(file, index);
            index +=4;
            if(numAttributes == 0){
                error = new InvalidDataException("Invalid vmesh file: cannot have 0 attributes");
                return null;
            }
            //attributes
            EAttribute[] attributes = new EAttribute[numAttributes];
            uint totalAttributes = 0;
            for(int i=0; i<numAttributes; i++){
                //for each attribute, read it in.
                attributes[i] = (EAttribute)BitConverter.ToUInt32(file, index);
                totalAttributes += ((uint)attributes[i] % 5);
                index+=4;
            }
            //file size check
            if(12/*header*/ + totalAttributes*numVerts*4 + numInds*4 + numAttributes*4 > file.Length){
                error = new InvalidDataException("Invalid vmesh file: file too small");
                return null;
            }
            //vertices
            float[] vertices = new float[numVerts*totalAttributes];
            for(int i=0; i<numVerts*totalAttributes; i++){
                vertices[i] = BitConverter.ToSingle(file, index);
                index+=4;
            }
            //indices
            uint[] indices = new uint[numInds];
            for(int i=0; i<numInds; i++){
                indices[i] = BitConverter.ToUInt32(file, index);
                index+=4;
            }
            //removable triangles, if they exist:
            byte[]? removableTris = null;
            if(12/*header*/ + totalAttributes*numVerts*4 + numInds*4 + numAttributes*4 > file.Length){
                removableTris = new byte[numTris];
                for(int i=0; i<numTris; i++){
                    removableTris[i] = file[index];
                    index++;
                }
            }
            //Construct the mesh with the data
            error = null;
            return new VMesh(vertices, indices, new Attributes(attributes), removableTris);
        }catch(Exception e){
            error = e;
            return null;
        }
    }

    public static VModel? LoadModel(string path, out List<VError>? errors){
        errors = null;
        string file;
        try{
            file = File.ReadAllText(path);
            return ParseLoadModel(file, out errors, path);
        }catch(Exception e){
            if(errors == null)errors = new List<VError>(4);
            errors.Add(new VError(e));
            return null;
        }
    }

    public static VModel? ParseLoadModel(string file, out List<VError>? errors, string path){
        errors = null;
        try{
            //parse the VMF file to get the elements
            Dictionary<string, string> vmf = ParseListMap(file, out errors);
            //Get the important values
            string? typeStr = null;
            string? meshStr = null;
            string? textureStr = null;
            string? opaqueStr = null;
            if(!vmf.TryGetValue("type", out typeStr)){
                if(errors == null) errors = new List<VError>(4);
                errors.Add(new VError("type parameter not specified in \"" + path + "\""));
                return null;
            }
            if(!vmf.TryGetValue("mesh", out meshStr)){
                if(errors == null) errors = new List<VError>(4);
                errors.Add(new VError("mesh parameter not specified in \"" + path + "\""));
                return null;
            }
            byte? opaque = null;
            if(typeStr.Equals("block") && !vmf.TryGetValue("opaque", out opaqueStr)){
                if(errors == null) errors = new List<VError>(4);
                errors.Add(new VError("opaque parameter not specified in \"" + path + "\", assuming 0"));
                opaque = 0;
            }
            if(!vmf.TryGetValue("texture", out textureStr)){
                if(errors == null) errors = new List<VError>(4);
                errors.Add(new VError("texture parameter not specified in \"" + path + "\""));
                return null;
            }
            //Now we get the actual data
            if(typeStr.Equals("block") && opaque is null){
                if(!byte.TryParse(opaqueStr, out var opaques)){
                    if(errors == null) errors = new List<VError>(4);
                    errors.Add(new VError("unable to parse blocks parameter in \"" + path + "\", assuming 0"));
                    opaques = 0;
                }
                opaque = opaques;
            }
            var pathSep = SplitFolderAndFile(path);
            VMesh? mesh = LoadMesh(File.ReadAllBytes(pathSep.Item1 + meshStr), out var exception);
            if(exception != null){
                if(errors==null)errors = new List<VError>(4);
                errors.Add(new VError(exception));
                return null;
            }
            if(mesh == null){
                throw new Exception("INVALID STATE: Mesh from LoadMesh shouldn't be null here under any circumstances!!");
            }
            ImageResult texture = ImageResult.FromMemory(File.ReadAllBytes(pathSep.Item1 + textureStr));
            //Finally, we place all the stuff into a model and return itt
            return new VModel(mesh.Value, texture, opaque);
        }catch(Exception e){
            if(errors == null)errors = new List<VError>(4);
            errors.Add(new VError(e));
            return null;
        }
    }
    //Serialization time!
    public static List<VError>? SaveModel(VModel m, string basePath, string meshPath, string imagePath, string vmfPath)
    {
        basePath += "/";
        try{
            //Save the mesh
            var totalMeshPath = basePath + meshPath;
            try{
                File.Delete(totalMeshPath);
            } catch(Exception){}
            FileStream stream = new FileStream(totalMeshPath, FileMode.CreateNew);
            var meshErrors = SerializeMesh(m.mesh, stream);
            stream.Flush();
            stream.Dispose();
            //Save the image
            var totalImagePath = basePath + imagePath;
            try{
                File.Delete(totalImagePath);
            } catch(Exception){}
            stream = new FileStream(totalImagePath, FileMode.CreateNew);
            ImageWriter w = new ImageWriter();
            w.WritePng(m.texture.Data, m.texture.Width, m.texture.Height, (StbImageWriteSharp.ColorComponents)m.texture.Comp, stream);
            stream.Flush();
            stream.Dispose();
            //save the actual vmf file.
            Dictionary<string, string> things = new Dictionary<string, string>();
            things["type"] = "entity";
            if(m.opaqueFaces is not null)
            {
                things["type"] = "block";
                things["opaque"] = m.opaqueFaces.Value.ToString();
            }
            things["mesh"] = Path.GetRelativePath(vmfPath, meshPath);
            things["texture"] = Path.GetRelativePath(vmfPath, imagePath);
            File.WriteAllText(basePath + vmfPath, CreateListMap(things));
            return meshErrors;
        } catch(Exception e)
        {
            var er = new List<VError>();
            er.Add(new VError(e));
            return er;
        }
    }
    public static List<VError>? SerializeMesh(VMesh mesh, Stream stream)
    {
        List<VError>? errors = null;
        //Figure out how much space we are actually going to need.
        uint length;
        {
            const uint staticBytes = 12; //the minimum bytes if a vmesh file - the first 12 bytes are always required.
            uint attributeBytes = (uint)mesh.attributes.Length * 4;
            uint verticesBytes  = (uint)mesh.vertices  .Length * 4;
            uint indicesBytes   = (uint)mesh.indices   .Length * 4;
            length = staticBytes + attributeBytes + verticesBytes + indicesBytes;
            if(mesh.triangleToFaces is not null)
            {
                length += (uint)mesh.triangleToFaces.Length;
            }
        }
        uint totalAttributes = mesh.attributes.TotalAttributes();
        if(!stream.CanWrite)
        {
            if(errors is null)errors = new List<VError>();
            VError v = new VError(null, "Unwritable stream", VErrorType.unknown);
            errors.Add(v);
        }
        //Now we need to actually write the data.
        // Generate header values
        uint V = (uint)mesh.vertices.Length / totalAttributes;
        uint I = (uint)mesh.indices.Length / 3;
        uint A = (uint)mesh.attributes.Length;
        //write the header
        var writer = new BinaryWriter(stream);
        writer.Write(V);
        writer.Write(I);
        writer.Write(A);
        foreach(EAttribute a in mesh.attributes)
        {
            writer.Write((uint)a);
        }
        foreach(float f in mesh.vertices)
        {
            writer.Write(f);
        }
        foreach(uint i in mesh.indices)
        {
            writer.Write(i);
        }
        if(mesh.triangleToFaces is not null)
        {
            foreach(byte b in mesh.triangleToFaces)
            {
                writer.Write(b);
            }
        }
        return errors;
    }
}