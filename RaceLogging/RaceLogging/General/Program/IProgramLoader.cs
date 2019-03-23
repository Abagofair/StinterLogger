using RaceLogging.General.Program.Config;

namespace RaceLogging.General.Program
{
    public interface IProgramLoader
    {
        ProgramConfig LoadProgram(string path, string fileName);
    }
}
