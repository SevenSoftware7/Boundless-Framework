using System;
using System.Collections.Generic;

namespace SevenDev.Utility;

public class ComparisonComparer<T>(Comparison<T?> Comparison) : IComparer<T> {
	public int Compare(T? x, T? y) {
		return Comparison(x, y);
	}
}