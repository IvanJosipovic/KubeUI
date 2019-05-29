using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using KubeUI.Schema;
using System.Runtime.CompilerServices;
using System;
using KubeUI.Core;

namespace KubeUI
{
    public interface IState
    {
        Dictionary<Type, Collection<object>> Data { get; set; }

        void ImportObject(string data);

        object GetCollection(Type type);
        object GetItem(Type type, int Id);
        int GetCount(Type type);
        int AddItem(Type type);
        void DeleteItem(Type type, int Id);

        bool IsValid(object item, Type type);
        bool IsValid<T>(T item);

        void SetUILevel(UILevel uILevel);
        UILevel GetUILevel();

        event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged([CallerMemberName] string propertyName = null);
    }
}