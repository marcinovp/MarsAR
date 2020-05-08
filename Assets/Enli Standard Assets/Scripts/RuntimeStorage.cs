using UnityEngine;
using System.Collections;

public static class RuntimeStorage
{
    private static Hashtable values;
    
    /// <summary>
    /// Set value of item with "name". Create new item if no such item already exists
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <param name="value">New value of item</param>
    public static void SetValue(string name, object value)
    {
        if (values == null)
            values = new Hashtable();
        if (!values.ContainsKey(name))
            values.Add(name, value);
        else
            values[name] = value;
    }

    /// <summary>
    /// Retrieve stored data
    /// </summary>
    /// <param name="name">The name of the desired item</param>
    /// <param name="defaultValue">The value to be returned if no value of the desired type is stored with the given name</param>
    /// <returns>The value of an item that was previously added with SetValue(), or the default value if none was found.</returns>
    public static string GetValueString(string name, string defaultValue)
    {
        if (values == null || !values.ContainsKey(name))
            return defaultValue;
        return (string)values[name];
    }

    /// <summary>
    /// Retrieve stored data
    /// </summary>
    /// <param name="name">The name of the desired item</param>
    /// <param name="defaultValue">The value to be returned if no value of the desired type is stored with the given name</param>
    /// <returns>The value of an item that was previously added with SetValue(), or the default value if none was found.</returns>
    public static int GetValueInteger(string name, int defaultValue)
    {
        if (values == null || !values.ContainsKey(name))
            return defaultValue;
        return (int)values[name];
    }

    /// <summary>
    /// Retrieve stored data
    /// </summary>
    /// <param name="name">The name of the desired item</param>
    /// <param name="defaultValue">The value to be returned if no value of the desired type is stored with the given name</param>
    /// <returns>The value of an item that was previously added with SetValue(), or the default value if none was found.</returns>
    public static float GetValueFloat(string name, float defaultValue)
    {
        if (values == null || !values.ContainsKey(name))
            return defaultValue;
        return (float)values[name];
    }

    /// <summary>
    /// Retrieve stored data
    /// </summary>
    /// <param name="name">The name of the desired item</param>
    /// <param name="defaultValue">The value to be returned if no value of the desired type is stored with the given name</param>
    /// <returns>The value of an item that was previously added with SetValue(), or the default value if none was found.</returns>
    public static double GetValueDouble(string name, double defaultValue)
    {
        if (values == null || !values.ContainsKey(name))
            return defaultValue;
        return (double)values[name];
    }

    /// <summary>
    /// Retrieve stored data
    /// </summary>
    /// <param name="name">The name of the desired item</param>
    /// <param name="defaultValue">The value to be returned if no value of the desired type is stored with the given name</param>
    /// <returns>The value of an item that was previously added with SetValue(), or the default value if none was found.</returns>
    public static bool GetValueBoolean(string name, bool defaultValue)
    {
        if (values == null || !values.ContainsKey(name))
            return defaultValue;
        return (bool)values[name];
    }

    /// <summary>
    /// Retrieve stored data
    /// </summary>
    /// <param name="name">The name of the desired item</param>
    /// <param name="defaultValue">The value to be returned if no value of the desired type is stored with the given name</param>
    /// <returns>The value of an item that was previously added with SetValue(), or the default value if no such item is present</returns>
    public static object GetValue(string name, object defaultValue)
    {
        if (values == null || !values.ContainsKey(name))
            return defaultValue;
        return values[name];
    }

    public static bool ContainsKey(string key)
    {
        if (values == null)
            return false;
        else
            return values.ContainsKey(key);
    }

    public static bool ContainsValue(object value)
    {
        if (values == null)
            return false;
        else
            return values.ContainsValue(values);
    }
}
