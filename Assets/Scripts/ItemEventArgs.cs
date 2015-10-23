using System;

public class ItemEventArgs : EventArgs
{
    public string itemName;
    public bool get;

    /// <summary>
    /// Event options for getting an item
    /// </summary>
    /// <param name="itemName">The item name</param>
    public ItemEventArgs(string itemName)
    {
        this.itemName = itemName;
        get = true;
    }
    /// <summary>
    /// Event options for getting an item
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <param name="get">Is it getting or dropping the item?</param>
    public ItemEventArgs(string itemName, bool get)
    {
        this.itemName = itemName;
        this.get = get;
    }
}
