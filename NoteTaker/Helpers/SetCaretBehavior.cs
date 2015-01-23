﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scrivener.Helpers
{
    public class SetCaretIndexBehavior : System.Windows.Interactivity.Behavior<TextBox>
    {
        public static readonly DependencyProperty CaretPositionProperty;
        private bool _internalChange;

        static SetCaretIndexBehavior()
        {

            CaretPositionProperty = DependencyProperty.Register("CaretPosition", typeof(int), typeof(SetCaretIndexBehavior), new PropertyMetadata(0, OnCaretPositionChanged));
        }

        public int CaretPosition
        {
            get { return Convert.ToInt32(GetValue(CaretPositionProperty)); }
            set { SetValue(CaretPositionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += OnKeyUp;
        }

        private static void OnCaretPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (SetCaretIndexBehavior)d;
            if (!behavior._internalChange)
            {
                behavior.AssociatedObject.CaretIndex = Convert.ToInt32(e.NewValue);
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _internalChange = true;
            CaretPosition = AssociatedObject.CaretIndex;
            _internalChange = false;
        }
    }
}
