using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace cjsonapi.Helpers
{
    class CJson
    {
        /*
          By Steve Hanov
          Released to the public domain
        */

        /* 
           Compress to convert from JSON to CJson
           Expand to convert from CJson to JSON.
        */

        /*
          *** Note: Translated to C# from CJson Javascript
        */
        public CJson()
        {

        }

        #region Compress
        public dynamic Compress(dynamic value)
        {
            Node root;
            dynamic templates;
            dynamic values;

            root = new Node(null, "");
            values = Process(root, value);
            values = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(values));
            templates = CreateTemplates(root);
            templates = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(templates));

            if (templates.Count > 0)
            {
                value = new { f = "cjson", t = templates, v = values };
                return value;
            }
            else // no templates, so no compression is possible.
            {
                return value;
            }
        }

        // Given the root of the key tree, process the value possibly adding to the // key tree.
        private dynamic Process(Node root, dynamic value)
        {
            dynamic result;
            dynamic i;
            Node node;

            Type valueType = value.GetType();
            if (valueType.Name == "JObject")
            {
                if (valueType.IsArray) // if it's an array
                {
                    ArrayList result1 = new ArrayList();

                    for (i = 0; i < value.Length; i++)
                    {
                        result1.Add(Process(root, value[i]));
                    }

                    result = result1;
                }
                else
                {
                    node = root;
                    IDictionary<string, object> result2 = new Dictionary<string, object>();
                    result2.Add("", new List<object>());

                    ArrayList resultList = new ArrayList();

                    // it's an object. For each key
                    foreach (var key in value)
                    {
                        if (value.ContainsKey(key.Name) == true)
                        {
                            node = node.Follow(key.Name);  // follow the node. 
                            resultList.Add(Process(root, value[key.Name])); // add its value to the array.
                        }
                    }

                    result2[""] = resultList;
                    node.links.Add(result2);
                    result = result2;
                }
            }
            else
            {
                result = value;
            }

            return result;
        }

        // Given the root of the key tree, return the array of template arrays.
        private ArrayList CreateTemplates(Node root)
        {
            ArrayList templates = new ArrayList();
            ArrayList queue = new ArrayList();
            dynamic node;
            ArrayList template = new ArrayList();
            dynamic cur;
            dynamic i;
            dynamic numChildren;

            root.templateIndex = 0;
            var keys = root.children.Keys;
            foreach (var key in keys)
            {
                queue.Add(root.children[key]);
            }

            // while queue not empty 
            while (queue.Count > 0)
            {
                // remove a node from the queue
                node = queue[0];
                queue.RemoveAt(0);

                numChildren = 0;

                // add its children to the queue. 
                var nodeKeys = node.children.Keys;
                foreach (var key in nodeKeys)
                {
                    queue.Add(node.children[key]);
                    numChildren += 1;
                }

                // if the node had more than one child, or it has links,
                int lenC = node.links.Count;
                if (numChildren > 1 || lenC > 0)
                {
                    cur = node;

                    // follow the path up from the node until one with a template
                    // id is reached.
                    while (cur.templateIndex == null)
                    {
                        template.Insert(0, cur.key);
                        cur = cur.parent;
                    }

                    template.Insert(0, cur.templateIndex);
                    templates.Add(template);
                    node.templateIndex = templates.Count;

                    int len = node.links.Count;
                    for (i = 0; i < len; i++)
                    {
                        if (i > node.links.Count)
                        {
                            var a = "error";
                        }

                        template.Insert(0, node.templateIndex);
                        node.links[i][""] = template;
                    }
                }
            }

            return templates;
        }
        #endregion Compress
    }

    class Node
    {
        public dynamic parent;
        public dynamic key;
        public IDictionary<string, object> children;
        public dynamic templateIndex;
        public List<object> links;

        public Node(dynamic parent, dynamic key)
        {
            this.parent = parent;
            this.key = key;
            this.children = new Dictionary<string, object>();
            this.templateIndex = null;
            this.links = new List<object>();
        }

        public Node Follow(dynamic key)
        {
            if (this.children != null && this.children.ContainsKey(key) == true)
            {
                return this.children[key];
            }
            else
            {
                this.children.Add(key, new Node(this, key));
                return this.children[key];
            }
        }
    }
}