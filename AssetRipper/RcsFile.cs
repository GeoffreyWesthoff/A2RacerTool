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

// This code is partially based on work by Joppe Schoenmaker (ThuverX) <https://github.com/ThuverX/a2racer2>
using System.Text;

namespace AssetRipper;

public class RcsFile(string filename)
{
   
   public static bool Create(string exportPath, string outPath)
   {
      using var fs = File.Open(Path.Combine(outPath, "rcs.img"), FileMode.Create);
      using var index = File.Open(Path.Combine(outPath, "rcs.ind"), FileMode.Create);
      using BinaryWriter indexStream = new(index);
      using BinaryWriter writeStream = new(fs);
      var files = Directory.EnumerateFiles(exportPath);

      var enumerable = files as string[] ?? files.ToArray();
      var numFiles = enumerable.Length;
      indexStream.Write((ushort)numFiles);
      
      
      foreach (var file in enumerable)
      {
         using FileStream fileStream = new(Path.Combine(exportPath, file), FileMode.Open, FileAccess.Read);
         using BinaryReader entryStream = new(fileStream);
         writeStream.Write(entryStream.ReadBytes((int)entryStream.BaseStream.Length));
         
         var entryName= string.Join("", Path.GetFileName(file).Split(Path.GetInvalidFileNameChars()));
         var entryOffset = (int)writeStream.BaseStream.Position - entryStream.BaseStream.Length;
         indexStream.Write(Encoding.UTF8.GetBytes(entryName));
         indexStream.BaseStream.Position += 20 - Encoding.UTF8.GetByteCount(entryName);
         indexStream.Write((uint)entryOffset);
      }

      return true;
   }
   
   public class RcsEntry(string name, int offset, byte[] data)
   {
      private string name = name;
      private int offset = offset;
      private byte[] data = data;
      
      public string GetName()
      {
         return name;
      }
      
      public int GetOffset()
      {
         return offset;
      }
      
      public byte[] GetData()
      {
         return data;
      }
   }

   private readonly string filename = filename;
   private byte[] data = [];

   public void Read(BinaryReader reader)
   {
      data = reader.ReadBytes((int)reader.BaseStream.Length);
   }

   public RcsEntry GetEntry(IndFile.IndEntry entry)
   {
      using var stream = new MemoryStream(data);
      using var reader = new BinaryReader(stream);
      
      reader.BaseStream.Position = entry.GetOffset();
      var length = entry.GetLength();
      if (length == -1)
      {
         entry.SetLength((int)reader.BaseStream.Length - entry.GetOffset());
      }
      return new RcsEntry(entry.GetName(), entry.GetOffset(), reader.ReadBytes(entry.GetLength()));
   }
}