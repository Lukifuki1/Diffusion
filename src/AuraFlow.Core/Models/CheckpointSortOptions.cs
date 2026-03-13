using System.ComponentModel;

namespace AuraFlow.Core.Models;

public class CheckpointSortOptions
{
    public CheckpointSortMode SortMode { get; set; } = CheckpointSortMode.SharedFolderType;
    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Descending;
    public bool SortConnectedModelsFirst { get; set; } = true;
}
