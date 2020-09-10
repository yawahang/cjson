using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using cjsonapi.Controllers;
using cjsonapi.Helpers;

namespace cjsonapi.Controllers
{
    [ApiController]
    [EnableCors("AllowOrigin"), Route("[action]/{id?}")]
    public class WeatherForecastController : ControllerBase
    { 
        private readonly CJson _cJson = new CJson();
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> GetJson()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
 
        [HttpGet]
        public IEnumerable<WeatherForecast> CJsonToJson()
        {
            var rng = new Random();
            dynamic actualJson = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
 
             actualJson = _cJson.Compress(actualJson); // create CJson
             actualJson = _cJson.Expand(actualJson); // Expand CJson to JSON
             return actualJson;
        }
 
        [HttpGet]
        public IEnumerable<WeatherForecast> JsonToCJson()
        {
            var rng = new Random();
            dynamic actualJson = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

             actualJson = _cJson.Compress(actualJson);  // create CJson
             return actualJson;
        }
    }
}
