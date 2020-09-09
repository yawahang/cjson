using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;

namespace cjsonapi.Controllers
{
    [ApiController]
    [EnableCors("AllowOrigin"), Route("[action]/{id?}")]
    public class WeatherForecastController : ControllerBase
    {
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
            var actualJson = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return actualJson;
        }
 
        [HttpGet]
        public IEnumerable<WeatherForecast> JsonToCJson()
        {
            var rng = new Random();
            var actualJson = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return Compress(actualJson);
        }

        // CJson Start 
        public dynamic Compress(dynamic value) {

            dynamic root;
            dynamic templates;
            dynamic values;
  
            root= new Node( null, "" );
            values= Process( root, value );
            templates = CreateTemplates( root );
            
            if ( templates.length > 0 ) {

                value = "{ \"f\": \"cjson\", \"t\":"+ templates + ", \"v\":"+ values +"}";
                return JsonConvert.DeserializeObject(value);
            } else { // no templates, so no compression is possible.

                return JsonConvert.DeserializeObject(value);
            }
        }
 
        // Given the root of the key tree, process the value possibly adding to the
        // key tree.
        public dynamic Process(dynamic root,dynamic value) {

            dynamic result;
            dynamic i;
            dynamic node;

            Type valueType = value.GetType(); 
            if ((valueType == typeof(object)) || (valueType.IsArray && value[0] == typeof(object))) {
               
                if (valueType.IsArray) {  // if it's an array
                   
                    result = Array.Empty<dynamic>(); // process each item in the array.

                    for( i = 0; i < value.length; i++ ) {
                        result.push(Process( root, value[i] )); 
                    }

                } else {

                    node = root;
                    result = "{\"\":[] }";
                    result = JsonConvert.DeserializeObject(value);
                  
                    // it's an object. For each key
                    foreach (string key in value) {

                        if (value && value[key] != null) {
                           
                            node = node.follow( key );  // follow the node.

                            result[""].push(Process(root,value[key]) ); // add its value to the array.
                        }
                    }

                    node.links.push(result);
                }
            } else {

                result = value;
            }

            return result;
        }

        // Given the root of the key tree, return the array of template arrays.
        public dynamic CreateTemplates(dynamic root) {

                dynamic templates = [];
                dynamic queue = [];
                dynamic node;
                dynamic template;
                dynamic cur;
                dynamic i;
                dynamic numChildren;

                root.templateIndex = 0;

                foreach (string key in root.children) {

                    if (root && root.children[key] != null) {
                        queue.push( root.children[key] );
                    }
                }

                // while queue not empty
                while( queue.length > 0 ) { // remove a ode from the queue

                    node = queue.shift();
                    numChildren = 0;

                    // add its children to the queue.
                    foreach (string key in node.children ) {

                        if (node && node.children[k] != null) {

                            queue.push( node.children[key] );
                            numChildren += 1;
                        }
                    }

                    // if the node had more than one child, or it has links,
                    if ( numChildren > 1 || node.links.length > 0 ) {
                        template = [];
                        cur = node;

                        // follow the path up from the node until one with a template
                        // id is reached.
                        while( cur.templateIndex === null ) {

                            template.unshift( cur.key );
                            cur = cur.parent;
                        }

                        template.unshift( cur.templateIndex );

                        templates.push( template );
                        node.templateIndex = templates.length;

                        for( i = 0; i < node.links.length; i++ ) {
                            node.links[i][""].unshift( node.templateIndex );
                        }
                    }
                }

                return templates;
            }
        // CJson End 
    }
 
    internal class Node
    {
        dynamic parent;
        dynamic key;
        dynamic children = Array.Empty<dynamic>();
        dynamic templateIndex = null;
        dynamic links = Array.Empty<dynamic>();;

        public Node(dynamic parent, dynamic key)
        {
            this.parent = parent;
            this.key = key;
            this.children = Array.Empty<dynamic>();;
            this.templateIndex = null;
            this.links = Array.Empty<dynamic>();;
        }
    }
}
