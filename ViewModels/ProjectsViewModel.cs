using Microsoft.UI.Xaml;
using WorkClosure.Helpers;
using WorkClosure.Models;
using WorkClosure.Services;

namespace WorkClosure.ViewModels;

public sealed class ProjectsViewModel : ObservableObject
{
    private readonly AppState _state = ((App)Application.Current).State;
    private IReadOnlyList<ProjectProgressSummary> _projectSummaries = [];
    private ProjectProgressSummary? _selectedSummary;
    private Guid _editingProjectId;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private bool _isActive = true;

    public ProjectsViewModel()
    {
        SaveProjectCommand = new RelayCommand(async () => await SaveProjectAsync(), CanSaveProject);
        NewProjectCommand = new RelayCommand(NewProject);
        DeleteProjectCommand = new RelayCommand(async () => await DeleteSelectedProjectAsync(), CanDeleteProject);
        _state.StateChanged += (_, _) => Refresh();
        Refresh();
        NewProject();
    }

    public IReadOnlyList<ProjectProgressSummary> ProjectSummaries
    {
        get => _projectSummaries;
        private set => SetProperty(ref _projectSummaries, value);
    }

    public ProjectProgressSummary? SelectedSummary
    {
        get => _selectedSummary;
        set
        {
            if (!SetProperty(ref _selectedSummary, value))
            {
                return;
            }

            if (value is null || !value.ProjectId.HasValue)
            {
                _editingProjectId = Guid.Empty;
                Name = string.Empty;
                Description = string.Empty;
                IsActive = true;
                OnPropertyChanged(nameof(EditorTitle));
                OnPropertyChanged(nameof(SaveButtonText));
                DeleteProjectCommand.NotifyCanExecuteChanged();
                return;
            }

            var project = _state.Projects.FirstOrDefault(item => item.Id == value.ProjectId.Value);
            if (project is not null)
            {
                _editingProjectId = project.Id;
                Name = project.Name;
                Description = project.Description;
                IsActive = project.IsActive;
                OnPropertyChanged(nameof(EditorTitle));
                OnPropertyChanged(nameof(SaveButtonText));
            }

            DeleteProjectCommand.NotifyCanExecuteChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                SaveProjectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public RelayCommand SaveProjectCommand { get; }
    public RelayCommand NewProjectCommand { get; }
    public RelayCommand DeleteProjectCommand { get; }

    public string EditorTitle => _editingProjectId == Guid.Empty ? "新建项目" : "编辑项目";

    public string SaveButtonText => _editingProjectId == Guid.Empty ? "创建项目" : "保存修改";

    public void Refresh()
    {
        ProjectSummaries = AnalyticsService.BuildProjectSummaries(_state.Projects, _state.Tasks);
    }

    private bool CanSaveProject()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    private bool CanDeleteProject()
    {
        return _editingProjectId != Guid.Empty;
    }

    private async Task SaveProjectAsync()
    {
        var existingCreatedAt = _state.Projects.FirstOrDefault(item => item.Id == _editingProjectId)?.CreatedAt ?? DateTimeOffset.Now;
        var project = new ProjectRecord
        {
            Id = _editingProjectId == Guid.Empty ? Guid.NewGuid() : _editingProjectId,
            Name = Name.Trim(),
            Description = Description.Trim(),
            IsActive = IsActive,
            CreatedAt = existingCreatedAt
        };

        await _state.SaveProjectAsync(project);
        Refresh();
        SelectedSummary = ProjectSummaries.FirstOrDefault(summary => summary.ProjectId == project.Id);
    }

    private async Task DeleteSelectedProjectAsync()
    {
        if (_editingProjectId == Guid.Empty)
        {
            return;
        }

        var deletingProjectId = _editingProjectId;
        NewProject();
        await _state.DeleteProjectAsync(deletingProjectId);
        Refresh();
    }

    private void NewProject()
    {
        _editingProjectId = Guid.Empty;
        Name = string.Empty;
        Description = string.Empty;
        IsActive = true;
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        SelectedSummary = null;
        DeleteProjectCommand.NotifyCanExecuteChanged();
    }
}
