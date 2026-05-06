using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace com.localizations.tmpro_font_generator.editor
{
	public static class GoogleImportedCallback
	{
		private static List<IGoogleImportedCallback> _callbacks;

		private static List<IGoogleImportedCallback> Callbacks
		{
			get
			{
				if (_callbacks != null)
				{
					return _callbacks;
				}

				_callbacks = new List<IGoogleImportedCallback>();
				var callbackTypes = GetAllImplementations(typeof(IGoogleImportedCallback));
				if (callbackTypes != null && callbackTypes.Count > 0)
				{
					foreach (var callbackType in callbackTypes)
					{
						IGoogleImportedCallback obj;
						try
						{
							obj = Activator.CreateInstance(callbackType) as IGoogleImportedCallback;
						}
						catch (Exception e)
						{
							Debug.LogException(e);
							continue;
						}

						if (obj == null)
							continue;

						_callbacks.Add(obj);
					}
				}

				return _callbacks;
			}
		}

		public static void OnImported(List<string> categories)
		{
			var callbacksList = Callbacks;
			if (callbacksList == null || callbacksList.Count == 0)
			{
				return;
			}

			foreach (var callback in callbacksList)
			{
				callback.OnImported(categories);
			}
		}

		private static List<Type> GetAllImplementations(Type interfaceType, Predicate<Type> filter = null)
		{
			Predicate<Type> subClassFilter = type => type.IsClass && !type.IsAbstract && interfaceType.IsAssignableFrom(type);
			return filter == null ? GetTypes(subClassFilter) : GetTypes(type => subClassFilter(type) && filter(type));
		}

		private static List<Type> GetTypes(Predicate<Type> filter = null)
		{
			var result = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				if (IsSystem(assembly))
				{
					continue;
				}

				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch
				{
					continue;
				}

				if (filter == null)
				{
					for (var i = 0; i < types.Length; i++)
					{
						result.Add(types[i]);
					}
				}
				else
				{
					for (var i = 0; i < types.Length; i++)
					{
						var type = types[i];
						if (filter(type))
						{
							result.Add(type);
						}
					}
				}
			}

			return result;
		}

		private static bool IsSystem(Assembly assembly)
		{
			var assemblyFullName = assembly.FullName;
			if (assemblyFullName.StartsWith("Unity", StringComparison.Ordinal)
				|| assemblyFullName.StartsWith("System", StringComparison.Ordinal)
				|| assemblyFullName.StartsWith("Mono", StringComparison.Ordinal)
				|| assemblyFullName.StartsWith("Accessibility", StringComparison.Ordinal)
				|| assemblyFullName.StartsWith("mscorlib", StringComparison.Ordinal)
			   )
			{
				return true;
			}

			return false;
		}
	}
}
