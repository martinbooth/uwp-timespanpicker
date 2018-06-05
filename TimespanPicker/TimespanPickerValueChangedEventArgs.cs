using System;

namespace TimespanPicker
{
    public sealed class TimespanPickerValueChangedEventArgs
    {
        public TimeSpan? NewTimespan { get; internal set; }

        public TimeSpan? OldTimespan { get; internal set; }
    }
}
