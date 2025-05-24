using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A localized text that can append a value to the text
/// </summary>
public class LocalizedTextAdditive : LocalizedText
{
    [SerializeField] private bool isSuffix = true;
    [SerializeField] private string objectPrefix;
    [SerializeField] private string objectSuffix;
    private object value;

    /// <summary>
    /// Sets a new value
    /// </summary>
    /// <param name="newValue">The new value</param>
    /// <param name="reload">Should the text be reloaded ?</param>
    public void SetValue(object newValue, bool reload)
    {
        value = newValue;
        if (reload) ReloadText();
    }

    public override void ReloadText()
    {
        base.ReloadText();

        if (value == null) return;

        string stringValue = objectPrefix + value + objectSuffix;
        if (isSuffix) text.text += stringValue;
        else text.text = stringValue + text.text;
    }

    /// <summary>
    /// Changes the additive parameters
    /// </summary>
    /// <param name="isSuffix">Is the value a suffix or not</param>
    /// <param name="prefix">The value's prefix</param>
    /// <param name="suffix">The value's suffix</param>
    public void SetParameters(bool isSuffix, string prefix, string suffix)
    {
        this.isSuffix = isSuffix;
        objectPrefix = prefix;
        objectSuffix = suffix;
    }

    /// <summary>
    /// Reset the additive component
    /// </summary>
    public void ResetAdditive()
    {
        objectPrefix = "";
        objectSuffix = "";
        value = null;
    }
}
