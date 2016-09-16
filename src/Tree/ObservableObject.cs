using System;
using System.ComponentModel;

namespace Tree {
    public class ObservableObject : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Set<T>(string propertyName, ref T backingField, T newValue, Action beforeChange = null, Action afterChange = null) {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("propertyName");

            if (backingField == null && newValue == null)
                return;

            if (backingField != null && backingField.Equals(newValue))
                return;

            beforeChange?.Invoke();

            backingField = newValue;

            OnPropertyChanged(propertyName);

            afterChange?.Invoke();
        }
    }
}
