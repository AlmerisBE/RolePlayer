namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;

public interface IGameLibraryService {
    string LibraryDirectory { get; }
    IEnumerable<GameDefinition> GetAvailableGames();
    void SaveGame(GameDefinition game);
    void DeleteGame(string fileName);
    void ReloadLibrary();
    void OpenLibraryDirectory();
}