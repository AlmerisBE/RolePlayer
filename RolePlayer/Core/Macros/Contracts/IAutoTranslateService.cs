namespace RolePlayer.Core.Macros.Contracts;

using RolePlayer.Core.Macros.Models;
using System.Collections.Generic;

public interface IAutoTranslateService {
    IEnumerable<AutoTranslateResult> Search(string query);
}