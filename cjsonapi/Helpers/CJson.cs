﻿using Newtonsoft.Json;
using System;

namespace Zenople.Application.WebApi.Helpers
{
    class CJson
    {
        /**
          By Steve Hanov
          Released to the public domain
        */

        /* 
           CJSON.stringify() to convert from objects to string
           CJSON.parse() to convert from string to objects.
           More documentation is pending.
        */

        /*
          ***Note: Translated to C# from CJson Javascript
        */
        public CJson()
        {

        }

        public dynamic Compress(dynamic value)
        {
            dynamic root;
            dynamic templates;
            dynamic values;

            root = new Node(null, "");
            values = ProcessValue(root, value);
            templates = CreateTemplates(root);

            if (templates.Length > 0)
            {
                value = "{ \"f\": \"cjson\", \"t\":" + templates + ", \"v\":" + values + "}";
                return JsonConvert.DeserializeObject(value);
            }
            else // no templates, so no compression is possible.
            {
                return JsonConvert.DeserializeObject(value);
            }
        }

        public dynamic Expand(dynamic json)
        {

            dynamic value;
            Type valueType = json.GetType();
            if (valueType.Name == "JObject")
            {
                value = json;
            }
            else
            {
                value = JsonConvert.DeserializeObject(json);
            }

            if (valueType.Name != "JObject" || (!value["f"] || value["f"] == null) || (value["f"] && value["f"] != "cjson")) // not in cjson format. Return as is.
            {
                return value;
            }

            return ProcessExpand(value["t"], value["v"]);
        }

        private dynamic ProcessExpand(dynamic templates, dynamic value)
        {
            dynamic result;
            dynamic i;
            dynamic keys;

            // if it's an array, then expand each element of the array.
            Type valueType = value.GetType();
            if ((valueType == typeof(object)) || (valueType.IsArray && value[0] == typeof(object)))
            {
                if (valueType.IsArray) // if it's an array
                {
                    result = Array.Empty<dynamic>(); // process each item in the array.
                    for (i = 0; i < value.Length; i++)
                    {
                        result.push(ProcessExpand(templates, value[i]));
                    }
                }
                else  // if it's an object, then recreate the keys from the template // and expand.
                {
                    result = JsonConvert.DeserializeObject("{}");
                    keys = GetKeys(templates, value[""][0]);
                    for (i = 0; i < keys.Length; i++)
                    {
                        result[keys[i]] = ProcessExpand(templates, value[""][i + 1]);
                    }
                }
            }
            else
            {
                result = value;
            }

            return result;
        }

        private dynamic GetKeys(dynamic templates, dynamic index)
        {
            dynamic keys = Array.Empty<dynamic>();

            while (index > 0)
            {
                keys = templates[index - 1].slice(1).concat(keys);
                index = templates[index - 1][0];
            }

            return keys;
        }

        // Given the root of the key tree, process the value possibly adding to the // key tree.
        private dynamic ProcessValue(dynamic root, dynamic value)
        {
            dynamic result;
            dynamic i;
            dynamic node;

            Type valueType = value.GetType();
            if (valueType.Name == "JObject")
            {
                if (valueType.IsArray) // if it's an array
                {
                    result = Array.Empty<dynamic>(); // process each item in the array.

                    for (i = 0; i < value.Length; i++)
                    {
                        result.push(ProcessValue(root, value[i]));
                    }
                }
                else
                {
                    node = root;
                    result = JsonConvert.DeserializeObject("{\"\":[]}");

                    // it's an object. For each key
                    foreach (var key in value)
                    {
                        if (value && value[key.Name] != null)
                        {
                            node = Follow(node, key.Name);  // follow the node. 
                            result[""].push(ProcessValue(root, value[key.Name])); // add its value to the array.
                        }
                    }
                    node.links.push(result);
                }
            }
            else
            {
                result = value;
            }

            return result;
        }

        private dynamic Follow(dynamic root, dynamic key)
        {
            if (root.children.ContainsKey(key))
            {
                return root.children[key];
            }
            else
            {
                root.children[key] = new Node(this, key);
                return root.children[key];
            }
        }

        // Given the root of the key tree, return the array of template arrays.
        private dynamic CreateTemplates(dynamic root)
        {
            dynamic templates = Array.Empty<dynamic>();
            dynamic queue = Array.Empty<dynamic>();
            dynamic node;
            dynamic template;
            dynamic cur;
            dynamic i;
            dynamic numChildren;

            root.templateIndex = 0;
            foreach (var key in root.children)
            {
                if (root && root.children[key.Name] != null)
                {
                    queue.push(root.children[key.Name]);
                }
            }

            // while queue not empty
            while (queue.Length > 0)
            { // remove a ode from the queue

                node = queue.shift();
                numChildren = 0;

                // add its children to the queue.
                foreach (var key in node.children)
                {
                    if (node && node.children[key.Name] != null)
                    {
                        queue.push(node.children[key.Name]);
                        numChildren += 1;
                    }
                }

                // if the node had more than one child, or it has links,
                if (numChildren > 1 || node.links.Length > 0)
                {
                    template = Array.Empty<dynamic>(); ;
                    cur = node;

                    // follow the path up from the node until one with a template
                    // id is reached.
                    while (cur.templateIndex == null)
                    {
                        template.unshift(cur.key);
                        cur = cur.parent;
                    }

                    template.unshift(cur.templateIndex);
                    templates.push(template);
                    node.templateIndex = templates.Length;

                    for (i = 0; i < node.links.Length; i++)
                    {
                        node.links[i][""].unshift(node.templateIndex);
                    }
                }
            }

            return templates;
        }
    }

    class Node
    {
        public dynamic parent;
        public dynamic key;
        public dynamic children = Array.Empty<dynamic>();
        public dynamic templateIndex;
        public dynamic links = Array.Empty<dynamic>();

        public Node(dynamic parent, dynamic key)
        {
            this.parent = parent;
            this.key = key;
            this.children = Array.Empty<dynamic>(); ;
            this.templateIndex = null;
            this.links = Array.Empty<dynamic>(); ;
        }
    }
}