using StinterLogger.RaceLogging.General.Program.Config;

namespace StinterLogger.RaceLogging.General.Program
{
    public interface IProgramLoader
    {
        ProgramConfig LoadProgram(string path, string fileName);
    }
}
