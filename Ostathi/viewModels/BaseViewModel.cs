using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ostathi;

public class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;

    // Indicates whether the ViewModel is currently performing an operation,
    // such as fetching data from a database, to display a loading indicator.
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // Stores the title of the page associated with this ViewModel.
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    // Event that notifies the View when a property value changes.
    public event PropertyChangedEventHandler? PropertyChanged;

    // Raises the PropertyChanged event to notify the View about property changes.
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Updates the property value and notifies the View only if the value has changed.
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        // Check if the old value is equal to the new value.
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        // Update the backing field with the new value.
        storage = value;

        // Notify the View that the property value has changed.
        OnPropertyChanged(propertyName);

        return true;
    }
}