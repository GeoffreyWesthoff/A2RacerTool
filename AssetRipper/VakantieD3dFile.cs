/* MIT License
   
   Copyright (c) 2025 Joppe Schoenmaker, Geoffrey Westhoff
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
*/

// Massive thanks to Joppe Schoenmaker (ThuverX) for figuring out the d3d format used by Vakantie Racer/A2 Racer III
namespace AssetRipper;

public class VakantieD3dFile(string filename) : D3dFile(filename)
{

    public class Vector3
    {
        public float X, Y, Z;
    }
    
    public new class Face
    {
        public short idx0, idx1, idx2, idx3;
        public byte Z;
    }
    
    public new class Vertex
    {
        public Vector3 Pos;
        public Vector3 Normal;
        public float U, V;
        public Vector3 Unk1;
        public float Unk2;
        public byte Unk3;
    }
    
    private int _numVertices;
    private int _numFaces;
    
    private readonly List<Vertex> _vertices = [];
    private readonly List<Face> _faces = [];
    
    public new string GetFilename()
    {
        return filename;
    }
    
    public new int GetNumVertices()
    {
        return _numVertices;
    }
    
    public new int GetNumFaces()
    {
        return _numFaces;
    }
    
    public new List<Vertex> GetVertices()
    {
        return _vertices;
    }
    
    public new List<Face> GetFaces()
    {
        return _faces;
    }
    
    
 public new VakantieD3dFile Read(BinaryReader reader)
    {

        reader.BaseStream.Position = 0x04;
        _numVertices = reader.ReadInt32();
        _numFaces = reader.ReadInt32();
        
        for (var i = 0; i < _numVertices; i++) {
            Vertex vertex = new()
            {
                Pos = new Vector3
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle()
                },
                Normal = new Vector3
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle()
                }
                ,
                U = reader.ReadSingle(),
                V = 1f - reader.ReadSingle(),
                Unk1 = new Vector3
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle()
                },
                Unk2 = reader.ReadSingle(),
                Unk3 = reader.ReadByte()
            };

            _vertices.Add(vertex);
        }

        for (var i = 0; i < _numFaces; i++) {
            Face face = new()
            {
                idx0 = reader.ReadInt16(),
                idx1 = reader.ReadInt16(),
                idx2 = reader.ReadInt16(),
                idx3 = reader.ReadInt16(),
                Z = reader.ReadByte()
            };

            _faces.Add(face);
        }

        return this;
    }

    private string FormatMax(float value) {
        return value.ToString("0.000000");
    }

    public new void WriteToObj(string filepath) {
        using StreamWriter writer = new(filepath);

        var outputname = Path.GetFileName(filepath).Replace(".obj", "");

        writer.WriteLine("# " + filepath);
        writer.WriteLine("o " + outputname);

        foreach (var vertex in _vertices) {
            writer.WriteLine("v " + FormatMax(vertex.Pos.X) + " " + FormatMax(vertex.Pos.Y) + " " + FormatMax(vertex.Pos.Z));
        }

        foreach (var vertex in _vertices)
        {
            writer.WriteLine("vn " + FormatMax(vertex.Normal.X) + " " + FormatMax(vertex.Normal.Y) + " " + FormatMax(vertex.Normal.Z));
        }

        foreach (var vertex in _vertices) {
            writer.WriteLine("vt " + FormatMax(vertex.U) + " " + FormatMax(vertex.V));
        }

        writer.WriteLine("s 0");

        foreach (var triangle in _faces) {
            writer.WriteLine("f " + (triangle.idx0 + 1) + "/" + (triangle.idx0 + 1) + " " + (triangle.idx1 + 1) + "/" + (triangle.idx1 + 1) + " " + (triangle.idx2 + 1) + "/" + (triangle.idx2 + 1));
        }
        
        writer.Close();
    }}