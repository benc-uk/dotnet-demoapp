using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_demoapp
{
    public class WeatherData
    {
        public Dictionary<string, object> currently { get; set; }
    }
}
