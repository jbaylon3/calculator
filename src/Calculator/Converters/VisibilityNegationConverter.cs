// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.UI.Xaml;

namespace CalculatorApp
{
    namespace Common
    {
        /// <summary>
        /// Value converter that translates Visible to Collapsed and vice versa
        /// </summary>
        [Windows.Foundation.Metadata.WebHostHidden]
        public sealed class VisibilityNegationConverter : Microsoft.UI.Xaml.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                var boxedVisibility = (value as Visibility?);
                if (boxedVisibility != null && boxedVisibility.Value == Visibility.Collapsed)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                return Convert(value, targetType, parameter, language);
            }
        }
    }
}
