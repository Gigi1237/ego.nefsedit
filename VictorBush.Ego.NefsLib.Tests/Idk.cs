using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.Tests
{
    public class DefalateResult
    {
        public int BytesRead { get; set; }
        public int ChunkSize { get; set; }
    }

    [TestClass]
    public class Idk
    {

        /// <summary>
        /// Takes data from an input file stream, compresses it, and writes it to the specified
        /// output file. File streams should already seek to the proper location before calling
        /// this function.
        /// </summary>
        /// <param name="infs">The input file stream to read from.</param>
        /// <param name="numBytes">Number of bytes to read in.</param>
        /// <param name="outfs">The output file stream to write compressed chunk to.</param>
        /// <returns>Number of bytes actually read from input file and the compressed size of the chunk that was written.</returns>
        public static async Task<DefalateResult> DeflateToFileAsync(Stream infs, int numBytes, Stream outfs)
        {
            DefalateResult result = new DefalateResult();

            // Read in the input data to compress
            var inData = new byte[numBytes];
            result.BytesRead = infs.Read(inData, 0, numBytes);
            result.ChunkSize = 0;

            // Deflate stream doesn't write properly directly to a FileStream when
            // doing this chunk business. So have to do this multi-stream setup.
            using (var inStream = new MemoryStream())
            using (var outStream = new MemoryStream())
            using (var deflateStream = new DeflateStream(outStream, CompressionMode.Compress))
            {
                // Read input chunk into memory stream
                await inStream.WriteAsync(inData, 0, result.BytesRead);
                inStream.Seek(0, SeekOrigin.Begin);

                // Compress the chunk with deflate stream
                await inStream.CopyToAsync(deflateStream, result.BytesRead);

                // Close deflate stream to finalize compression - breaking
                // the "using()" convention, but this is needed
                deflateStream.Close();

                // Write the compressed chunk to the output file
                var compressedData = outStream.ToArray();
                result.ChunkSize = (int)compressedData.Length;
                await outfs.WriteAsync(compressedData, 0, compressedData.Length);
            }

            return result;
        }

        [TestMethod]
        public void CompressFileAsync_1Chunk_FileCompressed()
        {
            var input = @"C:\Users\Victor\Desktop\nefswork\1 chunk\int_fr5.xml";
            var output = @"C:\Users\Victor\Desktop\nefswork\1 chunk\lol_lol.dat";
            var temp = @"C:\Users\Victor\Desktop\nefswork\1 chunk\temp\";

            using (var infile = File.OpenRead(input))
            using (var outfile = File.OpenWrite(output))
            {
                var result= DeflateToFileAsync(infile, 0x10000, outfile).Result;

            }

        }


        public void lol()
        {
            byte[] inputBlock = new byte[128];
            byte[] destBlock = new byte[256];

            var idxInner = 0;
            var idxOuter = 0;

            var wordSize = 2;  // word is 2 bytes here
            var numWordsInBlock = 64; // 128 bytes in block

            for (idxOuter = 0; idxOuter < numWordsInBlock; idxOuter += wordSize)
            {
                var workingVal = 0;

                for (idxInner = 0; idxInner < numWordsInBlock; idxInner += wordSize)
                {
                    var destIdx = idxInner + idxOuter;
                    var outerVal = BitConverter.ToUInt16(inputBlock, idxOuter);
                    var innerVal = BitConverter.ToUInt16(inputBlock, idxInner);
                    var oldDestVal = BitConverter.ToUInt16(destBlock, destIdx);

                    workingVal = workingVal + (outerVal * innerVal) + oldDestVal;

                    // Lower 2 bytes of working val goes to destination buffer
                    UInt16 newDestVal = (UInt16)(workingVal & 0x0000FFFF);
                    var newDestValBytes = BitConverter.GetBytes(newDestVal);
                    destBlock[destIdx] = newDestValBytes[0];
                    destBlock[destIdx + 1] = newDestValBytes[1];

                    // Upper 2 bytes of working val get re-used in working val
                    workingVal = workingVal >> 16;
                }
            }
        }

        [TestMethod]
        public void Haha()
        {
            Assert.IsTrue(false);
        }
    }
}
