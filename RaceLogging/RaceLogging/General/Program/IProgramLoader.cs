using StinterLogger.RaceLogging.General.Program.Config;

namespace StinterLogger.RaceLogging.General.Program
{
    public interface IProgramLoader
    {
        ProgramConfig LoadLocalProgram(string programName);
    }
}
