using WorkClosure.Helpers;

namespace WorkClosure.Models;

public sealed class ProjectRecord : ObservableObject
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private bool _isActive = true;
    private DateTimeOffset _createdAt = DateTimeOffset.Now;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
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

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }
}
