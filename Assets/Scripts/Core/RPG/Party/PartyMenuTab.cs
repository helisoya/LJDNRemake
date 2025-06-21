using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a tab in the party menu
/// </summary>
public class PartyMenuTab : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject root;
    public bool open { get { return root.activeInHierarchy; } }

    /// <summary>
    /// Opens the tab
    /// </summary>
    public void Open()
    {
        root.SetActive(true);
        OnOpen();
    }

    /// <summary>
    /// Closes the tab
    /// </summary>
    public void Close()
    {
        root.SetActive(false);
        OnClose();
    }

    /// <summary>
    /// On Open Callback
    /// </summary>
    protected virtual void OnOpen()
    {

    }

    /// <summary>
    /// On Close Callback
    /// </summary>
    protected virtual void OnClose()
    {

    }
}
