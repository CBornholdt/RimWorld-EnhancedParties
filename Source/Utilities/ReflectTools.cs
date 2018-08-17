using System;
using System.Reflection;
using RimWorld;
using Verse.AI;
using EnhancedParty;
using System.Collections.Generic;

namespace EnhancedParty
{
	static public class ReflectTools
	{
		static public Delegate CreateDelegateOrFallback(Type type, string methodName, Type fallback)
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
															| BindingFlags.Instance);
			if (method == null) {
				method = fallback.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
															| BindingFlags.Instance);
				return Delegate.CreateDelegate(fallback, method);
			}
			return Delegate.CreateDelegate(type, method);               
		}
	}
}
