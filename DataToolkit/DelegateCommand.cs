using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataToolkit
{
    public class DelegateCommand: ICommand
    {
        public DelegateCommand(Action doSomething): base()
        {
            _do = (obj) => doSomething();
            _enabled = true;
        }
        public DelegateCommand(Action<object> doSomething): base()
        {
            _do = doSomething;
            _enabled = true;
        }
        Action<object> _do;

        public bool CanExecute(object parameter)
        {
            return _enabled;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_do != null)
                _do(parameter);
        }
        private bool _enabled;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}
