﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using dotGit.Objects.Storage;

namespace dotGit.Objects
{
  public class Zlib
  {
    private static readonly int bufferLength = 4096;

    /// <summary>
    /// Returns a MemoryStream with the 'inflated' contents of the file specified in the <paramref name="path"/> parameter.
    /// </summary>
    /// <param name="path">Full path to the deflated file</param>
    /// <returns>Decompressed (inflated) MemoryStream</returns>
    public static MemoryStream Decompress(string path)
    {
      byte[] buffer = new byte[bufferLength];
      int size;

      MemoryStream output = new MemoryStream();
      using (FileStream fs = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
      {
        // MS, dare to be different ;-)
        // Skip the first two bytes, see : http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=97064
        fs.ReadByte();
        fs.ReadByte();

        using (System.IO.Compression.DeflateStream inflaterStream = new System.IO.Compression.DeflateStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
        {
          while ((size = inflaterStream.Read(buffer, 0, buffer.Length)) > 0)
            output.Write(buffer, 0, size);
        }
      }
      return output;
    }

    /// <summary>
    /// Returns a byte-array with the 'inflated' contents of the input array.
    /// </summary>
    /// <param name="input">inflated byte-array</param>
    /// <returns>Decompressed (inflated) byte-array</returns>
    public static byte[] Decompress(byte[] input)
    {
      byte[] buffer = new byte[1024];
      int size;

      using (MemoryStream outputStream = new MemoryStream())
      {
        using (MemoryStream inputStream = new MemoryStream(input))
        {
          // MS, dare to be different ;-)
          // Skip the first two bytes, see : http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=97064
          inputStream.ReadByte();
          inputStream.ReadByte();
          
          using (System.IO.Compression.DeflateStream inflaterStream = new System.IO.Compression.DeflateStream(inputStream, System.IO.Compression.CompressionMode.Decompress, false))
          {
            while ((size = inflaterStream.Read(buffer, 0, buffer.Length)) > 0)
              outputStream.Write(buffer, 0, size);
          }
        }
        return outputStream.ToArray();
      }
    }

    /// <summary>
    /// Returns a MemoryStream with the 'inflated' contents of the object in the GitObjectReader starting at the current stream position and with a decompressed size of <param name="destLength">destLength</param>
    /// </summary>
    /// <param name="input">An open GitObjectReader object pointing to a file system object in the Git Repository</param>
    /// <param name="destLength">Inflate the contents in the stream until the decompressed array reaches this size</param>
    /// <returns>Inflated contents in a MemoryStream</returns>
    public static MemoryStream Decompress(GitObjectReader input, long destLength)
    {

      //MemoryStream output = new MemoryStream();
      //Inflater inflater = new Inflater();

      //byte[] buffer = new byte[destLength];

      //while (output.Length < destLength)
      //{
      //  if (inflater.IsNeedingInput)
      //    inflater.SetInput(input.ReadBytes(bufferLength));

      //  int outLength = inflater.Inflate(buffer);

      //  if (outLength > 0)
      //    output.Write(buffer, 0, outLength);
      //  else
      //  {
      //    if (inflater.IsFinished)
      //      break;
      //  }
      //}

      //// rewind stream to end of content (buffer overhead)
      //input.Position -= inflater.RemainingInput;


      //return output;
      throw new NotImplementedException();
    }

  }
}