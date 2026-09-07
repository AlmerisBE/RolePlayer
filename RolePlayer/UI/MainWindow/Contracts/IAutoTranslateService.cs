namespace RolePlayer.UI.MainWindow.Contracts;

using RolePlayer.UI.MainWindow.Models;
using System.Collections.Generic;

public interface IAutoTranslateService {
    IEnumerable<AutoTranslateResult> Search(string query);
}