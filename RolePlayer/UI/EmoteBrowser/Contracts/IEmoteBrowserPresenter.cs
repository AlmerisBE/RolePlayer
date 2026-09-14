namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.Core.Emotes.Models;
using System;
using System.Collections.Generic;

public interface IEmoteBrowserPresenter : IDisposable {
    bool IsLoading { get; }
    string SearchQuery { get; set; }
    IReadOnlyList<string> AvailableCategories { get; }
    IReadOnlyDictionary<string, IReadOnlyList<EnrichedEmote>> GroupedEmotes { get; }

    void Initialize();
    void Refresh();
    void ApplyFilters();
    void SetSort(int columnIndex, bool descending);
}