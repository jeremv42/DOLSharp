using System;
using System.Collections.Generic;

namespace DOL.GS;

public static class EnumerableExts
{
	public static void Foreach<T>(this IEnumerable<T> self, Action<T> function)
	{
		foreach (var e in self)
			function(e);
	}
}
