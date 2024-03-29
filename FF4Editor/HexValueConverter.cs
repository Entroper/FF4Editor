﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace FF4Editor;

public class HexValueConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is byte byteValue)
			return byteValue.ToString("X2");

		return null;
	}

	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string stringValue)
			return byte.Parse(stringValue, NumberStyles.HexNumber);

		return null;
	}
}
