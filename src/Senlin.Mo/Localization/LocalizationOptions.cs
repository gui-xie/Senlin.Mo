﻿namespace Senlin.Mo;

public class LocalizationOptions
{
    private string[] _cultures = ["en", "zh"];

    public string[] SupportedCultures
    {
        get => _cultures;
        set
        {
            if (value.Length > 0)
            {
                _cultures = value;
            }
        }
    }
}