using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class FuckIL2CPP
{
    [ModuleInitializer]
    internal static void Init()
    {
        string path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "MelonLoader",
            "Il2CppAssemblies",
            "UnityEngine.CoreModule.dll"
        );

        if (!File.Exists(path))
            return;

        try
        {
            var cecil = AppDomain
                .CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Mono.Cecil");

            if (cecil == null)
            {
                string[] searchPaths =
                {
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "MelonLoader",
                        "net6",
                        "Mono.Cecil.dll"
                    ),
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "MelonLoader",
                        "Dependencies",
                        "Mono.Cecil.dll"
                    ),
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "MelonLoader",
                        "Mono.Cecil.dll"
                    ),
                };
                string cecilPath = searchPaths.FirstOrDefault(File.Exists);
                if (cecilPath == null)
                    return;
                cecil = Assembly.LoadFrom(cecilPath);
            }

            var assDefType = cecil.GetType("Mono.Cecil.AssemblyDefinition");
            var readerParamsType = cecil.GetType("Mono.Cecil.ReaderParameters");
            var readingModeType = cecil.GetType("Mono.Cecil.ReadingMode");

            object readerParams = Activator.CreateInstance(readerParamsType);
            var readingModeProp = readerParamsType.GetProperty("ReadMode");
            if (readingModeProp != null)
            {
                var immediateVal = Enum.Parse(readingModeType, "Immediate");
                readingModeProp.SetValue(readerParams, immediateVal);
            }

            byte[] fileBytes = File.ReadAllBytes(path);
            using var memStream = new MemoryStream(fileBytes);

            var readMethod = assDefType.GetMethod(
                "ReadAssembly",
                new[] { typeof(Stream), readerParamsType }
            );
            var assembly = readMethod.Invoke(null, new object[] { memStream, readerParams });

            try
            {
                var module = assDefType.GetProperty("MainModule").GetValue(assembly);
                var topTypes = (IEnumerable)module.GetType().GetProperty("Types").GetValue(module);

                int fixedCount = 0;
                var seenFullNames = new HashSet<string>();

                void ProcessTypeRecursive(object type)
                {
                    var nameProp = type.GetType().GetProperty("Name");
                    var nsProp = type.GetType().GetProperty("Namespace");

                    string name = (string)nameProp.GetValue(type);
                    string ns = (string)nsProp?.GetValue(type) ?? "";
                    string fullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";

                    if (name == "<>O" || name.StartsWith("<>O"))
                    {
                        if (seenFullNames.Contains(fullName))
                        {
                            fixedCount++;
                            string newName = $"<>O_Fix_{fixedCount}";
                            nameProp.SetValue(type, newName);
                            Console.WriteLine(
                                $"[FuckIL2CPP] Renamed duplicate '{fullName}' -> '{newName}'"
                            );
                        }
                        else
                        {
                            seenFullNames.Add(fullName);
                        }
                    }

                    var nestedProp = type.GetType().GetProperty("NestedTypes");
                    if (nestedProp != null)
                    {
                        var nestedList = (IEnumerable)nestedProp.GetValue(type);
                        if (nestedList != null)
                        {
                            foreach (var nested in nestedList.Cast<object>().ToList())
                            {
                                ProcessTypeRecursive(nested);
                            }
                        }
                    }
                }

                foreach (var type in topTypes.Cast<object>().ToList())
                {
                    ProcessTypeRecursive(type);
                }

                if (fixedCount > 0)
                {
                    var writeMethod = assDefType.GetMethod("Write", new[] { typeof(string) });
                    writeMethod.Invoke(assembly, new object[] { path });
                    Console.WriteLine($"[FuckIL2CPP] SUCCESS: Fixed {fixedCount} duplicate types!");
                }
            }
            finally
            {
                (assembly as IDisposable)?.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FuckIL2CPP] ERROR: {ex.Message}");
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute { }
}
