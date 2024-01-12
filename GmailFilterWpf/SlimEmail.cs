using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GmailFilterWpf;

public class SlimEmail : INotifyPropertyChanged
{
    private string _deleteState;
    public string Id { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public DateTime Date { get; set; }
    public string ThreadId { get; set; }

    public string DeleteState
    {
        get => _deleteState;
        set
        {
            if (value == _deleteState) return;
            _deleteState = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}