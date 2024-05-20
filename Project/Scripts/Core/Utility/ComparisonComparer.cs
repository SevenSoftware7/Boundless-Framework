namespace SevenDev.Utility;

using System;
using System.Collections.Generic;


public class ComparisonComparer<T>(Comparison<T?> Comparison) : IComparer<T> {
	public int Compare(T? x, T? y) {
		return Comparison(x, y);
	}
}