using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extends Dropdown.OptionData so that this option can be given a Tag. The Tag
/// can be used to help identify a corresponding object. In the example, it the
/// Tag is used to store the Id of a SavedGame document.
/// </summary>
public class OptionDataExt : Dropdown.OptionData
{
    public object Tag { get; set; }

    public OptionDataExt(object tag = null)
    {
        Tag = tag;
    }

    public OptionDataExt(string text, object tag = null) : base(text)
    {
        Tag = tag;
    }

    public OptionDataExt(Sprite image, object tag = null) : base(image)
    {
        Tag = tag;
    }

    public OptionDataExt(string text, Sprite image, object tag = null) : base(text, image)
    {
        Tag = tag;
    }
}

