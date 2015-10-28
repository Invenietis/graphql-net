using System.Collections.Generic;

namespace GraphQL.Net
{
    public class Query
    {
        public string Name { get; set; }
        public List<Input> Inputs { get; set; }
        public List<Field> Fields { get; set; }
        public List<Fragment> Fragments { get; set; }
    }

    public class Fragment
    {
        public string Name { get; set; }

        public string AppliedEntity { get; set; }

        public List<Field> Fields { get; set; }
    }

    public class Field
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public List<Field> Fields { get; set; }
        public string FragmentRef { get; set; }
        public Fragment FragmentInline { get; set; }
    }

    public class Input
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}