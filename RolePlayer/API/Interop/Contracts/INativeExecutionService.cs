namespace RolePlayer.API.Interop.Contracts;

public interface INativeExecutionService {
    void Execute(string commandOrMacroContent);
}