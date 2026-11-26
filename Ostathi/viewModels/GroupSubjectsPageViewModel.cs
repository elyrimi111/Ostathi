using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class GroupSubjectRowItem
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int SubjectId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
}

public partial class GroupSubjectsPageViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext = new();
    private readonly GroupSubjectService _groupSubjectService = new();

    private List<GroupSubjectRowItem> _allItems = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    public ObservableCollection<GroupSubjectRowItem> FilteredItems { get; } = new();

    public bool IsNotBusy => !IsBusy;
    public bool IsEmpty => !IsBusy && FilteredItems.Count == 0;

    public ICommand AddGroupSubjectCommand { get; }
    public ICommand EditGroupSubjectCommand { get; }
    public ICommand DeleteGroupSubjectCommand { get; }
    public ICommand RefreshCommand { get; }

    public GroupSubjectsPageViewModel()
    {
        AddGroupSubjectCommand = new AsyncRelayCommand(OnAddGroupSubjectAsync);
        EditGroupSubjectCommand = new AsyncRelayCommand<GroupSubjectRowItem>(OnEditGroupSubjectAsync);
        DeleteGroupSubjectCommand = new AsyncRelayCommand<GroupSubjectRowItem>(OnDeleteGroupSubjectAsync);
        RefreshCommand = new AsyncRelayCommand(OnRefreshAsync);
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    // Load all group-subject assignments with related Group and Subject entities
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var list = await _dbContext.GroupSubjects
                .AsNoTracking()
                .Include(gs => gs.Group)
                .Include(gs => gs.Subject)
                .OrderByDescending(gs => gs.Id)
                .Select(gs => new GroupSubjectRowItem
                {
                    Id = gs.Id,
                    GroupId = gs.GroupId,
                    SubjectId = gs.SubjectId,
                    GroupName = gs.Group != null ? gs.Group.Name : $"مجموعة #{gs.GroupId}",
                    SubjectName = gs.Subject != null ? gs.Subject.Name : $"مادة #{gs.SubjectId}",
                    Status = gs.Status,
                    Notes = gs.Notes
                })
                .ToListAsync();

            _allItems = list;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر تحميل مواد المجموعات: {ex.Message}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task OnRefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            await LoadDataAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void ApplyFilter()
    {
        string query = SearchQuery?.Trim() ?? string.Empty;

        var results = string.IsNullOrWhiteSpace(query)
            ? _allItems
            : _allItems.Where(x =>
                (!string.IsNullOrWhiteSpace(x.SubjectName) && x.SubjectName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.GroupName) && x.GroupName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.Status) && x.Status.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.Notes) && x.Notes.Contains(query, StringComparison.OrdinalIgnoreCase)));

        MainThread.BeginInvokeOnMainThread(() =>
        {
            FilteredItems.Clear();
            foreach (var item in results)
            {
                FilteredItems.Add(item);
            }
            OnPropertyChanged(nameof(IsEmpty));
        });
    }

    // Open AddGroupSubjectPage in Add mode
    private async Task OnAddGroupSubjectAsync()
    {
        if (Shell.Current == null) return;
        await Shell.Current.GoToAsync($"{nameof(AddGroupSubjectPage)}?GroupSubjectId=0");
    }

    // Open AddGroupSubjectPage in Edit mode with GroupSubjectId
    private async Task OnEditGroupSubjectAsync(GroupSubjectRowItem? item)
    {
        if (item == null || item.Id <= 0 || Shell.Current == null) return;
        await Shell.Current.GoToAsync($"{nameof(AddGroupSubjectPage)}?GroupSubjectId={item.Id}");
    }

    // Remove subject assignment from group
    private async Task OnDeleteGroupSubjectAsync(GroupSubjectRowItem? item)
    {
        if (item == null || Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            $"هل تريد إزالة مادة \"{item.SubjectName}\" من مجموعة \"{item.GroupName}\"؟",
            "حذف",
            "إلغاء");

        if (!confirm) return;

        bool removed = await _groupSubjectService.RemoveSubjectFromGroupAsync(item.Id);
        if (removed)
        {
            _allItems.RemoveAll(x => x.Id == item.Id);
            ApplyFilter();
            await Shell.Current.DisplayAlert("نجاح", "تمت إزالة المادة من المجموعة بنجاح", "موافق");
        }
        else
        {
            await Shell.Current.DisplayAlert("خطأ", "تعذر إزالة المادة من المجموعة", "موافق");
        }
    }
}
