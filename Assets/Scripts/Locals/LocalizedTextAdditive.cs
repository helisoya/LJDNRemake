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
    /// Reset the additive component
    /// </summary>
    public void ResetAdditive()
    {
        objectPrefix = "";
        objectSuffix = "";
        value = null;
    }
}
