using RaceLogging.General.Entities;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace RaceLogging.General.Program
{
    public interface IProgramWriter
    {
        Task<byte[]> WriteProgramToByteArrayAsync(SimProgram program);

        Task<byte[]> WriteProgramToCompressedByteArrayAsync(SimProgram program, CompressionLevel compressionLevel);

        Task<string> WriteProgramToStringAsync(SimProgram program);

        string WriteProgramToString(SimProgram program);

        byte[] WriteProgramToByteArray(SimProgram simProgram);

        byte[] WriteProgramToCompressedByteArray(SimProgram simProgram, CompressionLevel compressionLevel);
    }
}
