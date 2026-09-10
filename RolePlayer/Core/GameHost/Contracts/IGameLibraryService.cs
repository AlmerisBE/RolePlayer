namespace RolePlayer.Core.GameHost.Contracts;

using RolePlayer.Core.GameHost.Models;
using System.Collections.Generic;

public interface IGameLibraryService {
    string LibraryDirectory { get; }
    IEnumerable<GameDefinition> GetAvailableGames();
    void SaveGame(GameDefinition game);
    void DeleteGame(string fileName);
    void ReloadLibrary();
    void OpenLibraryDirectory();
}