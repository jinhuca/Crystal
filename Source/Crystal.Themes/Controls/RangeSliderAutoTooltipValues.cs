// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace Crystal.Themes.Controls
{
  public class RangeSliderAutoTooltipValues : INotifyPropertyChanged
    {
        private string? lowerValue;

        /// <summary>
        /// Gets the lower value of the range selection.
        /// </summary>
        public string? LowerValue
        {
            get => lowerValue;
            set
            {
                if (value == lowerValue)
                {
                    return;
                }

                lowerValue = value;
                OnPropertyChanged();
            }
        }

        private string? upperValue;

        /// <summary>
        /// Gets the upper value of the range selection.
        /// </summary>
        public string? UpperValue
        {
            get => upperValue;
            set
            {
                if (value == upperValue)
                {
                    return;
                }

                upperValue = value;
                OnPropertyChanged();
            }
        }

        internal RangeSliderAutoTooltipValues(RangeSlider rangeSlider)
        {
            UpdateValues(rangeSlider);
        }

        internal void UpdateValues(RangeSlider rangeSlider)
        {
            LowerValue = rangeSlider.GetToolTipNumber(rangeSlider.LowerValue);
            UpperValue = rangeSlider.GetToolTipNumber(rangeSlider.UpperValue);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{LowerValue} - {UpperValue}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}