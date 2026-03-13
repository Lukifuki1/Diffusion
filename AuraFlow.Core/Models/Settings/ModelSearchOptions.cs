using AuraFlow.Core.Models.Api;

namespace AuraFlow.Core.Models.Settings;

public record ModelSearchOptions(CivitPeriod SelectedPeriod, CivitSortMode SortMode, CivitModelType SelectedModelType, string SelectedBaseModelType);
