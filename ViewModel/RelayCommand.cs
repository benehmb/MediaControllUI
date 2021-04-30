using System;
using System.Windows.Input;

namespace MediaControll
{
    /// <summary>
    /// A basic command that runs an Action
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Private Members

        /// <summary>
        /// the action to run
        /// </summary>
        private Action mAcrion;

        #endregion

        #region Public Events

        /// <summary>
        /// The event that is fired when the <see cref="CanExecute(object)"/> 
        /// </summary>
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="action">the action to run</param>
        public RelayCommand(Action action)
        {
            mAcrion = action;

        }

        #endregion

        #region Command Methods

        /// <summary>
        /// A relay command can always be executed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Executes the commands Action
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            mAcrion();
        }

        #endregion
    }
}
