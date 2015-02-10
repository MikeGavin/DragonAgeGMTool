using GalaSoft.MvvmLight;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.ViewModel
{
    public abstract class FlyoutViewModelBase : ViewModelBase
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the header text of the flyout instance.
        /// </summary>
        public string Header
        {
            get { return _header; }
            set
            {
                if (_header == value)
                    return;
                _header = value;
                RaisePropertyChanged(GetPropertyName(() => Header));
            }
        }

        /// <summary>
        /// Gets or sets the position of the flyout instance.
        /// </summary>
        public Position Position
        {
            get { return _position; }
            set
            {
                if (_position == value)
                    return;
                _position = value;
                RaisePropertyChanged("Position");
            }
        }

        /// <summary>
        /// Gets or sets if the flyout insatnce is visible or collapsed.
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value)
                    return;
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }

        /// <summary>
        /// Gets or sets the theme of the flyout instance.
        /// 
        public FlyoutTheme Theme
        {
            get { return _theme; }
            set
            {
                if (_theme == value)
                    return;
                _theme = value;
                RaisePropertyChanged("Theme");
            }
        }

        #endregion

        #region protected members.

        protected string _header;
        protected Position _position;
        protected bool _isOpen;
        protected FlyoutTheme _theme;

        #endregion

    }
}
