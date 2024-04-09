using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Metrino.Development.UI.Core;

public static class Utilities
{
    public static string CreateFolder(string[] pathElements)
    {
        var path = Path.Combine(pathElements);

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        return path;
    }

    public static IEnumerable<Type> GetAllDerivedTypes<T>(bool includeAllAssemblies = true)
    {
        var type = typeof(T);

        List<Type> derivedTypes = GetAllDerivedTypes<T>(Assembly.GetAssembly(type)).ToList();

        if (includeAllAssemblies)
        {
            IEnumerable<Assembly> dependentAssemblies = GetDependentAssemblies(Assembly.GetAssembly(type));

            derivedTypes.AddRange(dependentAssemblies.SelectMany(a => GetAllDerivedTypes<T>(a)));
        }

        return derivedTypes;
    }

    private static IEnumerable<Assembly> GetDependentAssemblies(Assembly analyzedAssembly)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Where(aAssembly =>
        {
            return aAssembly.GetReferencedAssemblies()
                            .Select(a => a.FullName)
                            .Contains(analyzedAssembly.FullName);
        });
    }

    private static IEnumerable<Type> GetAllDerivedTypes<T>(Assembly assembly)
    {
        var baseType = typeof(T);
        TypeInfo baseTypeInfo = baseType.GetTypeInfo();

        IEnumerable<TypeInfo> definedTypes;
        try
        {
            definedTypes = assembly.DefinedTypes;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            definedTypes = new TypeInfo[0];
        }

        return definedTypes.Where(aType =>
        {
            if (baseTypeInfo.IsClass)
                return aType.IsSubclassOf(baseType);

            return baseTypeInfo.IsInterface && aType.ImplementedInterfaces.Contains(baseTypeInfo.AsType());

        }).Select(aType => aType.AsType());
    }

    //public static string FiberCodeToString(eFiberCode fiberCode)
    //{
    //    int fiberCodeIndex = (int)fiberCode;
    //    string[] codes = new string[] { "Unknown", "A", "B", "C", "D", "E", "F", "G" };
    //    string[] diameters = new string[] { "N/A", "8/125 \u03bcm", "9/125 \u03bcm", "50/125 \u03bcm", "62.5/125 \u03bcm", "100/140 \u03bcm", "5/125 \u03bcm", "5/125 \u03bcm" };

    //    return $"{codes[fiberCodeIndex]} ({diameters[fiberCodeIndex]})";
    //}
}
