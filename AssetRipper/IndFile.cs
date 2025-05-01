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

// This code is based in part on work by Joppe Schoenmaker (ThuverX) <https://github.com/ThuverX/a2racer2>
using System.Text;

namespace AssetRipper;

public class IndFile
{
    
    private List<IndEntry> entries = [];
    
    public List<IndEntry> GetEntries()
    {
        return entries;
    }

    public class IndEntry
    {
        private string name = "";
        private int offset = 0;
        private int length = -1;
        
        public void SetLength(int l)
        {
            length = l;
        }
        
        public int GetLength()
        {
            return length;
        }
        
        public string GetName()
        {
            return name;
        }
        
        public int GetOffset()
        {
            return offset;
        }
        
        public IndEntry Read(BinaryReader reader)
        {
            var start = reader.BaseStream.Position;
            name = Encoding.UTF8.GetString(reader.ReadBytes(0x14));
            name = name.Split("\0")[0];
            reader.BaseStream.Position = start + 0x14;
            offset = reader.ReadInt32();
            return this;
        }
    }
    private ushort numEntries;
    
    public IndFile Read(BinaryReader reader)
    {
        numEntries = reader.ReadUInt16();
        for (var i = 0; i < numEntries; i++)
        {
            IndEntry entry = new();
            entry.Read(reader);
            if (i > 0)
            {
                var prevEntry = entries[i - 1];
                prevEntry.SetLength(entry.GetOffset() - prevEntry.GetOffset());
            }

            entries.Add(entry);
        }
        return this;
    }
    
    public void WriteToObj(string filepath)
    {
        using var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        foreach (var entry in entries)
        {
            writer.WriteLine($"Entry: {entry.GetName()}; Offset: {entry.GetOffset()}; Length: {entry.GetLength()}");
        }
    }
}